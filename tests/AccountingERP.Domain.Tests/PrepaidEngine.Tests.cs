using System;
using System.Linq;
using AccountingERP.Domain.AccountingPrinciples.Engines;
using AccountingERP.Domain.AccountingPrinciples;
using Xunit;

namespace AccountingERP.Domain.Tests
{
    public class PrepaidEngineTests
    {
        [Fact]
        public void AllocatePrepaid_CreatesSchedule()
        {
            var engine = new PrepaidEngine();
            engine.AllocatePrepaid(new PrepaidAllocationRequest
            {
                Amount = 1200m,
                Months = 12,
                AssetAccount = "Asset:Prepaid"
            });

            var prepaids = engine.GetPrepaids();
            Assert.Single(prepaids);
            var p = prepaids[0];
            Assert.Equal(12, p.Months);
            Assert.Equal(12, p.Schedule.Count);
            Assert.Equal(100m, p.Schedule[0].Amount);
            Assert.Equal(1, p.Schedule[0].Month);
        }
    }
}
