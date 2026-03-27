using Npgsql;

var host = "localhost";
var port = 15432;
var dbName = "ams_db";
var username = "postgres";
var password = "123456";

var adminConnStr = $"Host={host};Port={port};Database=postgres;Username={username};Password={password}";

Console.WriteLine("========================================");
Console.WriteLine("AMS Database Setup Tool v2.0");
Console.WriteLine("========================================\n");

Console.WriteLine("Step 1: Connecting to PostgreSQL server...");
await using var adminConn = new NpgsqlConnection(adminConnStr);
await adminConn.OpenAsync();
Console.WriteLine("  ✓ Connected to PostgreSQL\n");

Console.WriteLine($"Step 2: Dropping existing database '{dbName}' if exists...");
try
{
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

Console.WriteLine($"Step 3: Creating new database '{dbName}'...");
await using var createCmd = new NpgsqlCommand($"CREATE DATABASE {dbName}", adminConn);
await createCmd.ExecuteNonQueryAsync();
Console.WriteLine($"  ✓ Database '{dbName}' created\n");

var connStr = $"Host={host};Port={port};Database={dbName};Username={username};Password={password}";
Console.WriteLine($"Step 4: Connecting to '{dbName}'...");
await using var conn = new NpgsqlConnection(connStr);
await conn.OpenAsync();
Console.WriteLine("  ✓ Connected\n");

Console.WriteLine("Step 5: Creating schemas...");
var schemas = new[] { "acc", "tax", "audit", "cfg", "rpt" };
foreach (var schema in schemas)
{
    await ExecuteSql(conn, $"CREATE SCHEMA {schema}");
}

Console.WriteLine("\nCreating tables...");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.fiscal_periods (
    fiscal_period_id UUID PRIMARY KEY,
    year INT NOT NULL,
    month INT NOT NULL,
    status VARCHAR(15) NOT NULL DEFAULT 'Open',
    closed_at TIMESTAMP,
    closed_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system',
    modified_at TIMESTAMP,
    modified_by VARCHAR(100),
    UNIQUE(year, month)
)
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.chart_of_accounts (
    account_id UUID PRIMARY KEY,
    account_code VARCHAR(20) NOT NULL UNIQUE,
    account_name VARCHAR(255) NOT NULL,
    account_number INT,
    account_level INT NOT NULL DEFAULT 1,
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
    tax_category VARCHAR(50),
    is_revenue_sharing BOOLEAN NOT NULL DEFAULT FALSE,
    revenue_sharing_percentage VARCHAR(20),
    reconciliation_account VARCHAR(50),
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system',
    modified_at TIMESTAMP,
    modified_by VARCHAR(100)
)
""");

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
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system',
    modified_at TIMESTAMP,
    modified_by VARCHAR(100),
    row_version BYTEA,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE
)
""");

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

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.voucher_attachments (
    attachment_id UUID PRIMARY KEY,
    voucher_id UUID NOT NULL,
    file_name VARCHAR(255),
    file_path VARCHAR(500),
    content_type VARCHAR(100),
    file_size BIGINT,
    description VARCHAR(500),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system',
    FOREIGN KEY (voucher_id) REFERENCES acc.vouchers(voucher_id) ON DELETE CASCADE
)
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.customers (
    customer_id UUID PRIMARY KEY,
    customer_code VARCHAR(20) NOT NULL UNIQUE,
    customer_name VARCHAR(255) NOT NULL,
    tax_code VARCHAR(20),
    address VARCHAR(500),
    phone VARCHAR(20),
    email VARCHAR(100),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    description VARCHAR(500),
    credit_limit NUMERIC(18,0) NOT NULL DEFAULT 0,
    payment_term_days INT NOT NULL DEFAULT 0,
    is_vat_payer BOOLEAN NOT NULL DEFAULT TRUE,
    invoice_type VARCHAR(100),
    province VARCHAR(100),
    district VARCHAR(100),
    ward VARCHAR(100),
    bank_account VARCHAR(50),
    bank_name VARCHAR(255),
    contact_person VARCHAR(255),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system',
    modified_at TIMESTAMP,
    modified_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE
)
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.vendors (
    vendor_id UUID PRIMARY KEY,
    vendor_code VARCHAR(20) NOT NULL UNIQUE,
    vendor_name VARCHAR(255) NOT NULL,
    tax_code VARCHAR(20),
    address VARCHAR(500),
    phone VARCHAR(20),
    email VARCHAR(100),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    description VARCHAR(500),
    credit_limit NUMERIC(18,0) NOT NULL DEFAULT 0,
    payment_term_days INT NOT NULL DEFAULT 0,
    is_vat_payer BOOLEAN NOT NULL DEFAULT TRUE,
    invoice_type VARCHAR(100),
    province VARCHAR(100),
    district VARCHAR(100),
    ward VARCHAR(100),
    bank_account VARCHAR(50),
    bank_name VARCHAR(255),
    contact_person VARCHAR(255),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system',
    modified_at TIMESTAMP,
    modified_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE
)
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.employees (
    employee_id UUID PRIMARY KEY,
    employee_code VARCHAR(20) NOT NULL UNIQUE,
    full_name VARCHAR(255) NOT NULL,
    email VARCHAR(100),
    phone VARCHAR(20),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    ad_username VARCHAR(100),
    department_code VARCHAR(20),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system',
    modified_at TIMESTAMP,
    modified_by VARCHAR(100)
)
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.departments (
    department_id UUID PRIMARY KEY,
    department_code VARCHAR(20) NOT NULL UNIQUE,
    department_name VARCHAR(255) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    parent_department_id UUID,
    description VARCHAR(500),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system',
    modified_at TIMESTAMP,
    modified_by VARCHAR(100)
)
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.banks (
    bank_id UUID PRIMARY KEY,
    bank_code VARCHAR(20) NOT NULL UNIQUE,
    bank_name VARCHAR(255) NOT NULL,
    swift_code VARCHAR(20),
    branch_name VARCHAR(255),
    address VARCHAR(500),
    phone VARCHAR(20),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system'
)
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.bank_accounts (
    bank_account_id UUID PRIMARY KEY,
    bank_id UUID NOT NULL,
    account_number VARCHAR(50) NOT NULL UNIQUE,
    account_name VARCHAR(255) NOT NULL,
    account_type VARCHAR(20),
    currency_code CHAR(3),
    opening_balance NUMERIC(18, 0) NOT NULL DEFAULT 0,
    is_primary BOOLEAN NOT NULL DEFAULT FALSE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    branch_name VARCHAR(255),
    account_holder VARCHAR(255),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system',
    FOREIGN KEY (bank_id) REFERENCES acc.banks(bank_id)
)
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.bank_transactions (
    bank_transaction_id UUID PRIMARY KEY,
    bank_account_id UUID NOT NULL,
    transaction_date DATE,
    transaction_type VARCHAR(20),
    amount NUMERIC(18, 0) NOT NULL DEFAULT 0,
    fee_amount NUMERIC(18, 0) NOT NULL DEFAULT 0,
    description VARCHAR(500),
    reference_number VARCHAR(50),
    status VARCHAR(20),
    is_reconciled BOOLEAN NOT NULL DEFAULT FALSE,
    reconciled_date DATE,
    voucher_id UUID,
    partner_name VARCHAR(255),
    partner_account_number VARCHAR(50),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system',
    FOREIGN KEY (bank_account_id) REFERENCES acc.bank_accounts(bank_account_id)
)
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.cash_books (
    cash_book_id UUID PRIMARY KEY,
    cash_book_code VARCHAR(20) NOT NULL UNIQUE,
    cash_book_name VARCHAR(255) NOT NULL,
    is_main BOOLEAN NOT NULL DEFAULT FALSE,
    currency_code CHAR(3),
    opening_balance NUMERIC(18, 0) NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    description VARCHAR(500),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system')
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.bank_reconciliations (
    bank_reconciliation_id UUID PRIMARY KEY,
    bank_account_id UUID NOT NULL,
    year INT NOT NULL,
    month INT NOT NULL,
    statement_closing_balance NUMERIC(18, 0) NOT NULL DEFAULT 0,
    book_closing_balance NUMERIC(18, 0) NOT NULL DEFAULT 0,
    in_transit_deposits NUMERIC(18, 0) NOT NULL DEFAULT 0,
    unrecorded_bank_fees NUMERIC(18, 0) NOT NULL DEFAULT 0,
    unrecorded_interest NUMERIC(18, 0) NOT NULL DEFAULT 0,
    reconciliation_date DATE,
    status VARCHAR(20),
    notes VARCHAR(1000),
    prepared_by VARCHAR(100),
    approved_by VARCHAR(100),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system',
    UNIQUE(bank_account_id, year, month),
    FOREIGN KEY (bank_account_id) REFERENCES acc.bank_accounts(bank_account_id)
)
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.products (
    product_id UUID PRIMARY KEY,
    product_code VARCHAR(20) NOT NULL UNIQUE,
    product_name VARCHAR(255) NOT NULL,
    product_name_en VARCHAR(255),
    product_group_id UUID,
    product_type VARCHAR(20),
    unit_of_measure VARCHAR(50),
    unit_price NUMERIC(18,2),
    currency_code CHAR(3) DEFAULT 'VND',
    vat_rate NUMERIC(5,4),
    is_excise_tax_item BOOLEAN NOT NULL DEFAULT FALSE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    tax_code VARCHAR(20),
    warehouse_id UUID,
    specification VARCHAR(500),
    origin VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system',
    modified_at TIMESTAMP,
    modified_by VARCHAR(100))
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.warehouses (
    warehouse_id UUID PRIMARY KEY,
    warehouse_code VARCHAR(20) NOT NULL UNIQUE,
    warehouse_name VARCHAR(255) NOT NULL,
    address VARCHAR(500),
    manager VARCHAR(100),
    pricing_method VARCHAR(20) DEFAULT 'AVCO',
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system',
    modified_at TIMESTAMP,
    modified_by VARCHAR(100))
""");

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
    useful_life_months INT NOT NULL DEFAULT 0,
    acquisition_date DATE NOT NULL,
    depreciation_start_date DATE,
    status VARCHAR(20) NOT NULL DEFAULT 'Active',
    depreciation_method VARCHAR(50),
    accumulated_depreciation NUMERIC(18, 0),
    book_value NUMERIC(18, 0),
    department_code VARCHAR(20),
    description VARCHAR(500),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system',
    modified_at TIMESTAMP,
    modified_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE
)
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.asset_groups (
    group_id UUID PRIMARY KEY,
    group_code VARCHAR(20) NOT NULL UNIQUE,
    group_name VARCHAR(255) NOT NULL,
    depreciation_method VARCHAR(50),
    useful_life_months INT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    description VARCHAR(500),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system')
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.ledger_entries (
    ledger_entry_id UUID PRIMARY KEY,
    fiscal_period_id UUID NOT NULL,
    voucher_id UUID,
    voucher_no VARCHAR(25),
    voucher_date DATE,
    account_id UUID NOT NULL,
    account_code VARCHAR(20),
    debit_amount NUMERIC(18, 0) NOT NULL DEFAULT 0,
    credit_amount NUMERIC(18, 0) NOT NULL DEFAULT 0,
    description VARCHAR(500),
    currency_code CHAR(3),
    exchange_rate NUMERIC(18, 4)
)
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.account_balances (
    balance_id UUID PRIMARY KEY,
    fiscal_period_id UUID NOT NULL,
    account_id UUID NOT NULL,
    account_code VARCHAR(20),
    opening_debit NUMERIC(18, 0) NOT NULL DEFAULT 0,
    opening_credit NUMERIC(18, 0) NOT NULL DEFAULT 0,
    period_debit NUMERIC(18, 0) NOT NULL DEFAULT 0,
    period_credit NUMERIC(18, 0) NOT NULL DEFAULT 0,
    closing_debit NUMERIC(18, 0) NOT NULL DEFAULT 0,
    closing_credit NUMERIC(18, 0) NOT NULL DEFAULT 0,
    UNIQUE(fiscal_period_id, account_id)
)
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.exchange_rates (
    exchange_rate_id UUID PRIMARY KEY,
    currency_code CHAR(3) NOT NULL,
    exchange_date DATE NOT NULL,
    rate NUMERIC(18, 6) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(currency_code, exchange_date)
)
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.receivables (
    receivable_id UUID PRIMARY KEY,
    customer_id UUID NOT NULL,
    fiscal_period_id UUID NOT NULL,
    voucher_id UUID,
    invoice_no VARCHAR(50),
    invoice_date DATE,
    due_date DATE,
    total_amount NUMERIC(18, 0) NOT NULL DEFAULT 0,
    paid_amount NUMERIC(18, 0) NOT NULL DEFAULT 0,
    outstanding_amount NUMERIC(18, 0) NOT NULL DEFAULT 0,
    currency_code CHAR(3),
    status VARCHAR(20),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system')
""");

