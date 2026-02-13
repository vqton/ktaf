namespace AccountingERP.Domain.Exceptions
{
    /// <summary>
    /// Exception khi hóa đơn không khớp với bút toán kế toán
    /// </summary>
    public class InvoiceAccountingMismatchException : DomainException
    {
        public InvoiceAccountingMismatchException(string message) : base(message) { }
        
        public InvoiceAccountingMismatchException(string invoiceNumber, string reason)
            : base($"Hóa đơn {invoiceNumber}: {reason}") { }
    }
}
