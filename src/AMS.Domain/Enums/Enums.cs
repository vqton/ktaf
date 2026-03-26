namespace AMS.Domain.Enums;

/// <summary>
/// Types of accounting vouchers/documents used in the system.
/// </summary>
public enum VoucherType
{
    /// <summary>Phiếu thu - Receipt voucher</summary>
    PT,
    /// <summary>Phiếu chi - Payment voucher</summary>
    PC,
    /// <summary>Báo có - Credit note</summary>
    BC,
    /// <summary>Báo nợ - Debit note</summary>
    BN,
    /// <summary>Hóa đơn - Invoice</summary>
    HB,
    /// <summary>Hợp đồng - Contract</summary>
    HM,
    /// <summary>Nhập kho - Goods receipt</summary>
    NK,
    /// <summary>Xuất kho - Goods issue</summary>
    XK,
    /// <summary>Phiếu báo - Notice</summary>
    PB,
    /// <summary>Kết chuyển - Transfer/Allocation</summary>
    KC,
    /// <summary>Bút toán - Journal entry</summary>
    BT
}

/// <summary>
/// Status lifecycle of an accounting voucher.
/// </summary>
public enum VoucherStatus
{
    /// <summary>Voucher is being edited and not yet submitted.</summary>
    Draft,
    /// <summary>Voucher has been submitted and awaiting approval.</summary>
    Pending,
    /// <summary>Voucher has been approved and ready for posting.</summary>
    Approved,
    /// <summary>Voucher has been posted to the general ledger.</summary>
    Posted,
    /// <summary>Voucher has been reversed by another voucher.</summary>
    Reversed
}

/// <summary>
/// Status of a fiscal accounting period.
/// </summary>
public enum FiscalPeriodStatus
{
    /// <summary>Period is open for posting vouchers.</summary>
    Open,
    /// <summary>Period has been closed and no further posting allowed.</summary>
    Closed,
    /// <summary>Period is locked and cannot be modified.</summary>
    Locked
}

/// <summary>
/// Classification types for chart of accounts.
/// </summary>
public enum AccountType
{
    /// <summary>Tài sản - Assets (1xx accounts)</summary>
    Asset,
    /// <summary>Nợ phải trả - Liabilities (2xx accounts)</summary>
    Liability,
    /// <summary>Vốn chủ sở hữu - Equity (3xx accounts)</summary>
    Equity,
    /// <summary>Doanh thu - Revenue (4xx, 5xx, 7xx accounts)</summary>
    Revenue,
    /// <summary>Chi phí - Expense (6xx, 8xx accounts)</summary>
    Expense
}

/// <summary>
/// Types of taxes in the Vietnamese tax system.
/// </summary>
public enum TaxType
{
    /// <summary>Giá trị gia tăng - Value Added Tax</summary>
    GTGT,
    /// <summary>Thu nhập doanh nghiệp - Corporate Income Tax</summary>
    TNDN,
    /// <summary>Thu nhập cá nhân - Personal Income Tax</summary>
    TNCN,
    /// <summary>Tiêu thụ đặc biệt - Special Consumption Tax</summary>
    TTDB,
    /// <summary>Nhà thầu nước ngoài - Foreign Contractor Tax</summary>
    NTC
}

/// <summary>
/// Types of products or services.
/// </summary>
public enum ProductType
{
    /// <summary>Hàng hóa - Physical goods for sale</summary>
    Product,
    /// <summary>Dịch vụ - Services rendered</summary>
    Service,
    /// <summary>Tài sản cố định - Fixed assets</summary>
    FixedAsset
}

/// <summary>
/// Types of inventory transactions.
/// </summary>
public enum InventoryTransactionType
{
    /// <summary>Nhập mua - Purchase receipt</summary>
    PurchaseIn,
    /// <summary>Xuất bán - Sales delivery</summary>
    SaleOut,
    /// <summary>Xuất trả - Return to vendor</summary>
    ReturnOut,
    /// <summary>Nhập trả - Return from customer</summary>
    ReturnIn,
    /// <summary>Chuyển kho - Transfer between warehouses</summary>
    Transfer,
    /// <summary>Kiểm kê - Inventory count adjustment</summary>
    Adjustment,
    /// <summary>Xuất mẫu - Sample output</summary>
    SampleOut,
    /// <summary>Xuất sử dụng nội bộ - Internal use</summary>
    InternalOut
}

/// <summary>
/// Inventory costing methods for inventory valuation.
/// </summary>
public enum PricingMethod
{
    /// <summary>First In First Out - Earliest inventory items sold first</summary>
    FIFO,
    /// <summary>Bình quân gia quyền - Weighted average cost</summary>
    BQGQ
}

/// <summary>
/// Status of a fixed asset in its lifecycle.
/// </summary>
public enum AssetStatus
{
    /// <summary>Asset is currently in use and being depreciated</summary>
    Active,
    /// <summary>Asset is under repair and not in use</summary>
    Repair,
    /// <summary>Asset is idle and not in use</summary>
    Idle,
    /// <summary>Asset has been disposed and no longer owned</summary>
    Disposed
}

/// <summary>
/// Adjustment flags for Corporate Income Tax non-deductible expenses.
/// </summary>
public enum CITAdjFlag
{
    /// <summary>Quà tặng > 2tr - Gifts exceeding 2 million VND</summary>
    GIFT,
    /// <summary>Tiệc không HĐ - Entertainment without contract</summary>
    MEAL,
    /// <summary>Chi phí cá nhân - Personal expenses</summary>
    PERSONAL,
    /// <summary>Lãi vay vượt mức - Excess interest expense</summary>
    INTEREST_EXCESS,
    /// <summary>Chi tiếp khách vượt định mức - Entertainment expenses exceeding limit</summary>
    ENTERTAINMENT
}

/// <summary>
/// Types of database transactions/operations for audit logging.
/// </summary>
public enum TransactionType
{
    /// <summary>New record created</summary>
    INSERT,
    /// <summary>Existing record updated</summary>
    UPDATE,
    /// <summary>Record deleted (soft or hard)</summary>
    DELETE,
    /// <summary>Voucher or record approved</summary>
    APPROVE,
    /// <summary>Voucher or record rejected</summary>
    REJECT,
    /// <summary>Voucher posted to general ledger</summary>
    POST,
    /// <summary>Voucher reversed by another voucher</summary>
    REVERSE,
    /// <summary>Voucher submitted for approval</summary>
    SUBMIT
}

/// <summary>
/// Status of a tax declaration submission.
/// </summary>
public enum TaxDeclarationStatus
{
    /// <summary>Tax declaration is being prepared</summary>
    Draft,
    /// <summary>Tax declaration has been submitted to tax authority</summary>
    Submitted,
    /// <summary>Tax declaration has been accepted by tax authority</summary>
    Accepted,
    /// <summary>Tax declaration has been amended after submission</summary>
    Amended
}
