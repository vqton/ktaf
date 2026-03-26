using Microsoft.AspNetCore.Mvc;
using AMS.Web.Common;
using AMS.Application.Interfaces;
using AMS.Application.DTOs;
using System.Text;

namespace AMS.Web.Controllers;

public class ReportsController : BaseController
{
    private readonly IFinancialReportService _financialReportService;
    private readonly ITrialBalanceService _trialBalanceService;
    private readonly ILedgerService _ledgerService;
    private readonly IFiscalPeriodService _fiscalPeriodService;

    public ReportsController(
        IFinancialReportService financialReportService,
        ITrialBalanceService trialBalanceService,
        ILedgerService ledgerService,
        IFiscalPeriodService fiscalPeriodService)
    {
        _financialReportService = financialReportService;
        _trialBalanceService = trialBalanceService;
        _ledgerService = ledgerService;
        _fiscalPeriodService = fiscalPeriodService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> TrialBalance(int year, int month)
    {
        var fiscalPeriod = await _fiscalPeriodService.GetByYearMonthAsync(year, month);
        if (fiscalPeriod == null)
            return NotFound($"Không tìm thấy kỳ kế toán {year}/{month}");

        var result = await _trialBalanceService.GetTrialBalanceAsync(fiscalPeriod.Id);
        return View(result);
    }

    public async Task<IActionResult> BalanceSheet(int year, int month)
    {
        var result = await _financialReportService.GetBalanceSheetAsync(year, month);
        if (!result.IsSuccess)
            return Failure(result.ErrorMessage ?? "Lỗi khi lấy bảng cân đối kế toán");

        return View(result.Data);
    }

    public async Task<IActionResult> IncomeStatement(int year, int month)
    {
        var result = await _financialReportService.GetIncomeStatementAsync(year, month);
        if (!result.IsSuccess)
            return Failure(result.ErrorMessage ?? "Lỗi khi lấy báo cáo kết quả kinh doanh");

        return View(result.Data);
    }

    public async Task<IActionResult> CashFlowStatement(int year, int month)
    {
        var result = await _financialReportService.GetCashFlowStatementAsync(year, month);
        if (!result.IsSuccess)
            return Failure(result.ErrorMessage ?? "Lỗi khi lấy báo cáo lưu chuyển tiền tệ");

        return View(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> GenerateBalanceSheet(int year, int month)
    {
        var result = await _financialReportService.GenerateBalanceSheetAsync(year, month);
        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> GenerateIncomeStatement(int year, int month)
    {
        var result = await _financialReportService.GenerateIncomeStatementAsync(year, month);
        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> GenerateCashFlowStatement(int year, int month)
    {
        var result = await _financialReportService.GenerateCashFlowStatementAsync(year, month);
        return FromResult(result);
    }

    public async Task<IActionResult> GeneralLedger(Guid accountId, DateTime fromDate, DateTime toDate)
    {
        var entries = await _ledgerService.GetByAccountAsync(accountId, CancellationToken.None);
        return View(entries);
    }

    public async Task<IActionResult> GetAllReports(int year, int month)
    {
        var result = await _financialReportService.GetAllReportsAsync(year, month);
        if (!result.IsSuccess)
            return Failure(result.ErrorMessage ?? "Lỗi khi lấy danh sách báo cáo");

        return View(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> ExportTrialBalance(Guid fiscalPeriodId, string format)
    {
        var data = await _trialBalanceService.GetTrialBalanceAsync(fiscalPeriodId);
        
        if (data == null || !data.Any())
            return Failure("Không có dữ liệu");

        if (format.ToLower() == "excel")
        {
            var csv = ExportTrialBalanceToCsv(data);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"TrialBalance_{fiscalPeriodId}.csv");
        }
        else if (format.ToLower() == "pdf")
        {
            return Failure("PDF export chưa được triển khai");
        }

        return Failure("Định dạng không hỗ trợ");
    }

    [HttpGet]
    public async Task<IActionResult> ExportBalanceSheet(int year, int month, string format)
    {
        var result = await _financialReportService.GetBalanceSheetAsync(year, month);
        if (!result.IsSuccess)
            return Failure("Lỗi khi xuất báo cáo");

        if (format.ToLower() == "excel")
        {
            var csv = ExportToCsv(result.Data);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"BalanceSheet_{year}_{month}.csv");
        }

        return Failure("Định dạng không hỗ trợ");
    }

    [HttpGet]
    public async Task<IActionResult> ExportIncomeStatement(int year, int month, string format)
    {
        var result = await _financialReportService.GetIncomeStatementAsync(year, month);
        if (!result.IsSuccess)
            return Failure("Lỗi khi xuất báo cáo");

        if (format.ToLower() == "excel")
        {
            var csv = ExportToCsv(result.Data);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"IncomeStatement_{year}_{month}.csv");
        }

        return Failure("Định dạng không hỗ trợ");
    }

    [HttpGet]
    public async Task<IActionResult> ExportCashFlowStatement(int year, int month, string format)
    {
        var result = await _financialReportService.GetCashFlowStatementAsync(year, month);
        if (!result.IsSuccess)
            return Failure("Lỗi khi xuất báo cáo");

        if (format.ToLower() == "excel")
        {
            var csv = ExportToCsv(result.Data);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"CashFlowStatement_{year}_{month}.csv");
        }

        return Failure("Định dạng không hỗ trợ");
    }

    [HttpGet]
    public async Task<IActionResult> ExportLedger(Guid accountId, string fromDate, string toDate, string format = "excel")
    {
        var entries = await _ledgerService.GetByAccountAsync(accountId, CancellationToken.None);
        
        if (entries == null || !entries.Any())
            return Failure("Không có dữ liệu");

        var sb = new StringBuilder();
        sb.AppendLine("Ngày,Số CT,Diễn giải,Nợ,Có,Số dư");
        
        foreach (var item in entries)
        {
            sb.AppendLine($"{item.VoucherDate:dd/MM/yyyy},{item.VoucherNo},{item.Description},{item.DebitAmount},{item.CreditAmount},");
        }

        var fileName = $"GeneralLedger_{accountId}_{DateTime.Now:yyyyMMddHHmmss}.csv";
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv; charset=utf-8", fileName);
    }

    private static string ExportToCsv(object data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("AccountCode,AccountName,Debit,Credit");
        
        var type = data.GetType();
        var properties = type.GetProperties();
        
        foreach (var prop in properties)
        {
            var value = prop.GetValue(data);
            sb.AppendLine($"{prop.Name},{value}");
        }

        return sb.ToString();
    }

    private static string ExportTrialBalanceToCsv(IEnumerable<LedgerSummaryDto> data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("AccountCode,AccountName,OpeningDebit,OpeningCredit,DebitTurnover,CreditTurnover,ClosingDebit,ClosingCredit");
        
        foreach (var item in data)
        {
            sb.AppendLine($"{item.AccountCode},{item.AccountName},{item.OpeningDebit},{item.OpeningCredit},{item.DebitTurnover},{item.CreditTurnover},{item.ClosingDebit},{item.ClosingCredit}");
        }

        return sb.ToString();
    }
}
