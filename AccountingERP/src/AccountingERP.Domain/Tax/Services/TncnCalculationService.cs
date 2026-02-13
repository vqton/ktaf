using System;
using System.Collections.Generic;
using System.Linq;
using AccountingERP.Domain.Entities;
using AccountingERP.Domain.ValueObjects;

namespace AccountingERP.Domain.Tax.Services
{
    /// <summary>
    /// Domain Service: Tính toán thuế TNCN
    /// TT111/2013/TT-BTC: Thuế thu nhập cá nhân
    /// </summary>
    public interface ITncnCalculationService
    {
        TncnReport CalculateMonthlyTncn(Employee employee, decimal grossSalary);
        TncnReport CalculateAnnualTncn(Employee employee, IEnumerable<MonthlyIncome> monthlyIncomes);
        decimal CalculateProgressiveTax(decimal taxableIncome);
        bool IsResident(string identification, int daysInVietnam);
        Money CalculateWithholdingTax(PaymentToIndividual payment);
    }

    /// <summary>
    /// Implementation of TNCN Calculation Service
    /// </summary>
    public class TncnCalculationService : ITncnCalculationService
    {
        private const decimal PERSONAL_DEDUCTION = 11_000_000m; // 11 triệu/tháng
        private const decimal DEPENDENT_DEDUCTION = 4_400_000m; // 4.4 triệu/ngườii phụ thuộc/tháng
        private const decimal BASE_SALARY_2026 = 2_340_000m; // Lương cơ sở 2026

