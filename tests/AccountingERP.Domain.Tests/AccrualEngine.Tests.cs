using System;
using System.Linq;
using AccountingERP.Domain.AccountingPrinciples.Engines;
using AccountingERP.Domain.AccountingPrinciples;
using Xunit;

namespace AccountingERP.Domain.Tests
{
    public class AccrualEngineTests
    {
        [Fact]
        public void RecordAndReverseAccrual_Works()
        {
            var engine = new AccrualEngine();
            var accrual = new AccrualEvent
            {
                Id = "A1",
                Amount = 1000m,
                Date = DateTime.Today,
                RevenueAccount = "4000",
                ExpenseAccount = "5000"
            };
            engine.RecordAccrual(accrual);
            var list = engine.GetAccruals();
            Assert.Single(list);
            Assert.Equal(1000m, list[0].Amount);

            engine.ReverseAccrual("A1");
            Assert.True(list[0].Reversed);
        }
    }
}
