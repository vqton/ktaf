namespace AMS.Application.Common.Constants;

/// <summary>
/// Application-wide constants.
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Default currency code for the application.
    /// </summary>
    public const string DefaultCurrency = "VND";

    /// <summary>
    /// Default page size for paginated queries.
    /// </summary>
    public const int DefaultPageSize = 50;

    /// <summary>
    /// Maximum page size allowed for paginated queries.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Standard date format for display (Vietnamese format).
    /// </summary>
    public const string DateFormat = "dd/MM/yyyy";

    /// <summary>
    /// Standard date-time format for display.
    /// </summary>
    public const string DateTimeFormat = "dd/MM/yyyy HH:mm:ss";

    /// <summary>
    /// Date format for database storage (ISO 8601 date).
    /// </summary>
    public const string DateFormatForDatabase = "yyyy-MM-dd";

    /// <summary>
    /// Monetary threshold requiring director approval (50 million VND).
    /// </summary>
    public const decimal DirectorApprovalThreshold = 50_000_000m;

    /// <summary>
    /// Maximum number of lines allowed per voucher document.
    /// </summary>
    public const int MaxVoucherLinesPerDocument = 500;

    /// <summary>
    /// Maximum file size for attachments in MB.
    /// </summary>
    public const int MaxAttachmentSizeMB = 10;

    /// <summary>
    /// Default personal deduction for PIT calculation (giảm trừ gia cảnh) - 11,000,000 VND/month as per TT 99/2025.
    /// </summary>
    public const long DefaultPersonalDeduction = 11_000_000;

    /// <summary>
    /// Cache key constants.
    /// </summary>
    public static class CacheKeys
    {
        /// <summary>
        /// Cache key for all chart of accounts.
        /// </summary>
        public const string ChartOfAccounts = "chart_of_accounts_all";

        /// <summary>
        /// Cache key for all tax rates.
        /// </summary>
        public const string TaxRates = "tax_rates_all";

        /// <summary>
        /// Cache key for PIT brackets.
        /// </summary>
        public const string PITBrackets = "pit_brackets";

        /// <summary>
        /// Cache key for fiscal periods.
        /// </summary>
        public const string FiscalPeriods = "fiscal_periods";
    }

    /// <summary>
    /// Cache duration constants in minutes.
    /// </summary>
    public static class CacheDurations
    {
        /// <summary>
        /// Chart of accounts cache duration in minutes.
        /// </summary>
        public const int ChartOfAccountsMinutes = 30;

        /// <summary>
        /// Tax rates cache duration in minutes.
        /// </summary>
        public const int TaxRatesMinutes = 5;

        /// <summary>
        /// PIT brackets cache duration in minutes.
        /// </summary>
        public const int PITBracketsMinutes = 60;

        /// <summary>
        /// Fiscal periods cache duration in minutes.
        /// </summary>
        public const int FiscalPeriodsMinutes = 15;
    }
}