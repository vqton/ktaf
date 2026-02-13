using System;
using System.Collections.Generic;
using AccountingERP.Domain.AccountingPrinciples;

namespace AccountingERP.Domain.AccountingPrinciples.Engines
{
    public class AccrualEngine : IAcctEngine
    {
        private readonly List<AccrualRecord> _accruals = new();

        public void RecordAccrual(AccrualEvent accrual)
        {
            if (accrual == null) throw new ArgumentNullException(nameof(accrual));
            _accruals.Add(new AccrualRecord
            {
                Id = accrual.Id,
                Amount = accrual.Amount,
                Date = accrual.Date,
                RevenueAccount = accrual.RevenueAccount,
                ExpenseAccount = accrual.ExpenseAccount,
                Reversed = false
            });
        }

        public void ReverseAccrual(string accrualId)
        {
            var item = _accruals.Find(a => a.Id == accrualId);
            if (item != null) item.Reversed = true;
        }

        public void AllocatePrepaid(PrepaidAllocationRequest request)
        {
            // Not implemented at this layer; in Phase A this would be wired to PrepaidEngine.
            // This method intentionally acts as a placeholder to satisfy interface completeness.
        }

        public void CreateProvision(ProvisionRequest request)
        {
            // Not implemented in this engine; ProvisionEngine should handle this.
        }

        // Simple query helpers for testing and basic usage
        public IReadOnlyList<AccrualRecord> GetAccruals() => _accruals.AsReadOnly();
    }
}
