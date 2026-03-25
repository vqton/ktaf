namespace AMS.Infrastructure.Data;

using AMS.Domain.Entities;
using AMS.Domain.Entities.DM;
using AMS.Domain.Entities.HR;
using AMS.Domain.Entities.Inventory;
using AMS.Domain.Entities.Assets;
using AMS.Domain.Entities.Tax;
using AMS.Domain.Entities.Audit;
using AMS.Domain.Entities.Cfg;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Entity Framework Core database context for the AMS application.
/// Manages all database operations and entity configurations.
/// </summary>
public class AMSDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the AMSDbContext class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public AMSDbContext(DbContextOptions<AMSDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Vouchers DbSet.
    /// </summary>
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    /// <summary>
    /// Gets or sets the VoucherLines DbSet.
    /// </summary>
    public DbSet<VoucherLine> VoucherLines => Set<VoucherLine>();

    /// <summary>
    /// Gets or sets the VoucherAttachments DbSet.
    /// </summary>
    public DbSet<VoucherAttachment> VoucherAttachments => Set<VoucherAttachment>();

    /// <summary>
    /// Gets or sets the FiscalPeriods DbSet.
    /// </summary>
    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();

    /// <summary>
    /// Gets or sets the LedgerEntries DbSet.
    /// </summary>
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    /// <summary>
    /// Gets or sets the AccountBalances DbSet.
    /// </summary>
    public DbSet<AccountBalance> AccountBalances => Set<AccountBalance>();

    /// <summary>
    /// Gets or sets the ChartOfAccounts DbSet.
    /// </summary>
    public DbSet<ChartOfAccounts> ChartOfAccounts => Set<ChartOfAccounts>();

    /// <summary>
    /// Gets or sets the Customers DbSet.
    /// </summary>
    public DbSet<Customer> Customers => Set<Customer>();

    /// <summary>
    /// Gets or sets the Vendors DbSet.
    /// </summary>
    public DbSet<Vendor> Vendors => Set<Vendor>();

    /// <summary>
    /// Gets or sets the Employees DbSet.
    /// </summary>
    public DbSet<Employee> Employees => Set<Employee>();

    /// <summary>
    /// Gets or sets the Products DbSet.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Gets or sets the ProductGroups DbSet.
    /// </summary>
    public DbSet<ProductGroup> ProductGroups => Set<ProductGroup>();

    /// <summary>
    /// Gets or sets the Warehouses DbSet.
    /// </summary>
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    /// <summary>
    /// Gets or sets the Banks DbSet.
    /// </summary>
    public DbSet<Bank> Banks => Set<Bank>();

    /// <summary>
    /// Gets or sets the BankAccounts DbSet.
    /// </summary>
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();

    /// <summary>
    /// Gets or sets the BankTransactions DbSet.
    /// </summary>
    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();

    /// <summary>
    /// Gets or sets the CashBooks DbSet.
    /// </summary>
    public DbSet<CashBook> CashBooks => Set<CashBook>();

    /// <summary>
    /// Gets or sets the CashBookEntries DbSet.
    /// </summary>
    public DbSet<CashBookEntry> CashBookEntries => Set<CashBookEntry>();

    /// <summary>
    /// Gets or sets the BankReconciliations DbSet.
    /// </summary>
    public DbSet<BankReconciliation> BankReconciliations => Set<BankReconciliation>();

    /// <summary>
    /// Gets or sets the ExchangeRates DbSet.
    /// </summary>
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

    /// <summary>
    /// Gets or sets the FixedAssets DbSet.
    /// </summary>
    public DbSet<FixedAsset> FixedAssets => Set<FixedAsset>();

    /// <summary>
    /// Gets or sets the DepreciationSchedules DbSet.
    /// </summary>
    public DbSet<DepreciationSchedule> DepreciationSchedules => Set<DepreciationSchedule>();

    /// <summary>
    /// Gets or sets the AssetGroups DbSet.
    /// </summary>
    public DbSet<AssetGroup> AssetGroups => Set<AssetGroup>();

    /// <summary>
    /// Gets or sets the Departments DbSet.
    /// </summary>
    public DbSet<Department> Departments => Set<Department>();

    /// <summary>
    /// Gets or sets the TaxRates DbSet.
    /// </summary>
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();

    /// <summary>
    /// Gets or sets the PITBrackets DbSet.
    /// </summary>
    public DbSet<PITBracket> PITBrackets => Set<PITBracket>();

    /// <summary>
    /// Gets or sets the PITAllowances DbSet.
    /// </summary>
    public DbSet<PITAllowance> PITAllowances => Set<PITAllowance>();

    /// <summary>
    /// Gets or sets the ExciseTaxItems DbSet.
    /// </summary>
    public DbSet<ExciseTaxItem> ExciseTaxItems => Set<ExciseTaxItem>();

    /// <summary>
    /// Gets or sets the TaxDeclarations DbSet.
    /// </summary>
    public DbSet<TaxDeclaration> TaxDeclarations => Set<TaxDeclaration>();

    /// <summary>
    /// Gets or sets the VATInputRegisters DbSet.
    /// </summary>
    public DbSet<VATInputRegister> VATInputRegisters => Set<VATInputRegister>();

    /// <summary>
    /// Gets or sets the VATOutputRegisters DbSet.
    /// </summary>
    public DbSet<VATOutputRegister> VATOutputRegisters => Set<VATOutputRegister>();

    /// <summary>
    /// Gets or sets the TaxPayments DbSet.
    /// </summary>
    public DbSet<TaxPayment> TaxPayments => Set<TaxPayment>();

    /// <summary>
    /// Gets or sets the TaxCalendars DbSet.
    /// </summary>
    public DbSet<TaxCalendar> TaxCalendars => Set<TaxCalendar>();

    /// <summary>
    /// Gets or sets the CITAdjustments DbSet.
    /// </summary>
    public DbSet<CITAdjustment> CITAdjustments => Set<CITAdjustment>();

    /// <summary>
    /// Gets or sets the CITLossCarryForwards DbSet.
    /// </summary>
    public DbSet<CITLossCarryForward> CITLossCarryForwards => Set<CITLossCarryForward>();

    /// <summary>
    /// Gets or sets the WithholdingTaxes DbSet.
    /// </summary>
    public DbSet<WithholdingTax> WithholdingTaxes => Set<WithholdingTax>();

    /// <summary>
    /// Gets or sets the InventoryTransactions DbSet.
    /// </summary>
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

    /// <summary>
    /// Gets or sets the InventoryBalances DbSet.
    /// </summary>
    public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();

    /// <summary>
    /// Gets or sets the NumberSequences DbSet.
    /// </summary>
    public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();

    /// <summary>
    /// Gets or sets the AppSettings DbSet.
    /// </summary>
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    /// <summary>
    /// Gets or sets the OutboxMessages DbSet.
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Configures the model relationships and constraints using Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The builder used to configure the model.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("acc");

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.ToTable("vouchers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("voucher_id");
            entity.Property(e => e.VoucherNo).HasColumnName("voucher_no").HasMaxLength(25).IsRequired();
            entity.Property(e => e.Type).HasColumnName("voucher_type").HasMaxLength(10).HasConversion<string>();
            entity.Property(e => e.VoucherDate).HasColumnName("voucher_date").IsRequired();
            entity.Property(e => e.FiscalPeriodId).HasColumnName("fiscal_period_id").IsRequired();
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(15).HasConversion<string>();
            entity.Property(e => e.TotalDebit).HasColumnName("total_debit").HasPrecision(18, 0);
            entity.Property(e => e.TotalCredit).HasColumnName("total_credit").HasPrecision(18, 0);
            entity.Property(e => e.CurrencyCode).HasColumnName("currency_code").HasMaxLength(3);
            entity.Property(e => e.ExchangeRate).HasColumnName("exchange_rate").HasPrecision(18, 4);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100);
            entity.Property(e => e.ModifiedAt).HasColumnName("modified_at");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

            entity.HasIndex(e => e.VoucherNo);
            entity.HasIndex(e => e.VoucherDate);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.FiscalPeriodId);

            entity.HasOne(e => e.FiscalPeriod)
                .WithMany(p => p.Vouchers)
                .HasForeignKey(e => e.FiscalPeriodId);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<VoucherLine>(entity =>
        {
            entity.ToTable("voucher_lines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("voucher_line_id");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id").IsRequired();
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.DebitAmount).HasColumnName("debit_amount").HasPrecision(18, 0);
            entity.Property(e => e.CreditAmount).HasColumnName("credit_amount").HasPrecision(18, 0);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.VendorId).HasColumnName("vendor_id");
            entity.Property(e => e.IsExciseTaxLine).HasColumnName("is_excise_tax_line");
            entity.Property(e => e.CitAdjFlag).HasColumnName("cit_adj_flag").HasMaxLength(50);

            entity.HasIndex(e => e.VoucherId);
            entity.HasIndex(e => e.AccountId);

            entity.HasOne(e => e.Voucher)
                .WithMany(v => v.Lines)
                .HasForeignKey(e => e.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VoucherAttachment>(entity =>
        {
            entity.ToTable("voucher_attachments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("attachment_id");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id").IsRequired();
            entity.Property(e => e.FileName).HasColumnName("file_name").HasMaxLength(255);
            entity.Property(e => e.FilePath).HasColumnName("file_path").HasMaxLength(500);
            entity.Property(e => e.ContentType).HasColumnName("content_type").HasMaxLength(100);
            entity.Property(e => e.FileSize).HasColumnName("file_size");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);

            entity.HasOne(e => e.Voucher)
                .WithMany(v => v.Attachments)
                .HasForeignKey(e => e.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FiscalPeriod>(entity =>
        {
            entity.ToTable("fiscal_periods");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("fiscal_period_id");
            entity.Property(e => e.Year).HasColumnName("year").IsRequired();
            entity.Property(e => e.Month).HasColumnName("month").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(15).HasConversion<string>();

            entity.HasIndex(e => new { e.Year, e.Month }).IsUnique();
        });

        modelBuilder.Entity<LedgerEntry>(entity =>
        {
            entity.ToTable("ledger_entries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ledger_entry_id");
            entity.Property(e => e.FiscalPeriodId).HasColumnName("fiscal_period_id").IsRequired();
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");
            entity.Property(e => e.VoucherNo).HasColumnName("voucher_no").HasMaxLength(25);
            entity.Property(e => e.VoucherDate).HasColumnName("voucher_date");
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.AccountCode).HasColumnName("account_code").HasMaxLength(20);
            entity.Property(e => e.DebitAmount).HasColumnName("debit_amount").HasPrecision(18, 0);
            entity.Property(e => e.CreditAmount).HasColumnName("credit_amount").HasPrecision(18, 0);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.CurrencyCode).HasColumnName("currency_code").HasMaxLength(3);
            entity.Property(e => e.ExchangeRate).HasColumnName("exchange_rate").HasPrecision(18, 4);

            entity.HasIndex(e => e.FiscalPeriodId);
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.VoucherDate);
        });

        modelBuilder.Entity<AccountBalance>(entity =>
        {
            entity.ToTable("account_balances");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("balance_id");
            entity.Property(e => e.FiscalPeriodId).HasColumnName("fiscal_period_id").IsRequired();
            entity.Property(e => e.AccountId).HasColumnName("account_id").IsRequired();
            entity.Property(e => e.AccountCode).HasColumnName("account_code").HasMaxLength(20);
            entity.Property(e => e.OpeningDebit).HasColumnName("opening_debit").HasPrecision(18, 0);
            entity.Property(e => e.OpeningCredit).HasColumnName("opening_credit").HasPrecision(18, 0);
            entity.Property(e => e.PeriodDebit).HasColumnName("period_debit").HasPrecision(18, 0);
            entity.Property(e => e.PeriodCredit).HasColumnName("period_credit").HasPrecision(18, 0);
            entity.Property(e => e.ClosingDebit).HasColumnName("closing_debit").HasPrecision(18, 0);
            entity.Property(e => e.ClosingCredit).HasColumnName("closing_credit").HasPrecision(18, 0);

            entity.HasIndex(e => new { e.FiscalPeriodId, e.AccountId }).IsUnique();
            entity.HasIndex(e => e.FiscalPeriodId);
            entity.HasIndex(e => e.AccountId);

            entity.HasOne(e => e.FiscalPeriod)
                .WithMany()
                .HasForeignKey(e => e.FiscalPeriodId);

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId);
        });

        modelBuilder.Entity<ChartOfAccounts>(entity =>
        {
            entity.ToTable("chart_of_accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("account_id");
            entity.Property(e => e.Code).HasColumnName("account_code").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasColumnName("account_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.AccountNumber).HasColumnName("account_number");
            entity.Property(e => e.ParentId).HasColumnName("parent_account_id");
            entity.Property(e => e.AccountType).HasColumnName("account_type").HasMaxLength(20).HasConversion<string>();
            entity.Property(e => e.IsDetail).HasColumnName("is_detail_account");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);

            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.ParentId);

            entity.HasOne(e => e.Parent)
                .WithMany(a => a.Children)
                .HasForeignKey(e => e.ParentId);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("customer_id");
            entity.Property(e => e.Code).HasColumnName("customer_code").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasColumnName("customer_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.TaxCode).HasColumnName("tax_code").HasMaxLength(20);
            entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(500);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.TaxCode);
        });

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.ToTable("vendors");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("vendor_id");
            entity.Property(e => e.Code).HasColumnName("vendor_code").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasColumnName("vendor_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.TaxCode).HasColumnName("tax_code").HasMaxLength(20);
            entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(500);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.TaxCode);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employees");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("employee_id");
            entity.Property(e => e.EmployeeCode).HasColumnName("employee_code").HasMaxLength(20).IsRequired();
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            entity.HasIndex(e => e.EmployeeCode).IsUnique();
            entity.HasIndex(e => e.ADUsername);
        });

        modelBuilder.Entity<Bank>(entity =>
        {
            entity.ToTable("banks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("bank_id");
            entity.Property(e => e.Code).HasColumnName("bank_code").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasColumnName("bank_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.SwiftCode).HasColumnName("swift_code").HasMaxLength(20);
            entity.Property(e => e.BranchName).HasColumnName("branch_name").HasMaxLength(255);
            entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(500);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.ToTable("bank_accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("bank_account_id");
            entity.Property(e => e.BankId).HasColumnName("bank_id").IsRequired();
            entity.Property(e => e.AccountNumber).HasColumnName("account_number").HasMaxLength(50).IsRequired();
            entity.Property(e => e.AccountName).HasColumnName("account_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.AccountType).HasColumnName("account_type").HasMaxLength(20);
            entity.Property(e => e.CurrencyCode).HasColumnName("currency_code").HasMaxLength(3);
            entity.Property(e => e.OpeningBalance).HasColumnName("opening_balance").HasPrecision(18, 0);
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.BranchName).HasColumnName("branch_name").HasMaxLength(255);
            entity.Property(e => e.AccountHolder).HasColumnName("account_holder").HasMaxLength(255);

            entity.HasIndex(e => e.AccountNumber).IsUnique();
            entity.HasOne(e => e.Bank)
                .WithMany(b => b.BankAccounts)
                .HasForeignKey(e => e.BankId);
        });

        modelBuilder.Entity<BankTransaction>(entity =>
        {
            entity.ToTable("bank_transactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("bank_transaction_id");
            entity.Property(e => e.BankAccountId).HasColumnName("bank_account_id").IsRequired();
            entity.Property(e => e.TransactionDate).HasColumnName("transaction_date");
            entity.Property(e => e.TransactionType).HasColumnName("transaction_type").HasMaxLength(20).HasConversion<string>();
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(18, 0);
            entity.Property(e => e.FeeAmount).HasColumnName("fee_amount").HasPrecision(18, 0);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.ReferenceNumber).HasColumnName("reference_number").HasMaxLength(50);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasConversion<string>();
            entity.Property(e => e.IsReconciled).HasColumnName("is_reconciled");
            entity.Property(e => e.ReconciledDate).HasColumnName("reconciled_date");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");
            entity.Property(e => e.PartnerName).HasColumnName("partner_name").HasMaxLength(255);
            entity.Property(e => e.PartnerAccountNumber).HasColumnName("partner_account_number").HasMaxLength(50);

            entity.HasIndex(e => e.BankAccountId);
            entity.HasIndex(e => e.TransactionDate);
            entity.HasOne(e => e.BankAccount)
                .WithMany(b => b.Transactions)
                .HasForeignKey(e => e.BankAccountId);
        });

        modelBuilder.Entity<CashBook>(entity =>
        {
            entity.ToTable("cash_books");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("cash_book_id");
            entity.Property(e => e.Code).HasColumnName("cash_book_code").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasColumnName("cash_book_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.IsMain).HasColumnName("is_main");
            entity.Property(e => e.CurrencyCode).HasColumnName("currency_code").HasMaxLength(3);
            entity.Property(e => e.OpeningBalance).HasColumnName("opening_balance").HasPrecision(18, 0);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);

            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<CashBookEntry>(entity =>
        {
            entity.ToTable("cash_book_entries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("cash_book_entry_id");
            entity.Property(e => e.CashBookId).HasColumnName("cash_book_id").IsRequired();
            entity.Property(e => e.FiscalPeriodId).HasColumnName("fiscal_period_id").IsRequired();
            entity.Property(e => e.EntryDate).HasColumnName("entry_date");
            entity.Property(e => e.EntryType).HasColumnName("entry_type").HasMaxLength(20).HasConversion<string>();
            entity.Property(e => e.ReferenceNo).HasColumnName("reference_no").HasMaxLength(50);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.ReceiptAmount).HasColumnName("receipt_amount").HasPrecision(18, 0);
            entity.Property(e => e.PaymentAmount).HasColumnName("payment_amount").HasPrecision(18, 0);
            entity.Property(e => e.Balance).HasColumnName("balance").HasPrecision(18, 0);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasConversion<string>();
            entity.Property(e => e.IsReconciled).HasColumnName("is_reconciled");
            entity.Property(e => e.VoucherId).HasColumnName("voucher_id");
            entity.Property(e => e.PartnerName).HasColumnName("partner_name").HasMaxLength(255);

            entity.HasIndex(e => e.CashBookId);
            entity.HasIndex(e => e.EntryDate);
            entity.HasOne(e => e.CashBook)
                .WithMany(c => c.Entries)
                .HasForeignKey(e => e.CashBookId);
        });

        modelBuilder.Entity<BankReconciliation>(entity =>
        {
            entity.ToTable("bank_reconciliations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("bank_reconciliation_id");
            entity.Property(e => e.BankAccountId).HasColumnName("bank_account_id").IsRequired();
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.StatementClosingBalance).HasColumnName("statement_closing_balance").HasPrecision(18, 0);
            entity.Property(e => e.BookClosingBalance).HasColumnName("book_closing_balance").HasPrecision(18, 0);
            entity.Property(e => e.InTransitDeposits).HasColumnName("in_transit_deposits").HasPrecision(18, 0);
            entity.Property(e => e.UnrecordedBankFees).HasColumnName("unrecorded_bank_fees").HasPrecision(18, 0);
            entity.Property(e => e.UnrecordedInterest).HasColumnName("unrecorded_interest").HasPrecision(18, 0);
            entity.Property(e => e.ReconciliationDate).HasColumnName("reconciliation_date");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasConversion<string>();
            entity.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(1000);
            entity.Property(e => e.PreparedBy).HasColumnName("prepared_by").HasMaxLength(100);
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by").HasMaxLength(100);

            entity.HasIndex(e => new { e.BankAccountId, e.Year, e.Month }).IsUnique();
            entity.HasOne(e => e.BankAccount)
                .WithMany()
                .HasForeignKey(e => e.BankAccountId);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("product_id");
            entity.Property(e => e.ProductCode).HasColumnName("product_code").HasMaxLength(20).IsRequired();
            entity.Property(e => e.ProductName).HasColumnName("product_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Type).HasColumnName("product_type").HasMaxLength(20).HasConversion<string>();
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            entity.HasIndex(e => e.ProductCode).IsUnique();
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("warehouses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("warehouse_id");
            entity.Property(e => e.WarehouseCode).HasColumnName("warehouse_code").HasMaxLength(20).IsRequired();
            entity.Property(e => e.WarehouseName).HasColumnName("warehouse_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            entity.HasIndex(e => e.WarehouseCode).IsUnique();
        });

        modelBuilder.Entity<FixedAsset>(entity =>
        {
            entity.ToTable("fixed_assets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("asset_id");
            entity.Property(e => e.AssetCode).HasColumnName("asset_code").HasMaxLength(20).IsRequired();
            entity.Property(e => e.AssetName).HasColumnName("asset_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasConversion<string>();

            entity.HasIndex(e => e.AssetCode).IsUnique();
        });

        modelBuilder.Entity<DepreciationSchedule>(entity =>
        {
            entity.ToTable("depreciation_schedules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("schedule_id");
            entity.Property(e => e.AssetId).HasColumnName("asset_id").IsRequired();
            entity.Property(e => e.PeriodYear).HasColumnName("period_year").IsRequired();
            entity.Property(e => e.PeriodMonth).HasColumnName("period_month").IsRequired();
            entity.Property(e => e.DepreciationAmount).HasColumnName("depreciation_amount").HasPrecision(18, 0);

            entity.HasIndex(e => new { e.AssetId, e.PeriodYear, e.PeriodMonth }).IsUnique();
        });

        modelBuilder.Entity<TaxRate>(entity =>
        {
            entity.ToTable("tax_rates");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("tax_rate_id");
            entity.Property(e => e.TaxRateKey).HasColumnName("tax_rate_key").HasMaxLength(50).IsRequired();
            entity.Property(e => e.TaxType).HasColumnName("tax_type").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Rate).HasColumnName("rate").HasPrecision(5, 4);
            entity.Property(e => e.EffectiveFrom).HasColumnName("effective_from");
            entity.Property(e => e.EffectiveTo).HasColumnName("effective_to");

            entity.HasIndex(e => e.TaxRateKey);
            entity.HasIndex(e => e.EffectiveFrom);
        });

        modelBuilder.Entity<PITBracket>(entity =>
        {
            entity.ToTable("pit_brackets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("pit_bracket_id");
            entity.Property(e => e.BracketNo).HasColumnName("bracket_no").IsRequired();
            entity.Property(e => e.FromAmount).HasColumnName("from_amount");
            entity.Property(e => e.ToAmount).HasColumnName("to_amount");
            entity.Property(e => e.TaxRate).HasColumnName("tax_rate").HasPrecision(5, 4);
            entity.Property(e => e.QuickDeduction).HasColumnName("quick_deduction");

            entity.HasIndex(e => new { e.BracketNo, e.EffectiveFrom }).IsUnique();
        });

        modelBuilder.Entity<TaxDeclaration>(entity =>
        {
            entity.ToTable("tax_declarations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("tax_declaration_id");
            entity.Property(e => e.TaxType).HasColumnName("tax_type").HasMaxLength(20).IsRequired();
            entity.Property(e => e.PeriodYear).HasColumnName("period_year").IsRequired();
            entity.Property(e => e.PeriodMonth).HasColumnName("period_month").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasConversion<string>();
            entity.Property(e => e.TotalTaxDue).HasColumnName("total_tax_due").HasPrecision(18, 0);

            entity.HasIndex(e => new { e.TaxType, e.PeriodYear, e.PeriodMonth }).IsUnique();
        });

        modelBuilder.Entity<NumberSequence>(entity =>
        {
            entity.ToTable("number_sequences");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("sequence_id");
            entity.Property(e => e.SequenceType).HasColumnName("sequence_type").HasMaxLength(50).IsRequired();
            entity.Property(e => e.CurrentValue).HasColumnName("current_value");
            entity.Property(e => e.Prefix).HasColumnName("prefix").HasMaxLength(20);

            entity.HasIndex(e => new { e.SequenceType, e.FiscalPeriodId }).IsUnique();
        });

        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.ToTable("app_settings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("setting_id");
            entity.Property(e => e.SettingKey).HasColumnName("setting_key").HasMaxLength(100).IsRequired();
            entity.Property(e => e.SettingValue).HasColumnName("setting_value");
            entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(50);

            entity.HasIndex(e => e.SettingKey).IsUnique();
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("outbox_id");
            entity.Property(e => e.MessageType).HasColumnName("message_type").HasMaxLength(200).IsRequired();
            entity.Property(e => e.Payload).HasColumnName("payload");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.RetryCount).HasColumnName("retry_count");

            entity.HasIndex(e => e.ProcessedAt);
        });
    }
}
