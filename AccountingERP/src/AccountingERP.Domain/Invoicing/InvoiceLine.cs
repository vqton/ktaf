using System;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.Invoicing
{
    /// <summary>
    /// Dòng hàng hóa/dịch vụ trên hóa đơn
    /// </summary>
    public class InvoiceLine : BaseEntity
    {
        public InvoiceId InvoiceId { get; private set; }
        public string ProductCode { get; private set; } = string.Empty;
        public string ProductName { get; private set; } = string.Empty;
        public string Unit { get; private set; } = string.Empty;
        public decimal Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal Discount { get; private set; }
        public Money LineTotal { get; private set; } = Money.Zero(Currency.VND);
        public Currency Currency { get; private set; }
        
        private InvoiceLine() { } // EF Core
        
        public static InvoiceLine Create(
            InvoiceId invoiceId,
            string productCode,
            string productName,
            string unit,
            decimal quantity,
            decimal unitPrice,
            Currency currency,
            decimal? discount = null)
        {
            var disc = discount ?? 0m;
            var total = (quantity * unitPrice) - disc;
            
            return new InvoiceLine
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoiceId,
                ProductCode = productCode?.Trim() ?? string.Empty,
                ProductName = productName?.Trim() ?? string.Empty,
                Unit = unit?.Trim() ?? "cái",
                Quantity = quantity,
                UnitPrice = unitPrice,
                Discount = disc,
                LineTotal = Money.Create(total, currency),
                Currency = currency
            };
        }
    }
}