await ExecuteSql(conn, """
CREATE TABLE IF NOT EXISTS acc.payables (
    payable_id UUID PRIMARY KEY,
    vendor_id UUID NOT NULL,
    fiscal_period_id UUID NOT NULL,
    voucher_id UUID,
    invoice_no VARCHAR(50),
    invoice_date DATE,
    due_date DATE,
    total_amount NUMERIC(18, 0) NOT NULL DEFAULT 0,
    paid_amount NUMERIC(18, 0) NOT NULL DEFAULT 0,
    outstanding_amount NUMERIC(18, 0) NOT NULL DEFAULT 0,
    currency_code CHAR(3),
    status VARCHAR(20),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100) NOT NULL DEFAULT 'system')
""");

await ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS acc.user_roles (user_id UUID NOT NULL, role_id UUID NOT NULL, assigned_at TIMESTAMP NOT NULL DEFAULT NOW(), assigned_by VARCHAR(100), PRIMARY KEY (user_id, role_id))");
await ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS acc.ad_groups (ad_group_id UUID PRIMARY KEY, group_name VARCHAR(255) NOT NULL UNIQUE, display_name VARCHAR(255), description VARCHAR(500), is_active BOOLEAN NOT NULL DEFAULT TRUE, created_at TIMESTAMP NOT NULL DEFAULT NOW())");
await ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS acc.user_ad_groups (user_id UUID NOT NULL, ad_group_id UUID NOT NULL, synced_at TIMESTAMP, PRIMARY KEY (user_id, ad_group_id))");
await ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS acc.ad_group_roles (ad_group_id UUID NOT NULL, role_id UUID NOT NULL, PRIMARY KEY (ad_group_id, role_id))");
await ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS acc.permissions (permission_id UUID PRIMARY KEY, permission_name VARCHAR(100) NOT NULL UNIQUE, description VARCHAR(500), module VARCHAR(50), is_active BOOLEAN NOT NULL DEFAULT TRUE, created_at TIMESTAMP NOT NULL DEFAULT NOW())");
await ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS acc.role_permissions (role_id UUID NOT NULL, permission_id UUID NOT NULL, PRIMARY KEY (role_id, permission_id))");

await ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS tax.tax_rates (tax_rate_id UUID PRIMARY KEY, tax_rate_key VARCHAR(50) NOT NULL UNIQUE, tax_type VARCHAR(20) NOT NULL, rate NUMERIC(5, 4), effective_from DATE, effective_to DATE, created_at TIMESTAMP NOT NULL DEFAULT NOW(), created_by VARCHAR(100) NOT NULL DEFAULT 'system')");

await ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS tax.pit_brackets (pit_bracket_id UUID PRIMARY KEY, bracket_no INT NOT NULL, from_amount NUMERIC(18, 0) NOT NULL, to_amount NUMERIC(18, 0), tax_rate NUMERIC(5, 4) NOT NULL, quick_deduction NUMERIC(18, 0) NOT NULL DEFAULT 0, effective_from DATE NOT NULL, created_at TIMESTAMP NOT NULL DEFAULT NOW(), created_by VARCHAR(100) NOT NULL DEFAULT 'system')");

await ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS tax.pit_allowances (allowance_id UUID PRIMARY KEY, allowance_code VARCHAR(50) NOT NULL UNIQUE, allowance_name VARCHAR(255) NOT NULL, amount NUMERIC(18, 0) NOT NULL, is_active BOOLEAN NOT NULL DEFAULT TRUE, created_at TIMESTAMP NOT NULL DEFAULT NOW())");

await ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS tax.vat_input_registers (id UUID PRIMARY KEY, voucher_id UUID NOT NULL, vendor_id UUID NOT NULL, fiscal_period_id UUID NOT NULL, invoice_no VARCHAR(50) NOT NULL, invoice_date DATE NOT NULL, total_amount NUMERIC(18, 0) NOT NULL DEFAULT 0, vat_amount NUMERIC(18, 0) NOT NULL DEFAULT 0, vat_rate NUMERIC(5, 4) NOT NULL DEFAULT 0, goods_amount NUMERIC(18, 0) NOT NULL DEFAULT 0, is_claimed BOOLEAN NOT NULL DEFAULT FALSE, claimed_date TIMESTAMP, created_at TIMESTAMP NOT NULL DEFAULT NOW(), created_by VARCHAR(100) NOT NULL DEFAULT 'system', modified_at TIMESTAMP, modified_by VARCHAR(100), is_deleted BOOLEAN NOT NULL DEFAULT FALSE)");

await ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS audit.audit_logs (audit_log_id BIGSERIAL PRIMARY KEY, event_time TIMESTAMP NOT NULL DEFAULT NOW(), user_name VARCHAR(100) NOT NULL, client_ip VARCHAR(45), table_name VARCHAR(100) NOT NULL, record_id VARCHAR(50) NOT NULL, action VARCHAR(10) NOT NULL, old_values JSONB, new_values JSONB, module VARCHAR(50))");

await ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS cfg.app_settings (setting_id UUID PRIMARY KEY, setting_key VARCHAR(100) NOT NULL UNIQUE, setting_value TEXT, category VARCHAR(50), created_at TIMESTAMP NOT NULL DEFAULT NOW(), modified_at TIMESTAMP)");
await ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS cfg.number_sequences (sequence_id UUID PRIMARY KEY, sequence_type VARCHAR(50) NOT NULL, fiscal_period_id UUID, current_value INT NOT NULL DEFAULT 0, prefix VARCHAR(20), UNIQUE(sequence_type, fiscal_period_id))");
await ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS cfg.outbox_messages (outbox_id UUID PRIMARY KEY, message_type VARCHAR(200) NOT NULL, payload JSONB, processed_at TIMESTAMP, retry_count INT NOT NULL DEFAULT 0, created_at TIMESTAMP NOT NULL DEFAULT NOW())");

await ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS acc.users (user_id UUID PRIMARY KEY, username VARCHAR(100) NOT NULL UNIQUE, display_name VARCHAR(255), email VARCHAR(100), department VARCHAR(100), employee_id UUID, is_active BOOLEAN NOT NULL DEFAULT TRUE, last_login_date TIMESTAMP, created_at TIMESTAMP NOT NULL DEFAULT NOW(), created_by VARCHAR(100) NOT NULL DEFAULT 'system', modified_at TIMESTAMP, modified_by VARCHAR(100))");
await ExecuteSql(conn, "CREATE TABLE IF NOT EXISTS acc.roles (role_id UUID PRIMARY KEY, role_name VARCHAR(100) NOT NULL UNIQUE, description VARCHAR(500), is_active BOOLEAN NOT NULL DEFAULT TRUE, sort_order INT, created_at TIMESTAMP NOT NULL DEFAULT NOW(), created_by VARCHAR(100) NOT NULL DEFAULT 'system')");

Console.WriteLine("\nCreating indexes...");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_vouchers_voucher_no ON acc.vouchers(voucher_no)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_vouchers_voucher_date ON acc.vouchers(voucher_date)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_vouchers_status ON acc.vouchers(status)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_vouchers_fiscal_period ON acc.vouchers(fiscal_period_id)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_voucher_lines_voucher_id ON acc.voucher_lines(voucher_id)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_voucher_lines_account_id ON acc.voucher_lines(account_id)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_customers_tax_code ON acc.customers(tax_code)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_vendors_tax_code ON acc.vendors(tax_code)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_employees_employee_code ON acc.employees(employee_code)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_employees_ad_username ON acc.employees(ad_username)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_banks_code ON acc.banks(bank_code)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_bank_accounts_number ON acc.bank_accounts(account_number)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_bank_transactions_date ON acc.bank_transactions(transaction_date)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_products_code ON acc.products(product_code)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_warehouses_code ON acc.warehouses(warehouse_code)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_fixed_assets_code ON acc.fixed_assets(asset_code)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_ledger_fiscal_period ON acc.ledger_entries(fiscal_period_id)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_ledger_account ON acc.ledger_entries(account_id)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_audit_logs_event_time ON audit.audit_logs(event_time)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_audit_logs_user_name ON audit.audit_logs(user_name)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_audit_logs_table_name ON audit.audit_logs(table_name)");
await ExecuteSql(conn, "CREATE INDEX IF NOT EXISTS idx_outbox_messages_processed ON cfg.outbox_messages(processed_at)");

