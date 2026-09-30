using AksTyreProduction.Web.Controllers;
using AksTyreProduction.Web.Data;
using AksTyreProduction.Web.Models;
using AksTyreProduction.Web.Services;
using Microsoft.AspNetCore.Mvc;
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
    private static Task Complete(ProductionService service,StationTransaction transaction,int durationMinutes,TransactionResult result,string notes,string? failureReason)
        =>service.CompleteStationAsync(transaction.Id,result,notes,failureReason,transaction.StartedAt.AddMinutes(durationMinutes));

    [Fact] public async Task CreatingTyreAlsoCreatesFirstRetread()
    {var db=Database();var c=new Customer{Name="Fleet",AccountNumber="F1"};db.Add(c);await db.SaveChangesAsync();var tyre=await new ProductionService(db).CreateTyreAsync("Michelin","11R22.5","SER1",c.Id,"JOB1");Assert.StartsWith("GTC26",tyre.AksTyreId);Assert.Equal(1,await db.RetreadJobs.CountAsync(x=>x.TyreId==tyre.Id));}

    [Fact] public async Task ReturningTyreGetsNextRetreadAndHistoryRemains()
    {var x=await Setup();x.job.Status=JobStatus.Dispatched;await x.db.SaveChangesAsync();var second=await x.service.StartRetreadAsync(x.tyre.Id,"JOB2");Assert.Equal(2,second.RetreadNumber);Assert.Equal(2,await x.db.RetreadJobs.CountAsync());Assert.Equal("JOB-T1",(await x.db.RetreadJobs.SingleAsync(j=>j.RetreadNumber==1)).JobNumber);}

    [Fact] public async Task StationCannotBeSkipped()
    {var x=await Setup();await Assert.ThrowsAsync<InvalidOperationException>(()=>x.service.StartStationAsync(x.job.Id,"Buffing",x.op.Id,null));Assert.Empty(await x.db.StationTransactions.ToListAsync());Assert.Equal("Initial Inspection",x.job.CurrentStage);}

    [Fact] public async Task StationMustMatchNextWorkflowStage()
    {var x=await Setup();var first=await x.service.StartStationAsync(x.job.Id,"Initial Inspection",x.op.Id,null);await Complete(x.service,first,10,TransactionResult.Pass,"",null);Assert.Equal("Shearography / NDT",x.job.CurrentStage);await Assert.ThrowsAsync<InvalidOperationException>(()=>x.service.StartStationAsync(x.job.Id,"Buffing",x.op.Id,null));await x.service.StartStationAsync(x.job.Id,"Shearography / NDT",x.op.Id,null);}

    [Fact] public async Task LabourUsesCapturedRateAndCorrectDecimalCalculation()
    {var x=await Setup();x.job.CurrentStage="Buffing";var tx=await x.service.StartStationAsync(x.job.Id,"Buffing",x.op.Id,null);var completedAt=tx.StartedAt.AddMinutes(95);await x.service.CompleteStationAsync(tx.Id,TransactionResult.Completed,"",null,completedAt);x.op.LabourRate=175m;await x.db.SaveChangesAsync();Assert.Equal(completedAt,tx.EndedAt);Assert.Equal(95,tx.DurationMinutes);Assert.Equal(150m,tx.LabourRateSnapshot);Assert.Equal(237.50m,tx.LabourCost);}

    [Fact] public async Task MaterialUsesHistoricalPriceAndReducesInventory()
    {var x=await Setup();var usage=await x.service.AddMaterialAsync(x.job.Id,x.mat.Id,8m,null,null);x.mat.CurrentUnitCost=100m;await x.db.SaveChangesAsync();Assert.Equal(62.59m,usage.UnitCostSnapshot);Assert.Equal(500.72m,usage.TotalCost);Assert.Equal(92m,x.mat.QuantityOnHand);}

    [Fact] public async Task TotalCostComesFromUnderlyingTransactions()
    {var x=await Setup();x.job.BaseCost=100m;x.job.CurrentStage="Buffing";var tx=await x.service.StartStationAsync(x.job.Id,"Buffing",x.op.Id,null);await Complete(x.service,tx,60,TransactionResult.Completed,"",null);await x.service.AddMaterialAsync(x.job.Id,x.mat.Id,2m,null,null);Assert.Equal(375.18m,x.job.TotalCost);}

    [Fact] public async Task FailedInspectionRemainsWhenLaterAttemptPasses()
    {var x=await Setup();x.job.CurrentStage="Final Inspection";var first=await x.service.StartStationAsync(x.job.Id,"Final Inspection",x.op.Id,null);await Complete(x.service,first,10,TransactionResult.Fail,"Defect","Bonding void");x.job.CurrentStage="Final Inspection";var second=await x.service.StartStationAsync(x.job.Id,"Final Inspection",x.op.Id,null);await Complete(x.service,second,12,TransactionResult.Pass,"Rework accepted",null);Assert.Equal(2,await x.db.StationTransactions.CountAsync(x=>x.Station=="Final Inspection"));Assert.Contains(await x.db.StationTransactions.ToListAsync(),t=>t.Result==TransactionResult.Fail);}

    [Fact] public async Task QcRequiresPassedFinalInspection()
    {var x=await Setup();x.job.CurrentStage="Final Inspection";await Assert.ThrowsAsync<InvalidOperationException>(()=>x.service.ReleaseQcAsync(x.job.Id,x.op.Id));}

    [Fact] public async Task AnyJobCanBeReopenedToItsCorrectStageWhenPreviousResultWasIncorrect()
    {
        var x = await Setup();
        x.job.CurrentStage = "QC Release";
        x.job.Status = JobStatus.InProduction;
        x.db.StationTransactions.Add(new StationTransaction
        {
            RetreadJobId = x.job.Id,
            Station = "Final Inspection",
            OperatorId = x.op.Id,
            StartedAt = DateTime.Now.AddMinutes(-20),
            EndedAt = DateTime.Now.AddMinutes(-10),
            DurationMinutes = 10,
            LabourRateSnapshot = x.op.LabourRate,
            LabourCost = 25m,
            Result = TransactionResult.Completed,
            Notes = "Wrong previous result"
        });
        await x.db.SaveChangesAsync();

        await x.service.ReopenJobAsync(x.job.Id, x.op.Id, "Final Inspection");
        var reopened = await x.service.StartStationAsync(x.job.Id, "Final Inspection", x.op.Id, null);

        Assert.Equal(JobStatus.InProduction, x.job.Status);
        Assert.Equal("Final Inspection", x.job.CurrentStage);
        Assert.Equal("Final Inspection", reopened.Station);
    }

    [Fact] public async Task QcRejectsEarlierPassWhenLatestFinalInspectionFails()
    {var x=await Setup();x.job.CurrentStage="Final Inspection";var first=await x.service.StartStationAsync(x.job.Id,"Final Inspection",x.op.Id,null);await Complete(x.service,first,10,TransactionResult.Pass,"Accepted",null);x.job.CurrentStage="Final Inspection";var latest=await x.service.StartStationAsync(x.job.Id,"Final Inspection",x.op.Id,null);await Complete(x.service,latest,10,TransactionResult.Fail,"Defect","Bonding void");await Assert.ThrowsAsync<InvalidOperationException>(()=>x.service.ReleaseQcAsync(x.job.Id,x.op.Id));Assert.Equal(JobStatus.InProduction,x.job.Status);}

    [Fact] public async Task DispatchRequiresQcRelease()
    {var x=await Setup();await Assert.ThrowsAsync<InvalidOperationException>(()=>x.service.DispatchAsync(x.job.Id,"D1","Delivery",""));}

    [Fact] public async Task ScrappedTyreCannotStartAnotherRetread()
    {var x=await Setup();x.job.Status=JobStatus.Dispatched;await x.db.SaveChangesAsync();await x.service.ScrapAsync(x.tyre.Id,"End of life");await Assert.ThrowsAsync<InvalidOperationException>(()=>x.service.StartRetreadAsync(x.tyre.Id,"JOB2"));}

    [Fact] public void CustomerPortalContractContainsNoCostFields()
    {var names=typeof(CustomerProgressDto).GetProperties().Select(x=>x.Name).ToList();Assert.DoesNotContain(names,n=>n.Contains("Cost")||n.Contains("Rate")||n.Contains("Margin")||n.Contains("Price"));}

    [Fact] public async Task QcPassedAndReadyForDispatchAreSeparateAuditedStates()
    {var x=await Setup();x.job.CurrentStage="Final Inspection";var tx=await x.service.StartStationAsync(x.job.Id,"Final Inspection",x.op.Id,null);await Complete(x.service,tx,12,TransactionResult.Pass,"Accepted",null);await x.service.ReleaseQcAsync(x.job.Id,x.op.Id);Assert.Equal(JobStatus.QcPassed,x.job.Status);Assert.NotNull(x.job.QcPassedAt);Assert.Equal(x.op.Id,x.job.QcOperatorId);Assert.Equal(x.op.Name,x.job.QcOperatorNameSnapshot);await x.service.MarkReadyForDispatchAsync(x.job.Id);Assert.Equal(JobStatus.ReadyForDispatch,x.job.Status);Assert.NotNull(x.job.ReadyForDispatchAt);}

    [Fact] public async Task DraftInvoiceSnapshotsCalculatedSellingPrice()
    {var x=await Setup();x.job.Status=JobStatus.QcPassed;x.job.BaseCost=1000m;x.job.MarkupPercent=25m;await x.db.SaveChangesAsync();var invoice=await x.service.CreateInvoiceAsync(x.job.Id,"INV-TEST");Assert.Equal(1000m,invoice.ProductionCostSnapshot);Assert.Equal(1250m,invoice.SellingPriceSnapshot);}

    [Fact] public void SelectedRetreadDefaultsToLatestHistoryEntryAndHonorsPreviousChoice()
    {
        var tyre = new Tyre
        {
            Id = 9,
            AksTyreId = "GTC260099",
            Brand = "Michelin",
            Size = "11R22.5",
            SerialNumber = "HIST-99",
            Customer = new Customer { Name = "Fleet", AccountNumber = "F-99" },
            RetreadJobs =
            [
                new RetreadJob { Id = 10, RetreadNumber = 1, JobNumber = "R1", ReceivedAt = DateTime.Today.AddDays(-20), Status = JobStatus.Dispatched },
                new RetreadJob { Id = 11, RetreadNumber = 2, JobNumber = "R2", ReceivedAt = DateTime.Today.AddDays(-10), Status = JobStatus.InProduction },
                new RetreadJob { Id = 12, RetreadNumber = 3, JobNumber = "R3", ReceivedAt = DateTime.Today, Status = JobStatus.QcPassed }
            ]
        };

        Assert.Equal(3, TyreHistory.SelectRetread(tyre, null)!.RetreadNumber);
        Assert.Equal(1, TyreHistory.SelectRetread(tyre, 10)!.RetreadNumber);
        Assert.Equal(2, TyreHistory.SelectRetread(tyre, 11)!.RetreadNumber);
    }

    [Fact] public void RejectedJobsKeepInternalDetailsButUseGenericCustomerMessage()
    {
        var job = new RetreadJob { Status = JobStatus.Rejected, RejectionReason = "Casing separation" };
        Assert.Equal("Casing separation", job.RejectionReason);
        Assert.Equal(RetreadJob.DefaultCustomerVisibleRejectionMessage, job.CustomerFacingStatusMessage);
    }

    [Fact] public async Task FormsPreviewLoadsLiveTyreDataForSelectedRetread()
    {
        var db = Database();
        var customer = new Customer { Name = "Northfleet", AccountNumber = "NF-42" };
        var tyre = new Tyre
        {
            AksTyreId = "GTC260123",
            Brand = "Michelin",
            Size = "11R22.5",
            SerialNumber = "SER-123",
            Customer = customer,
            RetreadJobs =
            [
                new RetreadJob { RetreadNumber = 1, JobNumber = "JOB-01", ReceivedAt = DateTime.Today.AddDays(-7), CurrentStage = "Curing", Status = JobStatus.Dispatched },
                new RetreadJob { RetreadNumber = 2, JobNumber = "JOB-42", ReceivedAt = DateTime.Today, CurrentStage = "Final Inspection", Status = JobStatus.InProduction }
            ]
        };
        db.Tyres.Add(tyre);
        await db.SaveChangesAsync();

        var controller = new FormsController(db);
        var result = await controller.Preview("retread-inspection", tyre.Id, tyre.RetreadJobs.Last().Id);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<FormPreviewVm>(view.Model);
        Assert.Equal("GTC260123", model.AksTyreId);
        Assert.Equal("JOB-42", model.JobNumber);
        Assert.Equal("Final Inspection", model.CurrentStage);
        Assert.Equal("Northfleet", model.CustomerName);
    }

    [Fact] public void BarcodeRendererProducesPermanentIdAsSvg()
    {var svg=Code39Barcode.RenderSvg("GTC260001");Assert.Contains("<svg",svg);Assert.Contains("GTC260001",svg);Assert.Contains("<rect",svg);}
}
