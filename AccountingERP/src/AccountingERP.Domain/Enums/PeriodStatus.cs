namespace AccountingERP.Domain.Enums
{
    /// <summary>
    /// Trạng thái kỳ kế toán
    /// </summary>
    public enum PeriodStatus
    {
        /// <summary>
        /// Đang mở - cho phép ghi sổ
        /// </summary>
        Open = 1,

        /// <summary>
        /// Đang đóng - đang thực hiện các thao tác đóng kỳ
        /// </summary>
        Closing = 2,

        /// <summary>
        /// Đã đóng - không cho phép ghi sổ
        /// </summary>
        Closed = 3,

        /// <summary>
        /// Đã khóa vĩnh viễn - sau quyết toán thuế
        /// </summary>
        Locked = 4
    }
}
