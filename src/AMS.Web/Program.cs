using System.Text;
using AMS.Application.Interfaces;
using AMS.Application.Services;
using AMS.Application.Accounting.Vouchers.Interfaces;
using AMS.Application.Accounting.Vouchers.Services;
using AMS.Domain.Interfaces;
using AMS.Infrastructure;
using AMS.Infrastructure.Data;
using AMS.Infrastructure.Repositories;
using AMS.Web.Hubs;
using AMS.Web.Services;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.SignalR;
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
builder.Services.AddScoped<IFixedAssetRepository, FixedAssetRepository>();
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
builder.Services.AddScoped<IReceivableRepository, ReceivableRepository>();
builder.Services.AddScoped<IPayableRepository, PayableRepository>();
builder.Services.AddScoped<IReceivablePaymentRepository, ReceivablePaymentRepository>();
builder.Services.AddScoped<IPayablePaymentRepository, PayablePaymentRepository>();
builder.Services.AddScoped<IAgingReportRepository, AgingReportRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IInventoryReportService, InventoryReportService>();
builder.Services.AddScoped<ICostCenterRepository, CostCenterRepository>();
builder.Services.AddScoped<ICostAllocationRepository, CostAllocationRepository>();
builder.Services.AddScoped<ICostAllocationDetailRepository, CostAllocationDetailRepository>();
builder.Services.AddScoped<ICostReportRepository, CostReportRepository>();
builder.Services.AddScoped<ICostAccountingService, CostAccountingService>();
builder.Services.AddScoped<IRevenueRepository, RevenueRepository>();
builder.Services.AddScoped<IRevenueRecognitionRepository, RevenueRecognitionRepository>();
builder.Services.AddScoped<IRevenueReportRepository, RevenueReportRepository>();
builder.Services.AddScoped<IRevenueService, RevenueService>();
builder.Services.AddScoped<IFinancialReportRepository, FinancialReportRepository>();
builder.Services.AddScoped<IFinancialReportService, FinancialReportService>();
builder.Services.AddControllers();

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
builder.Services.AddScoped<IReceivablePayableService, ReceivablePayableService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IADGroupRepository, ADGroupRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IUserADGroupRepository, UserADGroupRepository>();
builder.Services.AddScoped<IADGroupRoleRepository, ADGroupRoleRepository>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

// Export services
builder.Services.AddScoped<IExportService, ExportService>();

builder.Services.AddControllersWithViews();

// Authentication - Windows Authentication via Negotiate
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin", "SystemAdmin"));

    options.AddPolicy("RequireAccountantRole", policy =>
        policy.RequireRole("Admin", "Accountant"));

    options.AddPolicy("RequireViewerRole", policy =>
        policy.RequireRole("Admin", "Accountant", "Viewer"));
});

// Hangfire Scheduler
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(connectionString);
    }));
builder.Services.AddHangfireServer();

// SignalR for real-time notifications
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 1024 * 1024;
});

builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
builder.Services.AddSingleton<INotificationService, SignalRNotificationService>();

// HttpClient for export services
builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "AMS Scheduler",
    Authorization = new[] { new HangfireAuthorizationFilter() },
    StatsPollingInterval = 5000
});

app.UseAuthentication();
app.UseAuthorization();

// SignalR Hub endpoint
app.MapHub<NotificationHub>("/hubs/notifications");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// API endpoints
app.MapControllers();

// Register Hangfire recurring jobs
RegisterRecurringJobs();

app.Run();

void RegisterRecurringJobs()
{
    // Example recurring job - can be customized based on requirements
    RecurringJob.AddOrUpdate<SampleBackgroundJob>(
        "sample-daily-job",
        job => job.ExecuteAsync(CancellationToken.None),
        Cron.Daily);
}

/// <summary>
/// User ID provider for SignalR.
/// </summary>
public class NameUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.Identity?.Name;
    }
}

/// <summary>
/// Authorization filter for Hangfire dashboard.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User?.Identity?.IsAuthenticated == true;
    }
}

/// <summary>
/// Sample background job for demonstration.
/// </summary>
public class SampleBackgroundJob
{
    private readonly ILogger<SampleBackgroundJob> _logger;
    private readonly INotificationService _notificationService;

    public SampleBackgroundJob(ILogger<SampleBackgroundJob> logger, INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sample daily job started at {Time}", DateTime.UtcNow);
        await _notificationService.SendJobCompletionNotificationAsync("Sample Daily Job", true);
        _logger.LogInformation("Sample daily job completed at {Time}", DateTime.UtcNow);
    }
}

/// <summary>
/// Partial class for Program to support top-level statements.
/// </summary>
public partial class Program { }
