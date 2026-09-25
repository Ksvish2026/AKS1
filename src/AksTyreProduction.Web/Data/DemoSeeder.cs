using AksTyreProduction.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AksTyreProduction.Web.Data;

public static class DemoSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        var demoUsers = new[]
        {
            new AppUser { Username = "Admin", PasswordHash = "Admin", RolesCsv = "Administrator,Management,QC", DefaultRole = ApplicationRoles.Administrator },
            new AppUser { Username = "Manager", PasswordHash = "Manager", RolesCsv = "Management,QC", DefaultRole = ApplicationRoles.Management },
            new AppUser { Username = "QC", PasswordHash = "QC", RolesCsv = "QC", DefaultRole = ApplicationRoles.QC }
        };

        foreach (var desired in demoUsers)
        {
            var existing = await db.Users.FirstOrDefaultAsync(x => x.Username.ToLower() == desired.Username.ToLower());
            if (existing is null)
            {
                db.Users.Add(desired);
            }
            else
            {
                existing.Username = desired.Username;
                existing.PasswordHash = desired.PasswordHash;
                existing.RolesCsv = desired.RolesCsv;
                existing.DefaultRole = desired.DefaultRole;
            }
        }

        await db.SaveChangesAsync();

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
            new Material { MaterialId="MAT-STEEL", Name="Steel Cord", Category="Reinforcement", UnitOfMeasure="kg", CurrentUnitCost=91.25m, QuantityOnHand=260m, ReorderLevel=70m, BatchTrackingRequired=true },
            new Material { MaterialId="MAT-ADH", Name="Adhesive", Category="Bonding", UnitOfMeasure="L", CurrentUnitCost=34.40m, QuantityOnHand=160m, ReorderLevel=40m, BatchTrackingRequired=true },
            new Material { MaterialId="MAT-TREAD", Name="Tread Compound", Category="Tread", UnitOfMeasure="kg", CurrentUnitCost=78.15m, QuantityOnHand=210m, ReorderLevel=60m, BatchTrackingRequired=true }
        }; db.Materials.AddRange(materials);
        await db.SaveChangesAsync();
        var materialBatches = new[] {
            new MaterialBatch { Material = materials[0], LotNumber = "RUB-2401", QuantityRemaining = 200m, ReceivedAt = DateTime.Today.AddDays(-18) },
            new MaterialBatch { Material = materials[1], LotNumber = "STEEL-2402", QuantityRemaining = 120m, ReceivedAt = DateTime.Today.AddDays(-10) },
            new MaterialBatch { Material = materials[2], LotNumber = "ADH-2405", QuantityRemaining = 90m, ReceivedAt = DateTime.Today.AddDays(-7) },
            new MaterialBatch { Material = materials[3], LotNumber = "TREAD-2408", QuantityRemaining = 180m, ReceivedAt = DateTime.Today.AddDays(-4) }
        }; db.MaterialBatches.AddRange(materialBatches);
        await db.SaveChangesAsync();
    }
}
