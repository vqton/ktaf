"""Seed data for TT99/2025 Chart of Accounts.

Contains ~140 accounts following Thông tư 99/2025/TT-BTC chart of accounts.

Usage:
    flask seed-tt99
    python -m app.utils.seed_tt99
"""

TT99_ACCOUNTS = [
    # CLASS 1: TÀI SẢN (Assets)
    {'ma_tk': '11', 'ten_tk': 'Tiền', 'loai_tk': 'tai_san', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '111', 'ten_tk': 'Tiền mặt', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '11', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '1111', 'ten_tk': 'Tiền Việt Nam', 'loai_tk': 'tai_san', 'cap_tk': 3, 'ma_tk_cha': '111', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '1112', 'ten_tk': 'Ngoại tệ', 'loai_tk': 'tai_san', 'cap_tk': 3, 'ma_tk_cha': '111', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '1113', 'ten_tk': 'Vàng tiền tệ', 'loai_tk': 'tai_san', 'cap_tk': 3, 'ma_tk_cha': '111', 'tinh_chat': 'du', 'co_the_dk': True},
    
    {'ma_tk': '112', 'ten_tk': 'Tiền gửi ngân hàng', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '11', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '1121', 'ten_tk': 'Tiền Việt Nam', 'loai_tk': 'tai_san', 'cap_tk': 3, 'ma_tk_cha': '112', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '1122', 'ten_tk': 'Ngoại tệ', 'loai_tk': 'tai_san', 'cap_tk': 3, 'ma_tk_cha': '112', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '1123', 'ten_tk': 'Vàng tiền tệ', 'loai_tk': 'tai_san', 'cap_tk': 3, 'ma_tk_cha': '112', 'tinh_chat': 'du', 'co_the_dk': True},

    {'ma_tk': '12', 'ten_tk': 'Đầu tư tài chính', 'loai_tk': 'tai_san', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '121', 'ten_tk': 'Chứng khoán kinh doanh', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '12', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '122', 'ten_tk': 'Đầu tư nắm giữ đến ngày đáo hạn', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '12', 'tinh_chat': 'du', 'co_the_dk': True},

    {'ma_tk': '13', 'ten_tk': 'Phải thu của khách hàng', 'loai_tk': 'tai_san', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '131', 'ten_tk': 'Phải thu của khách hàng', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '13', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '133', 'ten_tk': 'Thuế GTGT được khấu trừ', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '13', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '1331', 'ten_tk': 'Thuế GTGT được khấu trừ', 'loai_tk': 'tai_san', 'cap_tk': 3, 'ma_tk_cha': '133', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '1332', 'ten_tk': 'Thuế GTGT được khấu trừ hàng nhập khẩu', 'loai_tk': 'tai_san', 'cap_tk': 3, 'ma_tk_cha': '133', 'tinh_chat': 'du', 'co_the_dk': True},

    {'ma_tk': '15', 'ten_tk': 'Hàng tồn kho', 'loai_tk': 'tai_san', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '151', 'ten_tk': 'Mua hàng', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '15', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '152', 'ten_tk': 'Nguyên vật liệu', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '15', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '153', 'ten_tk': 'Công cụ, dụng cụ', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '15', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '154', 'ten_tk': 'Chi phí sản xuất, kinh doanh dở dang', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '15', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '155', 'ten_tk': 'Thành phẩm', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '15', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '156', 'ten_tk': 'Hàng hóa', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '15', 'tinh_chat': 'du', 'co_the_dk': True},

    {'ma_tk': '16', 'ten_tk': 'Tài sản cố định', 'loai_tk': 'tai_san', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '211', 'ten_tk': 'Tài sản cố định hữu hình', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '16', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '212', 'ten_tk': 'Tài sản cố định thuê tài chính', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '16', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '213', 'ten_tk': 'Tài sản cố định vô hình', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '16', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '214', 'ten_tk': 'Hao mòn tài sản cố định', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '16', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '17', 'ten_tk': 'Xây dựng cơ bản dở dang', 'loai_tk': 'tai_san', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '241', 'ten_tk': 'Xây dựng cơ bản dở dang', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '17', 'tinh_chat': 'du', 'co_the_dk': True},

    {'ma_tk': '18', 'ten_tk': 'Phải thu khác', 'loai_tk': 'tai_san', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '138', 'ten_tk': 'Phải thu khác', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '18', 'tinh_chat': 'du', 'co_the_dk': True},
    {'ma_tk': '1388', 'ten_tk': 'Phải thu khác', 'loai_tk': 'tai_san', 'cap_tk': 3, 'ma_tk_cha': '138', 'tinh_chat': 'du', 'co_the_dk': True},

    {'ma_tk': '19', 'ten_tk': 'Tài sản thuế thu nhập hoãn lại', 'loai_tk': 'tai_san', 'cap_tk': 1, 'tinh_chat': 'du', 'co_the_dk': False},
    {'ma_tk': '191', 'ten_tk': 'Tài sản thuế thu nhập hoãn lại', 'loai_tk': 'tai_san', 'cap_tk': 2, 'ma_tk_cha': '19', 'tinh_chat': 'du', 'co_the_dk': True},

    # CLASS 2: NGUỒN VỐN (Sources)
    {'ma_tk': '21', 'ten_tk': 'Phải trả người bán', 'loai_tk': 'nguon_von', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '211', 'ten_tk': 'Phải trả người bán', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '21', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '212', 'ten_tk': 'Phải trả người bán là bên liên quan', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '21', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '22', 'ten_tk': 'Thuế và các khoản phải nộp nhà nước', 'loai_tk': 'nguon_von', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '222', 'ten_tk': 'Thuế thu nhập doanh nghiệp phải nộp', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '22', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '223', 'ten_tk': 'Thuế thu nhập cá nhân phải nộp', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '22', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '224', 'ten_tk': 'Thuế GTGT phải nộp', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '22', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '2241', 'ten_tk': 'Thuế GTGT hàng bán ra', 'loai_tk': 'nguon_von', 'cap_tk': 3, 'ma_tk_cha': '224', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '2242', 'ten_tk': 'Thuế GTGT hàng nhập khẩu', 'loai_tk': 'nguon_von', 'cap_tk': 3, 'ma_tk_cha': '224', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '225', 'ten_tk': 'Thuế tiêu thụ đặc biệt', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '22', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '226', 'ten_tk': 'Thuế xuất, nhập khẩu', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '22', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '228', 'ten_tk': 'Thuế và các khoản phải nộp khác', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '22', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '23', 'ten_tk': 'Phải trả người lao động', 'loai_tk': 'nguon_von', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '334', 'ten_tk': 'Phải trả người lao động', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '23', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '24', 'ten_tk': 'Chi phí phải trả', 'loai_tk': 'nguon_von', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '335', 'ten_tk': 'Chi phí phải trả', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '24', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '25', 'ten_tk': 'Phải trả nội bộ', 'loai_tk': 'nguon_von', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '336', 'ten_tk': 'Phải trả nội bộ', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '25', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '26', 'ten_tk': 'Phải trả khác', 'loai_tk': 'nguon_von', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '338', 'ten_tk': 'Phải trả khác', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '26', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '3382', 'ten_tk': 'Kinh phí công đoàn', 'loai_tk': 'nguon_von', 'cap_tk': 3, 'ma_tk_cha': '338', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '3383', 'ten_tk': 'Bảo hiểm xã hội', 'loai_tk': 'nguon_von', 'cap_tk': 3, 'ma_tk_cha': '338', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '3384', 'ten_tk': 'Bảo hiểm y tế', 'loai_tk': 'nguon_von', 'cap_tk': 3, 'ma_tk_cha': '338', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '3385', 'ten_tk': 'Bảo hiểm thất nghiệp', 'loai_tk': 'nguon_von', 'cap_tk': 3, 'ma_tk_cha': '338', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '3386', 'ten_tk': 'Nhận ký quỹ, ký cược', 'loai_tk': 'nguon_von', 'cap_tk': 3, 'ma_tk_cha': '338', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '3387', 'ten_tk': 'Doanh thu chưa thực hiện', 'loai_tk': 'nguon_von', 'cap_tk': 3, 'ma_tk_cha': '338', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '3388', 'ten_tk': 'Phải trả khác', 'loai_tk': 'nguon_von', 'cap_tk': 3, 'ma_tk_cha': '338', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '27', 'ten_tk': 'Vay và nợ thuê tài chính', 'loai_tk': 'nguon_von', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '341', 'ten_tk': 'Vay ngắn hạn', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '27', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '342', 'ten_tk': 'Vay dài hạn', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '27', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '343', 'ten_tk': 'Nợ thuê tài chính', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '27', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '28', 'ten_tk': 'Thuế thu nhập hoãn lại phải trả', 'loai_tk': 'nguon_von', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '347', 'ten_tk': 'Thuế thu nhập hoãn lại phải trả', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '28', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '29', 'ten_tk': 'Nguồn vốn kinh doanh và các quỹ', 'loai_tk': 'nguon_von', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '411', 'ten_tk': 'Nguồn vốn kinh doanh', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '29', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '4111', 'ten_tk': 'Nguồn vốn kinh doanh', 'loai_tk': 'nguon_von', 'cap_tk': 3, 'ma_tk_cha': '411', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '4112', 'ten_tk': 'Chênh lệch đánh giá tài sản', 'loai_tk': 'nguon_von', 'cap_tk': 3, 'ma_tk_cha': '411', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '4113', 'ten_tk': 'Chênh lệch tỷ giá hối đoái', 'loai_tk': 'nguon_von', 'cap_tk': 3, 'ma_tk_cha': '411', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '412', 'ten_tk': 'Lợi nhuận sau thuế chưa phân phối', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '29', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '417', 'ten_tk': 'Quỹ phát triển khoa học và công nghệ', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '29', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '418', 'ten_tk': 'Quỹ hỗ trợ sắp xếp doanh nghiệp', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '29', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '419', 'ten_tk': 'Các quỹ khác thuộc vốn chủ sở hữu', 'loai_tk': 'nguon_von', 'cap_tk': 2, 'ma_tk_cha': '29', 'tinh_chat': 'co', 'co_the_dk': True},

    # CLASS 3: DOANH THU (Revenue)
    {'ma_tk': '51', 'ten_tk': 'Doanh thu hoạt động sản xuất, kinh doanh', 'loai_tk': 'doanh_thu', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '511', 'ten_tk': 'Doanh thu bán hàng và cung cấp dịch vụ', 'loai_tk': 'doanh_thu', 'cap_tk': 2, 'ma_tk_cha': '51', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '5111', 'ten_tk': 'Doanh thu bán hàng', 'loai_tk': 'doanh_thu', 'cap_tk': 3, 'ma_tk_cha': '511', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '5112', 'ten_tk': 'Doanh thu cung cấp dịch vụ', 'loai_tk': 'doanh_thu', 'cap_tk': 3, 'ma_tk_cha': '511', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '5113', 'ten_tk': 'Doanh thu trợ cấp, trợ giá', 'loai_tk': 'doanh_thu', 'cap_tk': 3, 'ma_tk_cha': '511', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '5114', 'ten_tk': 'Doanh thu khác', 'loai_tk': 'doanh_thu', 'cap_tk': 3, 'ma_tk_cha': '511', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '512', 'ten_tk': 'Doanh thu nội bộ', 'loai_tk': 'doanh_thu', 'cap_tk': 2, 'ma_tk_cha': '51', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '52', 'ten_tk': 'Doanh thu hoạt động tài chính', 'loai_tk': 'doanh_thu', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '521', 'ten_tk': 'Lãi tiền vay', 'loai_tk': 'doanh_thu', 'cap_tk': 2, 'ma_tk_cha': '52', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '522', 'ten_tk': 'Lãi chứng khoán kinh doanh', 'loai_tk': 'doanh_thu', 'cap_tk': 2, 'ma_tk_cha': '52', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '523', 'ten_tk': 'Lãi từ bán ngoại tệ', 'loai_tk': 'doanh_thu', 'cap_tk': 2, 'ma_tk_cha': '52', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '524', 'ten_tk': 'Lãi đầu tư tài chính', 'loai_tk': 'doanh_thu', 'cap_tk': 2, 'ma_tk_cha': '52', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '525', 'ten_tk': 'Lãi từ thanh lý tài sản tài chính', 'loai_tk': 'doanh_thu', 'cap_tk': 2, 'ma_tk_cha': '52', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '526', 'ten_tk': 'Doanh thu hoạt động tài chính khác', 'loai_tk': 'doanh_thu', 'cap_tk': 2, 'ma_tk_cha': '52', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '53', 'ten_tk': 'Thu nhập khác', 'loai_tk': 'doanh_thu', 'cap_tk': 1, 'tinh_chat': 'co', 'co_the_dk': False},
    {'ma_tk': '531', 'ten_tk': 'Thu nhập từ thanh lý, nhượng bán TSCĐ', 'loai_tk': 'doanh_thu', 'cap_tk': 2, 'ma_tk_cha': '53', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '532', 'ten_tk': 'Thu nhập từ phạt, bồi thường', 'loai_tk': 'doanh_thu', 'cap_tk': 2, 'ma_tk_cha': '53', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '533', 'ten_tk': 'Thu nhập từ nợ khó đòi đã xử lý', 'loai_tk': 'doanh_thu', 'cap_tk': 2, 'ma_tk_cha': '53', 'tinh_chat': 'co', 'co_the_dk': True},
    {'ma_tk': '538', 'ten_tk': 'Thu nhập khác', 'loai_tk': 'doanh_thu', 'cap_tk': 2, 'ma_tk_cha': '53', 'tinh_chat': 'co', 'co_the_dk': True},

    {'ma_tk': '54', 'ten_tk': 'Giá vốn hàng bán', 'loai_tk': 'chi_phi', 'cap_tk': 1, 'tinh_chat': 'no', 'co_the_dk': False},
    {'ma_tk': '541', 'ten_tk': 'Giá vốn hàng bán', 'loai_tk': 'chi_phi', 'cap_tk': 2, 'ma_tk_cha': '54', 'tinh_chat': 'no', 'co_the_dk': True},

    {'ma_tk': '55', 'ten_tk': 'Chi phí hoạt động tài chính', 'loai_tk': 'chi_phi', 'cap_tk': 1, 'tinh_chat': 'no', 'co_the_dk': False},
    {'ma_tk': '632', 'ten_tk': 'Giá vốn hàng bán', 'loai_tk': 'chi_phi', 'cap_tk': 2, 'ma_tk_cha': '55', 'tinh_chat': 'no', 'co_the_dk': True},
    {'ma_tk': '635', 'ten_tk': 'Chi phí tài chính', 'loai_tk': 'chi_phi', 'cap_tk': 2, 'ma_tk_cha': '55', 'tinh_chat': 'no', 'co_the_dk': True},

    {'ma_tk': '56', 'ten_tk': 'Chi phí bán hàng', 'loai_tk': 'chi_phi', 'cap_tk': 1, 'tinh_chat': 'no', 'co_the_dk': False},
    {'ma_tk': '641', 'ten_tk': 'Chi phí bán hàng', 'loai_tk': 'chi_phi', 'cap_tk': 2, 'ma_tk_cha': '56', 'tinh_chat': 'no', 'co_the_dk': True},

    {'ma_tk': '57', 'ten_tk': 'Chi phí quản lý doanh nghiệp', 'loai_tk': 'chi_phi', 'cap_tk': 1, 'tinh_chat': 'no', 'co_the_dk': False},
    {'ma_tk': '642', 'ten_tk': 'Chi phí quản lý doanh nghiệp', 'loai_tk': 'chi_phi', 'cap_tk': 2, 'ma_tk_cha': '57', 'tinh_chat': 'no', 'co_the_dk': True},

    {'ma_tk': '58', 'ten_tk': 'Chi phí khác', 'loai_tk': 'chi_phi', 'cap_tk': 1, 'tinh_chat': 'no', 'co_the_dk': False},
    {'ma_tk': '811', 'ten_tk': 'Chi phí khác', 'loai_tk': 'chi_phi', 'cap_tk': 2, 'ma_tk_cha': '58', 'tinh_chat': 'no', 'co_the_dk': True},

    {'ma_tk': '59', 'ten_tk': 'Thuế thu nhập doanh nghiệp', 'loai_tk': 'chi_phi', 'cap_tk': 1, 'tinh_chat': 'no', 'co_the_dk': False},
    {'ma_tk': '821', 'ten_tk': 'Thuế thu nhập doanh nghiệp', 'loai_tk': 'chi_phi', 'cap_tk': 2, 'ma_tk_cha': '59', 'tinh_chat': 'no', 'co_the_dk': True},

    # CLASS 9: KẾT QUẢ HOẠT ĐỘNG KINH DOANH (Operating Results)
    {'ma_tk': '91', 'ten_tk': 'Xác định kết quả kinh doanh', 'loai_tk': 'ngoai_bang', 'cap_tk': 1, 'tinh_chat': 'luong_tinh', 'co_the_dk': False},
    {'ma_tk': '911', 'ten_tk': 'Xác định kết quả kinh doanh', 'loai_tk': 'ngoai_bang', 'cap_tk': 2, 'ma_tk_cha': '91', 'tinh_chat': 'luong_tinh', 'co_the_dk': True},
]


def seed_tt99_accounts():
    """Seed TT99 chart of accounts into database.

    Only inserts accounts that don't already exist (idempotent).

    Returns:
        int: Number of accounts seeded
    """
    from app import create_app
    from app.extensions import db
    from app.modules.he_thong_tk.models import HeThongTaiKhoan

    app = create_app()
    with app.app_context():
        count = 0
        for acc_data in TT99_ACCOUNTS:
            existing = HeThongTaiKhoan.query.get(acc_data['ma_tk'])
            if not existing:
                tk = HeThongTaiKhoan(**acc_data)
                db.session.add(tk)
                count += 1
        
        db.session.commit()
        print(f"Seeded {count} TT99 accounts")


if __name__ == '__main__':
    seed_tt99_accounts()
