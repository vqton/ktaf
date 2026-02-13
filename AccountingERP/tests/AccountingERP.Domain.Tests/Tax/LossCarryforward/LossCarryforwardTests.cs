using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Tax.LossCarryforward;
using AccountingERP.Domain.Tax.Services;
using AccountingERP.Domain.ValueObjects;
using Xunit;

namespace AccountingERP.Domain.Tests.Tax.LossCarryforward
{
    public class LossCarryforwardTests
    {
        private TaxLossYear CreateLossYear(int year, decimal amount, Guid periodId)
        {
            return TaxLossYear.Create(year, Money.Create(amount, Currency.VND), periodId);
        }

        private Guid CreatePeriodId() => Guid.NewGuid();

        [Fact]
        public void TaxLossYear_Create_ValidData_ShouldSucceed()
        {
            var periodId = CreatePeriodId();
            var lossYear = TaxLossYear.Create(2020, Money.Create(1000000, Currency.VND), periodId, "0123456789");

            Assert.NotNull(lossYear);
            Assert.Equal(2020, lossYear.OriginYear);
            Assert.Equal(1000000m, lossYear.InitialAmount.Amount);
            Assert.Equal(1000000m, lossYear.RemainingAmount.Amount);
            Assert.Equal(2025, lossYear.ExpiryYear);
        }

        [Fact]
        public void TaxLossYear_IsExpired_After5Years_ShouldBeTrue()
        {
            var lossYear = CreateLossYear(2020, 1000000, CreatePeriodId());
            Assert.True(lossYear.IsExpired(2026));
        }

        [Fact]
        public void TaxLossYear_ApplyLoss_ValidAmount_ShouldReduceRemaining()
        {
            var periodId = CreatePeriodId();
            var lossYear = CreateLossYear(2020, 1000000, periodId);
            
            var applied = lossYear.ApplyLoss(Money.Create(300000, Currency.VND), 2021, periodId, "accountant");

            Assert.Equal(300000m, applied.Amount);
            Assert.Equal(700000m, lossYear.RemainingAmount.Amount);
        }

        [Fact]
        public void TaxLossYear_ApplyLoss_ExpiredYear_ShouldThrowException()
        {
            var periodId = CreatePeriodId();
            var lossYear = CreateLossYear(2020, 1000000, periodId);

            var ex = Assert.Throws<InvalidOperationException>(() => 
                lossYear.ApplyLoss(Money.Create(100000, Currency.VND), 2026, periodId, "accountant"));
            
            Assert.Contains("hết hạn", ex.Message);
        }

        [Fact]
        public void LossCarryforwardService_ApplyLoss_OldestFirst_ShouldApplyFIFO()
        {
            var service = new LossCarryforwardService();
            var periodId = CreatePeriodId();
            var losses = new List<TaxLossYear>
            {
                CreateLossYear(2020, 500000, periodId),
                CreateLossYear(2019, 300000, periodId) // Oldest
            };

            var result = service.ApplyLossToTaxableIncome(
                losses, Money.Create(400000, Currency.VND), 2021, periodId, "accountant");

            Assert.True(result.IsSuccessful);
            Assert.Equal(2019, result.Details[0].OriginYear); // Oldest applied first
        }

        [Fact]
        public void LossCarryforwardService_CalculateTaxableIncomeAfterLoss_ShouldDeductCorrectly()
        {
            var service = new LossCarryforwardService();
            var periodId = CreatePeriodId();
            var losses = new List<TaxLossYear>
            {
                CreateLossYear(2020, 500000, periodId)
            };

            var result = service.CalculateTaxableIncomeAfterLoss(
                Money.Create(1000000, Currency.VND), losses, 2021);

            Assert.Equal(500000m, result.Amount);
        }
    }
}
