using Npgsql;

var host = "localhost";
var port = 15432;
var dbName = "ams_db";
var username = "postgres";
var password = "123456";

// Connect to default postgres database first to drop/recreate ams_db
var adminConnStr = $"Host={host};Port={port};Database=postgres;Username={username};Password={password}";

Console.WriteLine("========================================");
Console.WriteLine("AMS Database Setup Tool");
Console.WriteLine("========================================\n");

Console.WriteLine("Step 1: Connecting to PostgreSQL server...");
await using var adminConn = new NpgsqlConnection(adminConnStr);
await adminConn.OpenAsync();
Console.WriteLine("  ✓ Connected to PostgreSQL\n");

// Drop existing database
Console.WriteLine($"Step 2: Dropping existing database '{dbName}' if exists...");
try
{
    // Terminate existing connections
    await using var terminateCmd = new NpgsqlCommand(
        $"SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '{dbName}' AND pid <> pg_backend_pid()",
        adminConn);
    await terminateCmd.ExecuteNonQueryAsync();
    
    await using var dropCmd = new NpgsqlCommand($"DROP DATABASE IF EXISTS {dbName}", adminConn);
    await dropCmd.ExecuteNonQueryAsync();
    Console.WriteLine($"  ✓ Database '{dbName}' dropped\n");
}
catch (Exception ex)
{
    Console.WriteLine($"  ⚠ Warning: {ex.Message}\n");
}

// Create new database
Console.WriteLine($"Step 3: Creating new database '{dbName}'...");
await using var createCmd = new NpgsqlCommand($"CREATE DATABASE {dbName}", adminConn);
await createCmd.ExecuteNonQueryAsync();
Console.WriteLine($"  ✓ Database '{dbName}' created\n");

// Now connect to the new database
var connStr = $"Host={host};Port={port};Database={dbName};Username={username};Password={password}";

Console.WriteLine($"Step 4: Connecting to '{dbName}'...");
await using var conn = new NpgsqlConnection(connStr);
await conn.OpenAsync();
Console.WriteLine("  ✓ Connected\n");

// Create schemas first
Console.WriteLine("Step 5: Creating schemas...");
var schemas = new[] { "acc", "tax", "audit", "cfg", "rpt" };
foreach (var schema in schemas)
{
    await ExecuteSql(conn, $"CREATE SCHEMA {schema}");
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
    account_number INT,
    account_level INT NOT NULL,
    parent_account_id UUID,
    account_type VARCHAR(20) NOT NULL,
    is_detail_account BOOLEAN NOT NULL DEFAULT FALSE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    description VARCHAR(500),
    allow_entry BOOLEAN NOT NULL DEFAULT TRUE,
    bank_account VARCHAR(50),
    bank_name VARCHAR(255),
    currency_code CHAR(3),
    opening_balance VARCHAR(50),
    is_bank_account BOOLEAN NOT NULL DEFAULT FALSE,
    is_tax_account BOOLEAN NOT NULL DEFAULT FALSE,
    is_vat_account BOOLEAN NOT NULL DEFAULT FALSE,
    vat_rate_code VARCHAR(20),
    is_revenue_sharing BOOLEAN NOT NULL DEFAULT FALSE,
    revenue_sharing_percentage VARCHAR(20),
    reconciliation_account VARCHAR(50)
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
    row_version BYTEA,
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

// Create FixedAssets table
await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.fixed_assets (
    asset_id UUID PRIMARY KEY,
    asset_code VARCHAR(20) NOT NULL UNIQUE,
    asset_name VARCHAR(255) NOT NULL,
    asset_group_id UUID,
    serial_number VARCHAR(100),
    model VARCHAR(100),
    account_id UUID,
    original_cost NUMERIC(18, 0) NOT NULL DEFAULT 0,
    residual_value NUMERIC(18, 0),
    useful_life_months INT NOT NULL,
    acquisition_date DATE NOT NULL,
    depreciation_start_date DATE,
    status VARCHAR(20) NOT NULL DEFAULT 'Active',
    depreciation_method VARCHAR(50) DEFAULT 'STRAIGHT_LINE',
    accumulated_depreciation NUMERIC(18, 0),
    book_value NUMERIC(18, 0),
    department_code VARCHAR(20),
    description VARCHAR(500),
    created_at TIMESTAMP NOT NULL,
    created_by VARCHAR(100) NOT NULL,
    modified_at TIMESTAMP,
    modified_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE
)
""");

// Create VATInputRegisters table
await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.vat_input_registers (
    id UUID PRIMARY KEY,
    voucher_id UUID NOT NULL,
    vendor_id UUID NOT NULL,
    fiscal_period_id UUID NOT NULL,
    invoice_no VARCHAR(50) NOT NULL,
    invoice_date DATE NOT NULL,
    total_amount NUMERIC(18, 0) NOT NULL DEFAULT 0,
    vat_amount NUMERIC(18, 0) NOT NULL DEFAULT 0,
    vat_rate NUMERIC(5, 4) NOT NULL DEFAULT 0,
    goods_amount NUMERIC(18, 0) NOT NULL DEFAULT 0,
    is_claimed BOOLEAN NOT NULL DEFAULT FALSE,
    claimed_date TIMESTAMP,
    created_at TIMESTAMP NOT NULL,
    created_by VARCHAR(100) NOT NULL,
    modified_at TIMESTAMP,
    modified_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE
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
Console.WriteLine("Step 6: Database setup complete!");
Console.WriteLine("========================================");
Console.WriteLine($"\nDatabase: {dbName}");
Console.WriteLine($"Host: {host}:{port}");
Console.WriteLine("\nYou can now run the AMS application.\n");

async Task ExecuteSql(NpgsqlConnection connection, string sql)
{
    try
    {
        await using var cmd = new NpgsqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync();
        
        // Create a friendly preview message
        var lines = sql.Split('\n');
        var firstLine = lines[0].Trim();
        if (firstLine.StartsWith("CREATE SCHEMA"))
            Console.WriteLine($"  ✓ Schema: {firstLine.Replace("CREATE SCHEMA ", "")}");
        else if (firstLine.StartsWith("CREATE TABLE"))
            Console.WriteLine($"  ✓ Table: {firstLine.Replace("CREATE TABLE IF NOT EXISTS ", "").Replace("acc.", "")}");
        else if (firstLine.StartsWith("CREATE INDEX"))
            Console.WriteLine($"  ✓ Index created");
        else
            Console.WriteLine($"  ✓ Executed");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ✗ Error: {ex.Message}");
    }
}