"""
File: app/utils/response.py
Module: Utilities — Chuẩn hoá API Response
Mô tả: Helper functions cho việc format response chuẩn REST API.
       Tất cả API routes phải dùng các hàm trong file này.

Response format chuẩn toàn hệ thống:
    {
        "success": true|false,
        "data": {},
        "message": "",
        "pagination": {"page": 1, "per_page": 20, "total": 100} | null
    }

Tham chiếu pháp lý:
    - TT99/2025/TT-BTC: Không quy định cụ thể về API format

Tác giả: [Tên]
Ngày tạo: 2025-01-xx
Cập nhật: [Ngày] — [Mô tả thay đổi]
"""

from typing import Any, Optional
from flask import jsonify


def success_response(
    data: Any = None,
    message: str = "",
    pagination: Optional[dict] = None,
    status_code: int = 200
) -> tuple:
    """Trả về response thành công chuẩn hoá.

    Args:
        data: Dữ liệu trả về cho client (dict, list, hoặc giá trị đơn).
        message: Thông báo mô tả kết quả (tuỳ chọn).
        pagination: Thông tin phân trang nếu có (dict với keys: page, per_page, total).
        status_code: HTTP status code (mặc định: 200).

    Returns:
        Tuple (response_dict, status_code) để Flask xử lý.

    Example:
        >>> return success_response(data=user_schema.dump(user), message="Đăng nhập thành công")
        >>> return success_response(data=users, pagination={"page": 1, "per_page": 20, "total": 100})
    """
    response = {
        "success": True,
        "data": data,
        "message": message
    }

    if pagination is not None:
        response["pagination"] = pagination

    return jsonify(response), status_code


def error_response(
    message: str,
    code: str = "ERROR",
    status_code: int = 400,
    errors: Optional[dict] = None
) -> tuple:
    """Trả về response lỗi chuẩn hoá.

    Args:
        message: Thông báo lỗi mô tả cho người dùng.
        code: Mã lỗi theo convention của hệ thống (ví dụ: 'BUT_TOAN_KHONG_CAN_BANG').
        status_code: HTTP status code (mặc định: 400).
        errors: Chi tiết lỗi validation (tuỳ chọn).

    Returns:
        Tuple (response_dict, status_code) để Flask xử lý.

    Example:
        >>> return error_response("Kỳ kế toán đã khoá", "KY_KE_TOAN_DA_KHOA", 400)
        >>> return error_response("Validation thất bại", "VALIDATION_ERROR", 400, {"email": "Invalid"})
    """
    response = {
        "success": False,
        "message": message,
        "code": code
    }

    if errors is not None:
        response["errors"] = errors

    return jsonify(response), status_code


def created_response(
    data: Any = None,
    message: str = "Tạo thành công",
    location: Optional[str] = None
) -> tuple:
    """Trả về response cho POST tạo mới thành công (HTTP 201).

    Args:
        data: Dữ liệu của resource vừa tạo.
        message: Thông báo thành công.
        location: URL tới resource vừa tạo (tuỳ chọn, dùng header Location).

    Returns:
        Tuple (response_dict, status_code) với các headers bổ sung nếu có.
    """
    response = {
        "success": True,
        "data": data,
        "message": message
    }

    result = jsonify(response), 201
    if location:
        result[1].headers["Location"] = location

    return result


def deleted_response(
    message: str = "Xoá thành công"
) -> tuple:
    """Trả về response cho DELETE thành công (HTTP 204).

    Args:
        message: Thông báo thành công (thường không hiển thị cho user).

    Returns:
        Tuple (response_dict, status_code).
    """
    return jsonify({
        "success": True,
        "message": message
    }), 204


def paginated_response(
    items: list,
    page: int,
    per_page: int,
    total: int,
    schema: Any = None
) -> tuple:
    """Trả về response có phân trang.

    Args:
        items: Danh sách items cần phân trang.
        page: Trang hiện tại (1-indexed).
        per_page: Số items trên mỗi trang.
        total: Tổng số items.
        schema: Marshmallow schema để serialize (tuỳ chọn).

    Returns:
        Tuple (response_dict, status_code).
    """
    if schema:
        data = schema.dump(items, many=True)
    else:
        data = items

    return success_response(
        data=data,
        pagination={
            "page": page,
            "per_page": per_page,
            "total": total,
            "pages": (total + per_page - 1) // per_page
        }
    )
