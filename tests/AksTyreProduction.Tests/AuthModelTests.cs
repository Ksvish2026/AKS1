using AksTyreProduction.Web.Controllers;
using AksTyreProduction.Web.Data;
using AksTyreProduction.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AksTyreProduction.Tests;

public class AuthModelTests
{
    [Fact]
    public void AllowedRolesIncludeManagementAndQc()
    {
        Assert.Contains("Administrator", ApplicationRoles.All);
        Assert.Contains("Management", ApplicationRoles.All);
        Assert.Contains("QC", ApplicationRoles.All);
    }

    [Fact]
    public void SelectedRoleFallsBackToDefaultRole()
    {
        var user = new AppUser { RolesCsv = "Administrator,Management", DefaultRole = "Management" };
        Assert.Equal("Management", user.SelectedRole);
    }

    [Fact]
    public async Task DemoSeederCreatesDocumentedAdminCredentials()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var db = new AppDbContext(options))
        {
            await DemoSeeder.SeedAsync(db);
        }

        await using (var db = new AppDbContext(options))
        {
            var admin = await db.Users.SingleAsync(x => x.Username == "Admin");
            Assert.Equal("Admin", admin.PasswordHash);
            Assert.Contains("Administrator", admin.RolesCsv);
        }
    }

    [Fact]
    public async Task DashboardStatisticsTracksProductionCounts()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using (var db = new AppDbContext(options))
        {
            var customer = new Customer { Name = "Stats Test Co", AccountNumber = "ST-1", Address = "Test ST", Site = "Site 1" };
            db.Customers.Add(customer);
            await db.SaveChangesAsync();

            db.Tyres.AddRange(
                new Tyre { AksTyreId = "STAT-001", CustomerId = customer.Id, Brand = "Michelin", Size = "11R22.5", SerialNumber = "ABC001", RetreadJobs = [ new RetreadJob { JobNumber = "J-1", Status = JobStatus.InProduction, CurrentStage = "Buffing", ReceivedAt = DateTime.Today.AddDays(-2), RetreadNumber = 1, StationTransactions = [ new StationTransaction { Station = "Buffing", Operator = new Operator { EmployeeId = "O-1", Name = "Tester", LabourRate = 100m }, StartedAt = DateTime.Today.AddHours(8), EndedAt = DateTime.Today.AddHours(9), DurationMinutes = 60, LabourRateSnapshot = 100m, LabourCost = 100m } ] } ] },
                new Tyre { AksTyreId = "STAT-002", CustomerId = customer.Id, Brand = "Bridgestone", Size = "12R22.5", SerialNumber = "ABC002", RetreadJobs = [ new RetreadJob { JobNumber = "J-2", Status = JobStatus.ReadyForDispatch, CurrentStage = "QC Release", ReceivedAt = DateTime.Today.AddDays(-5), RetreadNumber = 1, ReadyForDispatchAt = DateTime.Today } ] },
                new Tyre { AksTyreId = "STAT-003", CustomerId = customer.Id, Brand = "Continental", Size = "10R20", SerialNumber = "ABC003", RetreadJobs = [ new RetreadJob { JobNumber = "J-3", Status = JobStatus.Rejected, CurrentStage = "Final Inspection", ReceivedAt = DateTime.Today.AddDays(-1), RetreadNumber = 1, RejectionReason = "Sidewall damage" } ] }
            );

            await db.SaveChangesAsync();

            var controller = new DashboardController(db);
            var result = await controller.Statistics();
            var view = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<StatisticsVm>(view.Model);

            Assert.Equal(3, vm.TotalRetreads);
            Assert.Equal(2, vm.ActiveJobs);
            Assert.Equal(1, vm.ReadyForDispatchJobs);
            Assert.Equal(1, vm.RejectedJobs);
            Assert.Equal(3, vm.TotalTyres);
        }
    }
}
