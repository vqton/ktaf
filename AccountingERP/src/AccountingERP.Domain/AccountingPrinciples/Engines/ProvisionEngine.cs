using System;
using System.Collections.Generic;
using AccountingERP.Domain.AccountingPrinciples;

namespace AccountingERP.Domain.AccountingPrinciples.Engines
{
    public class ProvisionEngine : IAcctEngine
    {
        private readonly List<ProvisionRecord> _provisions = new();

        public void RecordAccrual(AccrualEvent accrual) { }
        public void ReverseAccrual(string accrualId) { }
        public void AllocatePrepaid(PrepaidAllocationRequest request) { }
        public void CreateProvision(ProvisionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            _provisions.Add(new ProvisionRecord
            {
                Id = Guid.NewGuid().ToString(),
                Amount = request.Amount,
                Category = request.Category,
                AsOf = request.AsOf
            });
        }

        public IReadOnlyList<ProvisionRecord> GetProvisions() => _provisions.AsReadOnly();
    }
}