Console.WriteLine("\nInserting initial data...");
await ExecuteSql(conn, "INSERT INTO acc.fiscal_periods (fiscal_period_id, year, month, status) SELECT gen_random_uuid(), EXTRACT(YEAR FROM CURRENT_DATE)::INT, EXTRACT(MONTH FROM CURRENT_DATE)::INT, 'Open' WHERE NOT EXISTS (SELECT 1 FROM acc.fiscal_periods WHERE year = EXTRACT(YEAR FROM CURRENT_DATE)::INT AND month = EXTRACT(MONTH FROM CURRENT_DATE)::INT)");
await ExecuteSql(conn, "INSERT INTO acc.roles (role_id, role_name, description, is_active, sort_order) SELECT gen_random_uuid(), 'Admin', 'System Administrator', true, 1 WHERE NOT EXISTS (SELECT 1 FROM acc.roles WHERE role_name = 'Admin')");
await ExecuteSql(conn, "INSERT INTO acc.roles (role_id, role_name, description, is_active, sort_order) SELECT gen_random_uuid(), 'User', 'Regular User', true, 2 WHERE NOT EXISTS (SELECT 1 FROM acc.roles WHERE role_name = 'User')");
await ExecuteSql(conn, "INSERT INTO cfg.app_settings (setting_id, setting_key, setting_value, category) SELECT gen_random_uuid(), 'CompanyName', 'AMS Company', 'General' WHERE NOT EXISTS (SELECT 1 FROM cfg.app_settings WHERE setting_key = 'CompanyName')");

Console.WriteLine("\nInserting tax rates...");
await ExecuteSql(conn, "INSERT INTO tax.tax_rates (tax_rate_id, tax_rate_key, tax_type, rate, effective_from, effective_to, created_at, created_by) VALUES (gen_random_uuid(), 'GTGT_STANDARD_REDUCED', 'GTGT', 0.08, '2025-07-01', '2026-12-31', NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO tax.tax_rates (tax_rate_id, tax_rate_key, tax_type, rate, effective_from, effective_to, created_at, created_by) VALUES (gen_random_uuid(), 'GTGT_STANDARD', 'GTGT', 0.10, '2027-01-01', NULL, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO tax.tax_rates (tax_rate_id, tax_rate_key, tax_type, rate, effective_from, effective_to, created_at, created_by) VALUES (gen_random_uuid(), 'GTGT_EXEMPT', 'GTGT', 0.00, '2026-01-01', NULL, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO tax.tax_rates (tax_rate_id, tax_rate_key, tax_type, rate, effective_from, effective_to, created_at, created_by) VALUES (gen_random_uuid(), 'CIT_STANDARD', 'CIT', 0.20, '2025-10-01', NULL, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO tax.tax_rates (tax_rate_id, tax_rate_key, tax_type, rate, effective_from, effective_to, created_at, created_by) VALUES (gen_random_uuid(), 'CIT_SME', 'CIT', 0.15, '2025-10-01', NULL, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO tax.tax_rates (tax_rate_id, tax_rate_key, tax_type, rate, effective_from, effective_to, created_at, created_by) VALUES (gen_random_uuid(), 'EXCISE_BIA', 'EXCISE', 0.65, '2026-01-01', NULL, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO tax.tax_rates (tax_rate_id, tax_rate_key, tax_type, rate, effective_from, effective_to, created_at, created_by) VALUES (gen_random_uuid(), 'EXCISE_NUOC_CO_DUONG', 'EXCISE', 0.10, '2026-01-01', NULL, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO tax.tax_rates (tax_rate_id, tax_rate_key, tax_type, rate, effective_from, effective_to, created_at, created_by) VALUES (gen_random_uuid(), 'EXCISE_RUOU_20', 'EXCISE', 0.65, '2026-01-01', NULL, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO tax.tax_rates (tax_rate_id, tax_rate_key, tax_type, rate, effective_from, effective_to, created_at, created_by) VALUES (gen_random_uuid(), 'EXCISE_RUOU_DUOI_20', 'EXCISE', 0.35, '2026-01-01', NULL, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO tax.tax_rates (tax_rate_id, tax_rate_key, tax_type, rate, effective_from, effective_to, created_at, created_by) VALUES (gen_random_uuid(), 'EXCISE_THUOC_LA', 'EXCISE', 0.75, '2026-01-01', NULL, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO tax.tax_rates (tax_rate_id, tax_rate_key, tax_type, rate, effective_from, effective_to, created_at, created_by) VALUES (gen_random_uuid(), 'LATE_PENALTY', 'PENALTY', 0.0003, '2026-07-01', NULL, NOW(), 'system') ON CONFLICT DO NOTHING");

