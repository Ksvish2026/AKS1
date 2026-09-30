using AksTyreProduction.Web.Data;
using AksTyreProduction.Web.Models;
using AksTyreProduction.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace AksTyreProduction.Tests;

public class ProductionRulesTests
{
    private static AppDbContext Database()
    {
        var options=new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(options);
    }
    private static async Task<(AppDbContext db,ProductionService service,Tyre tyre,RetreadJob job,Operator op,Material mat)> Setup()
    {
        var db=Database();var customer=new Customer{Name="Test Fleet",AccountNumber="T-1"};var op=new Operator{EmployeeId="O1",Name="Tester",Role="Operator",LabourRate=150m};var mat=new Material{MaterialId="M1",Name="Rubber",UnitOfMeasure="kg",CurrentUnitCost=62.59m,QuantityOnHand=100m};db.AddRange(customer,op,mat);await db.SaveChangesAsync();var tyre=new Tyre{AksTyreId="GTC269999",CustomerId=customer.Id,Brand="Michelin",Size="11R22.5",SerialNumber="TEST1"};db.Tyres.Add(tyre);await db.SaveChangesAsync();var job=new RetreadJob{TyreId=tyre.Id,RetreadNumber=1,JobNumber="JOB-T1",ReceivedAt=DateTime.Now,CurrentStage="Initial Inspection"};db.RetreadJobs.Add(job);await db.SaveChangesAsync();return(db,new ProductionService(db),tyre,job,op,mat);
    }

    [Fact] public async Task CreatingTyreAlsoCreatesFirstRetread()
    {var db=Database();var c=new Customer{Name="Fleet",AccountNumber="F1"};db.Add(c);await db.SaveChangesAsync();var tyre=await new ProductionService(db).CreateTyreAsync("Michelin","11R22.5","SER1",c.Id,"JOB1");Assert.StartsWith("GTC26",tyre.AksTyreId);Assert.Equal(1,await db.RetreadJobs.CountAsync(x=>x.TyreId==tyre.Id));}

    [Fact] public async Task ReturningTyreGetsNextRetreadAndHistoryRemains()
    {var x=await Setup();x.job.Status=JobStatus.Dispatched;await x.db.SaveChangesAsync();var second=await x.service.StartRetreadAsync(x.tyre.Id,"JOB2");Assert.Equal(2,second.RetreadNumber);Assert.Equal(2,await x.db.RetreadJobs.CountAsync());Assert.Equal("JOB-T1",(await x.db.RetreadJobs.SingleAsync(j=>j.RetreadNumber==1)).JobNumber);}

    [Fact] public async Task StationCannotBeSkipped()
    {var x=await Setup();await Assert.ThrowsAsync<InvalidOperationException>(()=>x.service.StartStationAsync(x.job.Id,"Buffing",x.op.Id,null));Assert.Empty(await x.db.StationTransactions.ToListAsync());Assert.Equal("Initial Inspection",x.job.CurrentStage);}

    [Fact] public async Task StationMustMatchNextWorkflowStage()
    {var x=await Setup();var first=await x.service.StartStationAsync(x.job.Id,"Initial Inspection",x.op.Id,null);await x.service.CompleteStationAsync(first.Id,10,TransactionResult.Pass,"",null);Assert.Equal("Shearography / NDT",x.job.CurrentStage);await Assert.ThrowsAsync<InvalidOperationException>(()=>x.service.StartStationAsync(x.job.Id,"Buffing",x.op.Id,null));await x.service.StartStationAsync(x.job.Id,"Shearography / NDT",x.op.Id,null);}

    [Fact] public async Task LabourUsesCapturedRateAndCorrectDecimalCalculation()
    {var x=await Setup();x.job.CurrentStage="Buffing";var tx=await x.service.StartStationAsync(x.job.Id,"Buffing",x.op.Id,null);await x.service.CompleteStationAsync(tx.Id,95,TransactionResult.Completed,"",null);x.op.LabourRate=175m;await x.db.SaveChangesAsync();Assert.Equal(150m,tx.LabourRateSnapshot);Assert.Equal(237.50m,tx.LabourCost);}

    [Fact] public async Task MaterialUsesHistoricalPriceAndReducesInventory()
    {var x=await Setup();var usage=await x.service.AddMaterialAsync(x.job.Id,x.mat.Id,8m,null,null);x.mat.CurrentUnitCost=100m;await x.db.SaveChangesAsync();Assert.Equal(62.59m,usage.UnitCostSnapshot);Assert.Equal(500.72m,usage.TotalCost);Assert.Equal(92m,x.mat.QuantityOnHand);}

