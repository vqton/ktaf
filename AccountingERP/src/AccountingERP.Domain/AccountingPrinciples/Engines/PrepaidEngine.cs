using System;
using System.Collections.Generic;
using AccountingERP.Domain.AccountingPrinciples;

namespace AccountingERP.Domain.AccountingPrinciples.Engines
{
    public class PrepaidEngine : IAcctEngine
    {
        private readonly List<PrepaidRecord> _prepaids = new();

        public void RecordAccrual(AccrualEvent accrual)
        {
            // Not applicable for prepaid engine; kept for interface compatibility
        }

        public void ReverseAccrual(string accrualId)
        {
            // Not applicable for prepaid engine; kept for interface compatibility
        }

        public void AllocatePrepaid(PrepaidAllocationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var perMonth = request.Amount / Math.Max(1, request.Months);
            var schedule = new List<MonthlyAllocation>();
            for (int m = 1; m <= request.Months; m++)
            {
                schedule.Add(new MonthlyAllocation { Month = m, Amount = perMonth });
            }
            _prepaids.Add(new PrepaidRecord
            {
                Id = Guid.NewGuid().ToString(),
                TotalAmount = request.Amount,
                Months = request.Months,
                AssetAccount = request.AssetAccount,
                StartDate = DateTime.UtcNow,
                Schedule = schedule
            });
        }

        public void CreateProvision(ProvisionRequest request)
        {
            // Not applicable for prepaid engine; kept for interface compatibility
        }

        public IReadOnlyList<PrepaidRecord> GetPrepaids() => _prepaids.AsReadOnly();
    }
}