Console.WriteLine("\nInserting PIT brackets (5 bậc từ 01/01/2026)...");
await ExecuteSql(conn, "INSERT INTO tax.pit_brackets (pit_bracket_id, bracket_no, from_amount, to_amount, tax_rate, quick_deduction, effective_from) VALUES (gen_random_uuid(), 1, 0, 10000000, 0.05, 0, '2026-01-01') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO tax.pit_brackets (pit_bracket_id, bracket_no, from_amount, to_amount, tax_rate, quick_deduction, effective_from) VALUES (gen_random_uuid(), 2, 10000001, 30000000, 0.10, 250000, '2026-01-01') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO tax.pit_brackets (pit_bracket_id, bracket_no, from_amount, to_amount, tax_rate, quick_deduction, effective_from) VALUES (gen_random_uuid(), 3, 30000001, 80000000, 0.20, 750000, '2026-01-01') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO tax.pit_brackets (pit_bracket_id, bracket_no, from_amount, to_amount, tax_rate, quick_deduction, effective_from) VALUES (gen_random_uuid(), 4, 80000001, 200000000, 0.30, 1650000, '2026-01-01') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO tax.pit_brackets (pit_bracket_id, bracket_no, from_amount, to_amount, tax_rate, quick_deduction, effective_from) VALUES (gen_random_uuid(), 5, 200000001, NULL, 0.35, 3250000, '2026-01-01') ON CONFLICT DO NOTHING");

Console.WriteLine("\nInserting PIT allowances...");
await ExecuteSql(conn, "INSERT INTO tax.pit_allowances (allowance_id, allowance_code, allowance_name, amount, is_active) VALUES (gen_random_uuid(), 'PIT_SELF_ALLOWANCE', 'Giảm trừ bản thân', 15500000, true) ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO tax.pit_allowances (allowance_id, allowance_code, allowance_name, amount, is_active) VALUES (gen_random_uuid(), 'PIT_DEPENDENT', 'Giảm trừ người phụ thuộc', 6200000, true) ON CONFLICT DO NOTHING");