    [Fact] public async Task TotalCostComesFromUnderlyingTransactions()
    {var x=await Setup();x.job.BaseCost=100m;x.job.CurrentStage="Buffing";var tx=await x.service.StartStationAsync(x.job.Id,"Buffing",x.op.Id,null);await x.service.CompleteStationAsync(tx.Id,60,TransactionResult.Completed,"",null);await x.service.AddMaterialAsync(x.job.Id,x.mat.Id,2m,null,null);Assert.Equal(375.18m,x.job.TotalCost);}

    [Fact] public async Task FailedInspectionRemainsWhenLaterAttemptPasses()
    {var x=await Setup();x.job.CurrentStage="Final Inspection";var first=await x.service.StartStationAsync(x.job.Id,"Final Inspection",x.op.Id,null);await x.service.CompleteStationAsync(first.Id,10,TransactionResult.Fail,"Defect","Bonding void");x.job.CurrentStage="Final Inspection";var second=await x.service.StartStationAsync(x.job.Id,"Final Inspection",x.op.Id,null);await x.service.CompleteStationAsync(second.Id,12,TransactionResult.Pass,"Rework accepted",null);Assert.Equal(2,await x.db.StationTransactions.CountAsync(x=>x.Station=="Final Inspection"));Assert.Contains(await x.db.StationTransactions.ToListAsync(),t=>t.Result==TransactionResult.Fail);}

    [Fact] public async Task QcRequiresPassedFinalInspection()
    {var x=await Setup();x.job.CurrentStage="Final Inspection";await Assert.ThrowsAsync<InvalidOperationException>(()=>x.service.ReleaseQcAsync(x.job.Id,x.op.Id));}

    [Fact] public async Task QcRejectsEarlierPassWhenLatestFinalInspectionFails()
    {var x=await Setup();x.job.CurrentStage="Final Inspection";var first=await x.service.StartStationAsync(x.job.Id,"Final Inspection",x.op.Id,null);await x.service.CompleteStationAsync(first.Id,10,TransactionResult.Pass,"Accepted"," ");x.job.CurrentStage="Final Inspection";var latest=await x.service.StartStationAsync(x.job.Id,"Final Inspection",x.op.Id,null);await x.service.CompleteStationAsync(latest.Id,10,TransactionResult.Fail,"Defect","Bonding void");await Assert.ThrowsAsync<InvalidOperationException>(()=>x.service.ReleaseQcAsync(x.job.Id,x.op.Id));Assert.Equal(JobStatus.InProduction,x.job.Status);}

    [Fact] public async Task DispatchRequiresQcRelease()
    {var x=await Setup();await Assert.ThrowsAsync<InvalidOperationException>(()=>x.service.DispatchAsync(x.job.Id,"D1","Delivery",""));}

    [Fact] public async Task ScrappedTyreCannotStartAnotherRetread()
    {var x=await Setup();x.job.Status=JobStatus.Dispatched;await x.db.SaveChangesAsync();await x.service.ScrapAsync(x.tyre.Id,"End of life");await Assert.ThrowsAsync<InvalidOperationException>(()=>x.service.StartRetreadAsync(x.tyre.Id,"JOB2"));}

    [Fact] public void CustomerPortalContractContainsNoCostFields()
    {var names=typeof(CustomerProgressDto).GetProperties().Select(x=>x.Name).ToList();Assert.DoesNotContain(names,n=>n.Contains("Cost")||n.Contains("Rate")||n.Contains("Margin")||n.Contains("Price"));}

    [Fact] public async Task QcPassedAndReadyForDispatchAreSeparateAuditedStates()
    {var x=await Setup();x.job.CurrentStage="Final Inspection";var tx=await x.service.StartStationAsync(x.job.Id,"Final Inspection",x.op.Id,null);await x.service.CompleteStationAsync(tx.Id,12,TransactionResult.Pass,"Accepted",null);await x.service.ReleaseQcAsync(x.job.Id,x.op.Id);Assert.Equal(JobStatus.QcPassed,x.job.Status);Assert.NotNull(x.job.QcPassedAt);await x.service.MarkReadyForDispatchAsync(x.job.Id);Assert.Equal(JobStatus.ReadyForDispatch,x.job.Status);Assert.NotNull(x.job.ReadyForDispatchAt);}

    [Fact] public async Task DraftInvoiceSnapshotsCalculatedSellingPrice()
    {var x=await Setup();x.job.Status=JobStatus.QcPassed;x.job.BaseCost=1000m;x.job.MarkupPercent=25m;await x.db.SaveChangesAsync();var invoice=await x.service.CreateInvoiceAsync(x.job.Id,"INV-TEST");Assert.Equal(1000m,invoice.ProductionCostSnapshot);Assert.Equal(1250m,invoice.SellingPriceSnapshot);}

    [Fact] public void BarcodeRendererProducesPermanentIdAsSvg()
    {var svg=Code39Barcode.RenderSvg("GTC260001");Assert.Contains("<svg",svg);Assert.Contains("GTC260001",svg);Assert.Contains("<rect",svg);}
}
