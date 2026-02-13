using Microsoft.EntityFrameworkCore;
using AccountingERP.Domain.Entities;

namespace AccountingERP.Infrastructure.Data;

/// <summary>
/// DbContext cho AccountingERP
/// </summary>
public class AccountingDbContext : DbContext
{
    public AccountingDbContext(DbContextOptions<AccountingDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountingDbContext).Assembly);

        // JournalEntry configuration
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.ToTable("JournalEntries");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EntryNumber).IsUnique();
            entity.HasIndex(e => e.OriginalDocumentNumber);
            entity.HasIndex(e => e.EntryDate);
            
            entity.Property(e => e.EntryNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.OriginalDocumentNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Reference).HasMaxLength(50);
            entity.Property(e => e.Status).HasConversion<string>();
            
            entity.HasMany(e => e.Lines)
                .WithOne(l => l.JournalEntry)
                .HasForeignKey(l => l.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // JournalEntryLine configuration
        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
            entity.ToTable("JournalEntryLines");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.AccountCode).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CostCenterCode).HasMaxLength(20);
            entity.Property(e => e.ProjectCode).HasMaxLength(20);
            
            entity.Property(e => e.DebitAmount)
                .HasPrecision(18, 2);
            entity.Property(e => e.CreditAmount)
                .HasPrecision(18, 2);
        });

        // Account configuration
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Accounts");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            
            entity.Property(e => e.Code).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.EnglishName).HasMaxLength(200);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.ParentCode).HasMaxLength(10);
            entity.Property(e => e.Description).HasMaxLength(1000);
        });
    }
}
