from datetime import datetime, date
from app.extensions import db
from sqlalchemy import text


def get_current_ky_ke_toan() -> dict:
    result = db.session.execute(
        text("""
            SELECT id, nam, thang, tu_ngay, den_ngay, trang_thai 
            FROM accounting.ky_ke_toan 
            WHERE :current_date BETWEEN tu_ngay AND den_ngay
        """),
        {"current_date": date.today()}
    ).fetchone()

    if result:
        return {
            'id': result[0],
            'nam': result[1],
            'thang': result[2],
            'tu_ngay': result[3],
            'den_ngay': result[4],
            'trang_thai': result[5]
        }
    return None


def is_ky_khoa(ky_ke_toan_id: int) -> bool:
    result = db.session.execute(
        text("SELECT trang_thai FROM accounting.ky_ke_toan WHERE id = :id"),
        {"id": ky_ke_toan_id}
    ).fetchone()

    return result and result[0] == 'khoa'


def check_ky_khoa(ngay_ct: date) -> None:
    from app import AppException

    result = db.session.execute(
        text("""
            SELECT trang_thai 
            FROM accounting.ky_ke_toan 
            WHERE :ngay_ct BETWEEN tu_ngay AND den_ngay
        """),
        {"ngay_ct": ngay_ct}
    ).fetchone()

    if result and result[0] == 'khoa':
        raise AppException("Kỳ kế toán đã khóa, không thể thao tác", 400)


def create_ky_ke_toan(nam: int, thang: int) -> dict:
    tu_ngay = date(nam, thang, 1)
    if thang == 12:
        den_ngay = date(nam + 1, 1, 1)
    else:
        den_ngay = date(nam, thang + 1, 1)

    result = db.session.execute(
        text("""
            INSERT INTO accounting.ky_ke_toan (nam, thang, tu_ngay, den_ngay, trang_thai)
            VALUES (:nam, :thang, :tu_ngay, :den_ngay, 'mo')
            RETURNING id
        """),
        {"nam": nam, "thang": thang, "tu_ngay": tu_ngay, "den_ngay": den_ngay}
    ).fetchone()

    db.session.commit()
    return {'id': result[0], 'nam': nam, 'thang': thang}


def lock_ky_ke_toan(ky_ke_toan_id: int) -> None:
    db.session.execute(
        text("UPDATE accounting.ky_ke_toan SET trang_thai = 'khoa' WHERE id = :id"),
        {"id": ky_ke_toan_id}
    )
    db.session.commit()


def unlock_ky_ke_toan(ky_ke_toan_id: int) -> None:
    db.session.execute(
        text("UPDATE accounting.ky_ke_toan SET trang_thai = 'mo' WHERE id = :id"),
        {"id": ky_ke_toan_id}
    )
    db.session.commit()
