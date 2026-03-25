using AMS.Application.Interfaces;
using AMS.Application.Services;
using AMS.Application.Accounting.Vouchers.Interfaces;
using AMS.Application.Accounting.Vouchers.Services;
using AMS.Domain.Interfaces;
using AMS.Infrastructure;
using AMS.Infrastructure.Data;
using AMS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AMSDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<IChartOfAccountsRepository, ChartOfAccountsRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IVendorRepository, VendorRepository>();
builder.Services.AddScoped<ILedgerRepository, LedgerRepository>();
builder.Services.AddScoped<IFiscalPeriodRepository, FiscalPeriodRepository>();
builder.Services.AddScoped<ITaxRepository, TaxRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IAccountBalanceRepository, AccountBalanceRepository>();
builder.Services.AddScoped<IBankRepository, BankRepository>();
builder.Services.AddScoped<IBankAccountRepository, BankAccountRepository>();
builder.Services.AddScoped<IBankTransactionRepository, BankTransactionRepository>();
builder.Services.AddScoped<ICashBookRepository, CashBookRepository>();
builder.Services.AddScoped<ICashBookEntryRepository, CashBookEntryRepository>();
builder.Services.AddScoped<IBankReconciliationRepository, BankReconciliationRepository>();

builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IChartOfAccountsService, ChartOfAccountsService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<ILedgerService, LedgerService>();
builder.Services.AddScoped<IFiscalPeriodService, FiscalPeriodService>();
builder.Services.AddScoped<ITaxService, TaxService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<ITrialBalanceService, TrialBalanceService>();
builder.Services.AddScoped<IMonthEndClosingService, MonthEndClosingService>();
builder.Services.AddScoped<IBankReconciliationService, BankReconciliationService>();
builder.Services.AddScoped<ICashFlowReportService, CashFlowReportService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/", () => "AMS - Accounting Management System");

app.Run();

/// <summary>
/// Partial class for Program to support top-level statements.
/// </summary>
public partial class Program { }