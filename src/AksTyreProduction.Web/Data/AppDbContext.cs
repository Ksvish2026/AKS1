using AksTyreProduction.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AksTyreProduction.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Tyre> Tyres => Set<Tyre>();
    public DbSet<RetreadJob> RetreadJobs => Set<RetreadJob>();
    public DbSet<Operator> Operators => Set<Operator>();
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<StationTransaction> StationTransactions => Set<StationTransaction>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<MaterialBatch> MaterialBatches => Set<MaterialBatch>();
    public DbSet<MaterialUsage> MaterialUsages => Set<MaterialUsage>();
    public DbSet<Repair> Repairs => Set<Repair>();
    public DbSet<PhotoAttachment> PhotoAttachments => Set<PhotoAttachment>();
    public DbSet<Dispatch> Dispatches => Set<Dispatch>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Tyre>().HasIndex(x => x.AksTyreId).IsUnique();
        model.Entity<Tyre>().HasIndex(x => x.SerialNumber);
        model.Entity<RetreadJob>().HasIndex(x => new { x.TyreId, x.RetreadNumber }).IsUnique();
        model.Entity<MaterialBatch>().HasIndex(x => x.LotNumber);
        model.Entity<AppUser>().HasIndex(x => x.Username).IsUnique();
        model.Entity<RetreadJob>().HasOne(x => x.QcOperator).WithMany().HasForeignKey(x => x.QcOperatorId).OnDelete(DeleteBehavior.SetNull);
        model.Entity<RetreadJob>().HasOne(x => x.Dispatch).WithOne(x => x.RetreadJob).HasForeignKey<Dispatch>(x => x.RetreadJobId);
        model.Entity<RetreadJob>().HasOne(x => x.Invoice).WithOne(x => x.RetreadJob).HasForeignKey<Invoice>(x => x.RetreadJobId);
        model.Entity<MaterialUsage>().HasOne(x => x.StationTransaction).WithMany(x => x.MaterialUsages).HasForeignKey(x => x.StationTransactionId).OnDelete(DeleteBehavior.SetNull);
        foreach (var property in model.Model.GetEntityTypes().SelectMany(e => e.GetProperties()).Where(p => p.ClrType == typeof(decimal))) { property.SetPrecision(18); property.SetScale(2); }
    }
}
