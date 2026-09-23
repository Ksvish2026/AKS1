using AksTyreProduction.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AksTyreProduction.Web.Data;

public static class DemoSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();
        if (await db.Customers.AnyAsync())
        {
            var flagshipJob=await db.RetreadJobs.Include(x=>x.Tyre).Where(x=>x.Tyre.AksTyreId=="GTC260001").OrderByDescending(x=>x.RetreadNumber).FirstOrDefaultAsync();
            if(flagshipJob!=null&&flagshipJob.EstimatedCompletion==null){flagshipJob.EstimatedCompletion=DateTime.Today.AddDays(2).AddHours(16);await db.SaveChangesAsync();}
            return;
        }
        var customers = new[] {
            new Customer { Name="XYZ Logistics", AccountNumber="XYZ-001", Address="18 Freight Road, Johannesburg", Site="Gauteng Fleet" },
            new Customer { Name="ABC Transport", AccountNumber="ABC-042", Address="7 Depot Avenue, Pretoria", Site="Main Depot" },
            new Customer { Name="Durban Freight Services", AccountNumber="DFS-118", Address="22 Harbour View, Durban", Site="Durban Port" }
        }; db.Customers.AddRange(customers);
        var ops = new[] {
            new Operator { EmployeeId="OP001", Name="Peter Mokoena", Role="Buffing Operator", LabourRate=150m },
            new Operator { EmployeeId="OP002", Name="John Naidoo", Role="Inspector", LabourRate=180m },
            new Operator { EmployeeId="OP003", Name="Sarah Dlamini", Role="Receiving", LabourRate=120m },
            new Operator { EmployeeId="OP004", Name="Mike Jacobs", Role="Building Operator", LabourRate=155m },
            new Operator { EmployeeId="OP005", Name="Thandi Nkosi", Role="QC Inspector", LabourRate=190m }
        }; db.Operators.AddRange(ops);
        var machines = new[] {
            new Machine { MachineId="BUF-01", Name="Buffing Machine 01", Type="Buffing" }, new Machine { MachineId="BUF-02", Name="Buffing Machine 02", Type="Buffing" },
            new Machine { MachineId="CUR-01", Name="Curing Press 01", Type="Curing" }, new Machine { MachineId="CUR-02", Name="Curing Press 02", Type="Curing" }, new Machine { MachineId="CUR-03", Name="Curing Press 03", Type="Curing" },
            new Machine { MachineId="BLD-01", Name="Building Machine 01", Type="Building" }
        }; db.Machines.AddRange(machines);
        var materials = new[] {
            new Material { MaterialId="MAT-RUB", Name="Rubber", Category="Rubber", UnitOfMeasure="kg", CurrentUnitCost=62.59m, QuantityOnHand=450m, ReorderLevel=80m, BatchTrackingRequired=true },
            new Material { MaterialId="MAT-CG", Name="Cushion Gum", Category="Bonding", UnitOfMeasure="kg", CurrentUnitCost=83.08m, QuantityOnHand=180m, ReorderLevel=30m, BatchTrackingRequired=true },
            new Material { MaterialId="MAT-BND", Name="Bonding Material", Category="Bonding", UnitOfMeasure="kg", CurrentUnitCost=83.08m, QuantityOnHand=90m, ReorderLevel=20m, BatchTrackingRequired=true },
            new Material { MaterialId="MAT-P100", Name="Patch 100 mm", Category="Repair", UnitOfMeasure="each", CurrentUnitCost=90m, QuantityOnHand=75m, ReorderLevel=15m },
            new Material { MaterialId="MAT-P150", Name="Patch 150 mm", Category="Repair", UnitOfMeasure="each", CurrentUnitCost=125m, QuantityOnHand=40m, ReorderLevel=10m },
            new Material { MaterialId="MAT-RR", Name="Repair Rubber", Category="Repair", UnitOfMeasure="kg", CurrentUnitCost=71m, QuantityOnHand=65m, ReorderLevel=12m },
            new Material { MaterialId="MAT-VAL", Name="Valve", Category="Component", UnitOfMeasure="each", CurrentUnitCost=24.50m, QuantityOnHand=140m, ReorderLevel=25m },
            new Material { MaterialId="MAT-ENV", Name="Envelope", Category="Component", UnitOfMeasure="each", CurrentUnitCost=185m, QuantityOnHand=55m, ReorderLevel=10m },
            new Material { MaterialId="MAT-CON", Name="Consumables", Category="Consumable", UnitOfMeasure="unit", CurrentUnitCost=35m, QuantityOnHand=250m, ReorderLevel=40m }
        }; db.Materials.AddRange(materials);
        await db.SaveChangesAsync();
        db.MaterialBatches.AddRange(
            new MaterialBatch { MaterialId=materials[0].Id, LotNumber="RB-260901", QuantityRemaining=300m, ReceivedAt=DateTime.Today.AddDays(-30) },
            new MaterialBatch { MaterialId=materials[1].Id, LotNumber="CG-260912", QuantityRemaining=120m, ReceivedAt=DateTime.Today.AddDays(-20) },
            new MaterialBatch { MaterialId=materials[2].Id, LotNumber="BM-260915", QuantityRemaining=70m, ReceivedAt=DateTime.Today.AddDays(-18) });
        var flagship = new Tyre { AksTyreId="GTC260001", CustomerId=customers[0].Id, Brand="Michelin", Size="11R22.5", SerialNumber="ABC123456", OriginalTreadPattern="X Multi D", CasingCondition="Good", Application="Long Haul" };
        db.Tyres.Add(flagship); await db.SaveChangesAsync();
        var oldJob = new RetreadJob { TyreId=flagship.Id, RetreadNumber=1, JobNumber="JOB-25001", ReceivedAt=DateTime.Today.AddYears(-1).AddDays(-18), CompletedAt=DateTime.Today.AddYears(-1).AddDays(-12), Status=JobStatus.Dispatched, CurrentStage="Dispatched", BaseCost=120m };
        var currentJob = new RetreadJob { TyreId=flagship.Id, RetreadNumber=2, JobNumber="JOB-26001", ReceivedAt=DateTime.Today.AddDays(-2), Status=JobStatus.InProduction, CurrentStage="Buffing", BaseCost=150m };
        db.RetreadJobs.AddRange(oldJob,currentJob); await db.SaveChangesAsync();
        AddHistory(oldJob, ops, machines, true);
        AddTx(currentJob, "Receiving", ops[2], null, 8, TransactionResult.Completed, "Returning casing received and identified.", -2800);
        AddTx(currentJob, "Initial Inspection", ops[1], null, 13, TransactionResult.Pass, "Casing suitable for retread.", -2700);
        AddTx(currentJob, "Shearography / NDT", ops[1], null, 9, TransactionResult.Pass, "No internal separation detected.", -2600);
        for (var i=2; i<=22; i++)
        {
            var tyre = new Tyre { AksTyreId=$"GTC26{i:0000}", CustomerId=customers[(i-1)%3].Id, Brand=i%3==0?"Bridgestone":i%2==0?"Goodyear":"Continental", Size=i%2==0?"315/80R22.5":"11R22.5", SerialNumber=$"SN26{i:00000}", OriginalTreadPattern="Regional Drive", CasingCondition="Good", Application="Commercial" };
            db.Tyres.Add(tyre); await db.SaveChangesAsync();
            var stageIndex=(i-2)%Workflow.Stages.Length; var status=i==21?JobStatus.Rejected:i>=18?JobStatus.Dispatched:JobStatus.InProduction;
            var job=new RetreadJob { TyreId=tyre.Id, RetreadNumber=1, JobNumber=$"JOB-26{i:000}", ReceivedAt=DateTime.Today.AddDays(-(i%8)), CurrentStage=status==JobStatus.Dispatched?"Dispatched":Workflow.Stages[stageIndex], Status=status, CompletedAt=status==JobStatus.Dispatched?DateTime.Today.AddDays(-1):null, RejectionReason=status==JobStatus.Rejected?"Casing separation":null, BaseCost=100m };
            db.RetreadJobs.Add(job); await db.SaveChangesAsync();
            for(var s=0;s<Math.Min(stageIndex,5);s++) AddTx(job,Workflow.Stages[s],ops[s%ops.Length],null,12+s*5,TransactionResult.Pass,"Demo production record",-(i*100+s*20));
        }
        await db.SaveChangesAsync();
        var cgBatch=await db.MaterialBatches.SingleAsync(x=>x.LotNumber=="CG-260912"); var rubberBatch=await db.MaterialBatches.SingleAsync(x=>x.LotNumber=="RB-260901");
        db.MaterialUsages.AddRange(
            new MaterialUsage { RetreadJobId=oldJob.Id, MaterialId=materials[0].Id, MaterialBatchId=rubberBatch.Id, Quantity=8m, UnitCostSnapshot=62.59m, TotalCost=500.72m, UsedAt=oldJob.ReceivedAt.AddDays(2) },
            new MaterialUsage { RetreadJobId=oldJob.Id, MaterialId=materials[1].Id, MaterialBatchId=cgBatch.Id, Quantity=1.2m, UnitCostSnapshot=83.08m, TotalCost=99.70m, UsedAt=oldJob.ReceivedAt.AddDays(2) },
            new MaterialUsage { RetreadJobId=currentJob.Id, MaterialId=materials[1].Id, MaterialBatchId=cgBatch.Id, Quantity=.8m, UnitCostSnapshot=83.08m, TotalCost=66.46m, UsedAt=DateTime.Today.AddDays(-1) });
        db.Repairs.Add(new Repair { RetreadJobId=oldJob.Id, Type="Sidewall repair", Description="100 mm patch fitted", CreatedAt=oldJob.ReceivedAt.AddDays(1) });
        db.Dispatches.Add(new Dispatch { RetreadJobId=oldJob.Id, DispatchDate=oldJob.CompletedAt!.Value, Method="Delivery", Reference="DSP-25001", Notes="Delivered to Gauteng fleet." });
        db.AuditEvents.Add(new AuditEvent { TyreId=flagship.Id, RetreadJobId=currentJob.Id, Action="Demo data created", NewValue="Current retread in Buffing", OccurredAt=DateTime.Now });
        await db.SaveChangesAsync();
    }

    private static void AddHistory(RetreadJob job, Operator[] ops, Machine[] machines, bool complete)
    {
        for(var i=0;i<Workflow.Stages.Length-1;i++) AddTx(job,Workflow.Stages[i],ops[i%ops.Length], i==3?machines[0]:i==8?machines[2]:null, i==3?95:20+i*3,TransactionResult.Pass,"Completed to specification",-(5000-i*180));
    }
    private static void AddTx(RetreadJob job,string station,Operator op,Machine? machine,int minutes,TransactionResult result,string notes,int minuteOffset)
    {
        var start=DateTime.Now.AddMinutes(minuteOffset); job.StationTransactions.Add(new StationTransaction { Station=station,OperatorId=op.Id,MachineId=machine?.Id,StartedAt=start,EndedAt=start.AddMinutes(minutes),DurationMinutes=minutes,LabourRateSnapshot=op.LabourRate,LabourCost=decimal.Round(op.LabourRate*minutes/60m,2),Result=result,Notes=notes });
    }
}
