using Npgsql;

var connStr = "Host=localhost;Port=15432;Database=ams_db;Username=postgres;Password=123456";

await using var conn = new NpgsqlConnection(connStr);
await conn.OpenAsync();

Console.WriteLine("Connected to PostgreSQL.\n");

// Create schemas first
Console.WriteLine("Creating schemas...");
var schemas = new[] { "acc", "tax", "audit", "cfg", "rpt" };
foreach (var schema in schemas)
{
    await ExecuteSql(conn, $"CREATE SCHEMA IF NOT EXISTS {schema}");
}

Console.WriteLine("\nCreating tables...");

// Create FiscalPeriods table
await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.fiscal_periods (
    fiscal_period_id UUID PRIMARY KEY,
    year INT NOT NULL,
    month INT NOT NULL,
    status VARCHAR(15) NOT NULL DEFAULT 'Open',
    closed_at TIMESTAMP,
    closed_by VARCHAR(100),
    UNIQUE(year, month)
)
""");

// Create ChartOfAccounts table
await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.chart_of_accounts (
    account_id UUID PRIMARY KEY,
    account_code VARCHAR(20) NOT NULL UNIQUE,
    account_name VARCHAR(255) NOT NULL,
    account_level INT NOT NULL,
    parent_account_id UUID,
    account_type VARCHAR(20) NOT NULL,
    is_detail_account BOOLEAN NOT NULL DEFAULT FALSE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    description VARCHAR(500)
)
""");

// Create Vouchers table
await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.vouchers (
    voucher_id UUID PRIMARY KEY,
    voucher_no VARCHAR(25) NOT NULL,
    voucher_type VARCHAR(10) NOT NULL,
    voucher_date DATE NOT NULL,
    fiscal_period_id UUID NOT NULL,
    description VARCHAR(500),
    status VARCHAR(15) NOT NULL DEFAULT 'Draft',
    total_debit NUMERIC(18, 0) NOT NULL DEFAULT 0,
    total_credit NUMERIC(18, 0) NOT NULL DEFAULT 0,
    currency_code CHAR(3) NOT NULL DEFAULT 'VND',
    exchange_rate NUMERIC(18, 4) NOT NULL DEFAULT 1,
    created_at TIMESTAMP NOT NULL,
    created_by VARCHAR(100) NOT NULL,
    modified_at TIMESTAMP,
    modified_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE
)
""");

// Create VoucherLines table
await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.voucher_lines (
    voucher_line_id UUID PRIMARY KEY,
    voucher_id UUID NOT NULL,
    account_id UUID NOT NULL,
    debit_amount NUMERIC(18, 0) NOT NULL DEFAULT 0,
    credit_amount NUMERIC(18, 0) NOT NULL DEFAULT 0,
    description VARCHAR(500),
    customer_id UUID,
    vendor_id UUID,
    is_excise_tax_line BOOLEAN NOT NULL DEFAULT FALSE,
    cit_adj_flag VARCHAR(50),
    FOREIGN KEY (voucher_id) REFERENCES acc.vouchers(voucher_id) ON DELETE CASCADE
)
""");

// Create Indexes
Console.WriteLine("\nCreating indexes...");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_vouchers_voucher_no ON acc.vouchers(voucher_no)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_vouchers_voucher_date ON acc.vouchers(voucher_date)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_vouchers_status ON acc.vouchers(status)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_vouchers_fiscal_period ON acc.vouchers(fiscal_period_id)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_voucher_lines_voucher_id ON acc.voucher_lines(voucher_id)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_voucher_lines_account_id ON acc.voucher_lines(account_id)");

// Create audit_logs table
await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS audit.audit_logs (
    audit_log_id BIGSERIAL PRIMARY KEY,
    event_time TIMESTAMP NOT NULL DEFAULT NOW(),
    user_name VARCHAR(100) NOT NULL,
    client_ip VARCHAR(45),
    table_name VARCHAR(100) NOT NULL,
    record_id VARCHAR(50) NOT NULL,
    action VARCHAR(10) NOT NULL,
    old_values JSONB,
    new_values JSONB,
    module VARCHAR(50)
)
""");

await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_audit_logs_event_time ON audit.audit_logs(event_time)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_audit_logs_user_name ON audit.audit_logs(user_name)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_audit_logs_table_name ON audit.audit_logs(table_name)");

Console.WriteLine("\n========================================");
Console.WriteLine("Database setup complete!");
Console.WriteLine("========================================\n");

async Task ExecuteSql(NpgsqlConnection connection, string sql)
{
    try
    {
        await using var cmd = new NpgsqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync();
        var preview = sql.Split('(')[0].Trim().Replace("CREATE", "").Replace("INDEX", "INDEX").Trim();
        Console.WriteLine($"  ✓ {preview}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ✗ Error: {ex.Message}");
    }
}