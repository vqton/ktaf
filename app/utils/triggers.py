"""PostgreSQL triggers for automatic SoCai (general ledger) updates.

Provides SQL trigger definitions that automatically update the SoCai table
when DinhKhoan (journal entries) are inserted, updated, or deleted.

Usage:
    Run SQL directly in PostgreSQL or use create_triggers() function.
"""

TRIGGER_FUNCTION = """
CREATE OR REPLACE FUNCTION accounting.update_so_cai()
RETURNS TRIGGER AS $$
DECLARE
    v_ky_ke_toan_id INTEGER;
    v_ma_tk VARCHAR(10);
BEGIN
    -- Get accounting period from chung_tu
    SELECT ct.ngay_hach_toan INTO v_ky_ke_toan_id
    FROM accounting.chung_tu ct
    WHERE ct.id = NEW.chung_tu_id;

    -- Skip if chung_tu not found or not approved
    IF NOT FOUND THEN
        RETURN NEW;
    END IF;

    -- Find or create SoCai record
    SELECT id INTO v_ky_ke_toan_id
    FROM accounting.ky_ke_toan
    WHERE NEW.chung_tu_id IN (SELECT id FROM accounting.chung_tu WHERE ngay_hach_toan BETWEEN tu_ngay AND den_ngay)
    LIMIT 1;

    IF NOT FOUND OR v_ky_ke_toan_id IS NULL THEN
        RETURN NEW;
    END IF;

    -- Update TK NO
    IF NEW.tk_no IS NOT NULL THEN
        INSERT INTO accounting.so_cai (ma_tk, ky_ke_toan_id, phat_sinh_no)
        VALUES (NEW.tk_no, v_ky_ke_toan_id, NEW.so_tien)
        ON CONFLICT (ma_tk, ky_ke_toan_id)
        DO UPDATE SET phat_sinh_no = so_cai.phat_sinh_no + NEW.so_tien;
    END IF;

    -- Update TK CO
    IF NEW.tk_co IS NOT NULL THEN
        INSERT INTO accounting.so_cai (ma_tk, ky_ke_toan_id, phat_sinh_co)
        VALUES (NEW.tk_co, v_ky_ke_toan_id, NEW.so_tien)
        ON CONFLICT (ma_tk, ky_ke_toan_id)
        DO UPDATE SET phat_sinh_co = so_cai.phat_sinh_co + NEW.so_tien;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
"""

TRIGGER_DINHKHOAN_INSERT = """
CREATE TRIGGER trig_dinhkhoan_insert
AFTER INSERT ON accounting.dinh_khoan
FOR EACH ROW
EXECUTE FUNCTION accounting.update_so_cai();
"""

TRIGGER_DINHKHOAN_UPDATE = """
CREATE TRIGGER trig_dinhkhoan_update
AFTER UPDATE ON accounting.dinh_khoan
FOR EACH ROW
EXECUTE FUNCTION accounting.update_so_cai();
"""

TRIGGER_DINHKHOAN_DELETE = """
CREATE TRIGGER trig_dinhkhoan_delete
AFTER DELETE ON accounting.dinh_khoan
FOR EACH ROW
EXECUTE FUNCTION accounting.update_so_cai();
"""


def create_triggers(connection):
    """Create all SoCai update triggers in database.

    Args:
        connection: SQLAlchemy connection object

    Creates:
        - update_so_cai() function
        - trig_dinhkhoan_insert trigger
        - trig_dinhkhoan_update trigger
        - trig_dinhkhoan_delete trigger
    """
    from sqlalchemy import text
    
    connection.execute(text(TRIGGER_FUNCTION))
    connection.execute(text(TRIGGER_DINHKHOAN_INSERT))
    connection.execute(text(TRIGGER_DINHKHOAN_UPDATE))
    connection.execute(text(TRIGGER_DINHKHOAN_DELETE))
    connection.commit()
