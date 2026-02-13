using System;
using AccountingERP.Domain.AccountingPrinciples.Engines;
using AccountingERP.Domain.AccountingPrinciples;
using Xunit;

namespace AccountingERP.Domain.Tests
{
    public class ProvisionEngineTests
    {
        [Fact]
        public void CreateProvision_AddsRecord()
        {
            var engine = new ProvisionEngine();
            engine.CreateProvision(new ProvisionRequest { Amount = 500m, Category = "Warranty", AsOf = DateTime.Today });
            var provisions = engine.GetProvisions();
            Assert.Single(provisions);
            Assert.Equal(500m, provisions[0].Amount);
            Assert.Equal("Warranty", provisions[0].Category);
        }
    }
}