Console.WriteLine("\nInserting sample chart of accounts (TT 99/2025)...");
await ExecuteSql(conn, "INSERT INTO acc.chart_of_accounts (account_id, account_code, account_name, account_number, account_level, account_type, is_detail_account, is_active, allow_entry, is_bank_account, is_deleted, created_at, created_by) VALUES (gen_random_uuid(), '111', 'Tiền mặt', 111, 1, 'Asset', false, true, true, false, false, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO acc.chart_of_accounts (account_id, account_code, account_name, account_number, account_level, account_type, is_detail_account, is_active, allow_entry, is_bank_account, is_deleted, created_at, created_by) VALUES (gen_random_uuid(), '112', 'Tiền gửi ngân hàng', 112, 1, 'Asset', false, true, true, true, false, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO acc.chart_of_accounts (account_id, account_code, account_name, account_number, account_level, account_type, is_detail_account, is_active, allow_entry, is_bank_account, is_deleted, created_at, created_by) VALUES (gen_random_uuid(), '131', 'Phải thu của khách hàng', 131, 1, 'Asset', false, true, true, false, false, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO acc.chart_of_accounts (account_id, account_code, account_name, account_number, account_level, account_type, is_detail_account, is_active, allow_entry, is_bank_account, is_deleted, created_at, created_by) VALUES (gen_random_uuid(), '141', 'Phải thu khác', 141, 1, 'Asset', false, true, true, false, false, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO acc.chart_of_accounts (account_id, account_code, account_name, account_number, account_level, account_type, is_detail_account, is_active, allow_entry, is_bank_account, is_deleted, created_at, created_by) VALUES (gen_random_uuid(), '152', 'Hàng hóa', 152, 1, 'Asset', false, true, true, false, false, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO acc.chart_of_accounts (account_id, account_code, account_name, account_number, account_level, account_type, is_detail_account, is_active, allow_entry, is_bank_account, is_deleted, created_at, created_by) VALUES (gen_random_uuid(), '211', 'TSCĐ', 211, 1, 'Asset', false, true, false, false, false, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO acc.chart_of_accounts (account_id, account_code, account_name, account_number, account_level, account_type, is_detail_account, is_active, allow_entry, is_bank_account, is_deleted, created_at, created_by) VALUES (gen_random_uuid(), '331', 'Phải trả cho người bán', 331, 1, 'Liability', false, true, true, false, false, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO acc.chart_of_accounts (account_id, account_code, account_name, account_number, account_level, account_type, is_detail_account, is_active, allow_entry, is_bank_account, is_deleted, created_at, created_by) VALUES (gen_random_uuid(), '333', 'Thuế và các khoản phải nộp', 333, 1, 'Liability', false, true, false, false, false, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO acc.chart_of_accounts (account_id, account_code, account_name, account_number, account_level, account_type, is_detail_account, is_active, allow_entry, is_bank_account, is_deleted, created_at, created_by) VALUES (gen_random_uuid(), '411', 'Vốn chủ sở hữu', 411, 1, 'Equity', false, true, false, false, false, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO acc.chart_of_accounts (account_id, account_code, account_name, account_number, account_level, account_type, is_detail_account, is_active, allow_entry, is_bank_account, is_deleted, created_at, created_by) VALUES (gen_random_uuid(), '511', 'Doanh thu bán hàng', 511, 1, 'Revenue', false, true, true, false, false, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO acc.chart_of_accounts (account_id, account_code, account_name, account_number, account_level, account_type, is_detail_account, is_active, allow_entry, is_bank_account, is_deleted, created_at, created_by) VALUES (gen_random_uuid(), '632', 'Giá vốn hàng bán', 632, 1, 'Expense', false, true, true, false, false, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO acc.chart_of_accounts (account_id, account_code, account_name, account_number, account_level, account_type, is_detail_account, is_active, allow_entry, is_bank_account, is_deleted, created_at, created_by) VALUES (gen_random_uuid(), '641', 'Chi phí bán hàng', 641, 1, 'Expense', false, true, true, false, false, NOW(), 'system') ON CONFLICT DO NOTHING");
await ExecuteSql(conn, "INSERT INTO acc.chart_of_accounts (account_id, account_code, account_name, account_number, account_level, account_type, is_detail_account, is_active, allow_entry, is_bank_account, is_deleted, created_at, created_by) VALUES (gen_random_uuid(), '642', 'Chi phí quản lý', 642, 1, 'Expense', false, true, true, false, false, NOW(), 'system') ON CONFLICT DO NOTHING");

Console.WriteLine("\n========================================");
Console.WriteLine("Step 6: Database setup complete!");
Console.WriteLine("========================================");
Console.WriteLine($"\nDatabase: {dbName}");
Console.WriteLine($"Host: {host}:{port}");
Console.WriteLine($"Tables created: ~45");
Console.WriteLine("\nYou can now run the AMS application.\n");

async Task ExecuteSql(NpgsqlConnection connection, string sql)
{
    try
    {
        await using var cmd = new NpgsqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync();
        
        var lines = sql.Split('\n');
        var firstLine = lines[0].Trim();
        if (firstLine.StartsWith("CREATE SCHEMA"))
            Console.WriteLine($"  ✓ Schema: {firstLine.Replace("CREATE SCHEMA ", "")}");
        else if (firstLine.StartsWith("CREATE TABLE"))
            Console.WriteLine($"  ✓ Table: {firstLine.Replace("CREATE TABLE IF NOT EXISTS ", "").Replace("acc.", "").Replace("tax.", "").Replace("audit.", "").Replace("cfg.", "").Replace("rpt.", "")}");
        else if (firstLine.StartsWith("CREATE INDEX"))
            Console.WriteLine($"  ✓ Index created");
        else if (firstLine.StartsWith("INSERT"))
            Console.WriteLine($"  ✓ Initial data");
        else
            Console.WriteLine($"  ✓ Executed");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ✗ Error: {ex.Message}");
    }
}