        /// <summary>
        /// Tính thuế TNCN tháng cho nhân viên
        /// </summary>
        public TncnReport CalculateMonthlyTncn(Employee employee, decimal grossSalary)
        {
            // 1. Calculate insurance deductions
            var insuranceDeductions = CalculateInsuranceDeductions(employee, grossSalary);

            // 2. Calculate personal and dependent deductions
            var personalDeduction = PERSONAL_DEDUCTION;
            var dependentDeduction = employee.Dependents * DEPENDENT_DEDUCTION;
            var totalDeductions = insuranceDeductions + personalDeduction + dependentDeduction;

            // 3. Calculate taxable income
            var taxableIncome = Math.Max(0, grossSalary - totalDeductions);

            // 4. Calculate tax
            decimal tax;
            if (employee.IsResident)
            {
                tax = CalculateProgressiveTax(taxableIncome);
            }
            else
            {
                // Non-resident: flat 20%
                tax = taxableIncome * 0.20m;
            }

            // 5. Calculate net salary
            var netSalary = grossSalary - insuranceDeductions - tax;

            return new TncnReport
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                TaxPeriod = "Monthly",
                GrossSalary = grossSalary,
                InsuranceDeductions = insuranceDeductions,
                PersonalDeduction = personalDeduction,
                DependentDeduction = dependentDeduction,
                TaxableIncome = taxableIncome,
                TaxAmount = tax,
                NetSalary = netSalary,
                TaxRate = tax / taxableIncome,
                CalculationDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Tính thuế TNCN năm (quyết toán năm)
        /// </summary>
        public TncnReport CalculateAnnualTncn(Employee employee, IEnumerable<MonthlyIncome> monthlyIncomes)
        {
            var annualGrossSalary = monthlyIncomes.Sum(m => m.GrossSalary);
            var annualInsurance = monthlyIncomes.Sum(m => m.InsuranceDeductions);
            
            // Annual deductions
            var personalDeduction = PERSONAL_DEDUCTION * 12;
            var dependentDeduction = employee.Dependents * DEPENDENT_DEDUCTION * 12;
            var totalDeductions = annualInsurance + personalDeduction + dependentDeduction;

            // Taxable income
            var taxableIncome = Math.Max(0, annualGrossSalary - totalDeductions);

            // Calculate tax
            decimal annualTax;
            if (employee.IsResident)
            {
                annualTax = CalculateProgressiveTax(taxableIncome);
            }
            else
            {
                annualTax = taxableIncome * 0.20m;
            }

            // Compare with monthly withholdings
            var monthlyTaxesPaid = monthlyIncomes.Sum(m => m.TaxWithheld);
            var taxToPay = annualTax - monthlyTaxesPaid;

            return new TncnReport
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                TaxPeriod = $"Annual-{monthlyIncomes.First().Year}",
                GrossSalary = annualGrossSalary,
                InsuranceDeductions = annualInsurance,
                PersonalDeduction = personalDeduction,
                DependentDeduction = dependentDeduction,
                TaxableIncome = taxableIncome,
                TaxAmount = annualTax,
                TaxPaid = monthlyTaxesPaid,
                TaxToPay = taxToPay,
                NetSalary = annualGrossSalary - annualInsurance - annualTax,
                TaxRate = annualTax / taxableIncome,
                CalculationDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Tính thuế theo biểu thuế lũy tiến từng phần
        /// TT111-Điều 22: Biểu thuế TNCN
        /// </summary>
        public decimal CalculateProgressiveTax(decimal taxableIncome)
        {
            if (taxableIncome <= 0) return 0;

            // Biểu thuế lũy tiến từng phần (tháng)
            // Bậc 1: 0-5 triệu: 5%
            // Bậc 2: 5-10 triệu: 10% - 0.25 triệu
            // Bậc 3: 10-18 triệu: 15% - 0.75 triệu
            // Bậc 4: 18-32 triệu: 20% - 1.65 triệu
            // Bậc 5: 32-52 triệu: 25% - 3.25 triệu
            // Bậc 6: 52-80 triệu: 30% - 5.85 triệu
            // Bậc 7: >80 triệu: 35% - 9.85 triệu

            var tax = 0m;

            if (taxableIncome <= 5_000_000)
            {
                tax = taxableIncome * 0.05m;
            }
            else if (taxableIncome <= 10_000_000)
            {
                tax = taxableIncome * 0.10m - 250_000m;
            }
            else if (taxableIncome <= 18_000_000)
            {
                tax = taxableIncome * 0.15m - 750_000m;
            }
            else if (taxableIncome <= 32_000_000)
            {
                tax = taxableIncome * 0.20m - 1_650_000m;
            }
            else if (taxableIncome <= 52_000_000)
            {
                tax = taxableIncome * 0.25m - 3_250_000m;
            }
            else if (taxableIncome <= 80_000_000)
            {
                tax = taxableIncome * 0.30m - 5_850_000m;
            }
            else
            {
                tax = taxableIncome * 0.35m - 9_850_000m;
            }

            return tax;
        }

        /// <summary>
        /// Kiểm tra cá nhân có phải là cư trú không
        /// </summary>
        public bool IsResident(string identification, int daysInVietnam)
        {
            // Cá nhân cư trú nếu thỏa mãn một trong các điều kiện:
            // 1. Có mặt tại VN >= 183 ngày trong năm dương lịch
            // 2. Có nơi ở thường trú tại VN
            // 3. Thuê nhà ở tại VN theo hợp đồng >= 183 ngày

            return daysInVietnam >= 183;
        }

        /// <summary>
        /// Tính thuế khấu trừ khi chi trả cho cá nhân
        /// </summary>
        public Money CalculateWithholdingTax(PaymentToIndividual payment)
        {
            // Khấu trừ thuế đối với cá nhân không phải là nhân viên
            // VD: Thuê ngoài, bản quyền, nhượng quyền...

            var tax = 0m;

            if (payment.PaymentType == PaymentType.Services && payment.Amount >= 2_000_000m)
            {
                // Thu nhập từ dịch vụ: 10% nếu >= 2 triệu/lần
                tax = payment.Amount * 0.10m;
            }
            else if (payment.PaymentType == PaymentType.Royalties)
            {
                // Bản quyền: 5%
                tax = payment.Amount * 0.05m;
            }
            else if (payment.PaymentType == PaymentType.Franchise)
            {
                // Nhượng quyền: 5%
                tax = payment.Amount * 0.05m;
            }
            else if (payment.PaymentType == PaymentType.Prizes)
            {
                // Trúng thưởng: 10% cho phần > 10 triệu
                if (payment.Amount > 10_000_000m)
                {
                    tax = (payment.Amount - 10_000_000m) * 0.10m;
                }
            }

            return Money.VND(tax);
        }

        private decimal CalculateInsuranceDeductions(Employee employee, decimal grossSalary)
        {
            // Bảo hiểm bắt buộc:
            // - BHXH: 8%
            // - BHYT: 1.5%
            // - BHTN: 1%
            // Tổng: 10.5%
            // Tính trên lương <= 20 lần lương cơ sở

            var maxBase = BASE_SALARY_2026 * 20;
            var insuranceBase = Math.Min(grossSalary, maxBase);

            var socialInsurance = insuranceBase * 0.08m;    // BHXH
            var healthInsurance = insuranceBase * 0.015m;   // BHYT
            var unemploymentInsurance = insuranceBase * 0.01m; // BHTN

            return socialInsurance + healthInsurance + unemploymentInsurance;
        }
    }

    /// <summary>
    /// Nhân viên
    /// </summary>
    public class Employee
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty; // CCCD/CMND
        public string TaxCode { get; set; } = string.Empty;  // MST cá nhân
        public bool IsResident { get; set; } = true;
        public int DaysInVietnam { get; set; }
        public int Dependents { get; set; }
        public string ContractType { get; set; } = string.Empty; // Chính thức/Thử việc/Hợp đồng
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Báo cáo TNCN
    /// </summary>
    public class TncnReport
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string TaxPeriod { get; set; } = string.Empty;
        public decimal GrossSalary { get; set; }
        public decimal InsuranceDeductions { get; set; }
        public decimal PersonalDeduction { get; set; }
        public decimal DependentDeduction { get; set; }
        public decimal TaxableIncome { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxPaid { get; set; }
        public decimal TaxToPay { get; set; }
        public decimal NetSalary { get; set; }
        public decimal TaxRate { get; set; }
        public DateTime CalculationDate { get; set; }
    }

    /// <summary>
    /// Thu nhập tháng
    /// </summary>
    public class MonthlyIncome
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal InsuranceDeductions { get; set; }
        public decimal TaxWithheld { get; set; }
    }

    /// <summary>
    /// Thanh toán cho cá nhân
    /// </summary>
    public class PaymentToIndividual
    {
        public Guid Id { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientTaxCode { get; set; } = string.Empty;
        public PaymentType PaymentType { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
    }

    public enum PaymentType
    {
        Services,       // Dịch vụ
        Royalties,      // Bản quyền
        Franchise,      // Nhượng quyền
        Prizes,         // Trúng thưởng
        Rent,           // Cho thuê tài sản
        Dividends       // Cổ tức
    }
}
