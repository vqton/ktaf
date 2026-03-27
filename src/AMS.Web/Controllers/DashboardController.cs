using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AMS.Web.Controllers
{
    /// <summary>
    /// Controller for dashboard data and widgets.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        /// <summary>
        /// Gets key performance indicators for the dashboard.
        /// </summary>
        /// <returns>KPI data.</returns>
        [HttpGet("kpis")]
        public IActionResult GetKpis()
        {
            // TODO: Replace with actual data from services
            var kpis = new
            {
                revenue = 125000000,
                revenueChange = 12.5,
                expenses = 98000000,
                expensesChange = 8.3,
                profit = 27000000,
                profitChange = 22.1,
                cash = 45000000,
                cashChange = 5.7
            };

            return Ok(new { success = true, data = kpis });
        }

        /// <summary>
        /// Gets revenue vs expense chart data for the specified period.
        /// </summary>
        /// <param name="period">The period (month, quarter, year).</param>
        /// <returns>Chart data.</returns>
        [HttpGet("charts/revenue-expense")]
        public IActionResult GetRevenueExpenseChart(string period)
        {
            // TODO: Replace with actual data from services
            var labels = new[] { "T1", "T2", "T3", "T4", "T5", "T6" };
            var revenueData = new[] { 120, 150, 180, 140, 160, 200 };
            var expenseData = new[] { 80, 90, 100, 85, 95, 110 };

            var data = new
            {
                labels,
                revenueData,
                expenseData
            };

            return Ok(new { success = true, data });
        }

        /// <summary>
        /// Gets expense breakdown chart data.
        /// </summary>
        /// <returns>Chart data.</returns>
        [HttpGet("charts/expense-breakdown")]
        public IActionResult GetExpenseBreakdownChart()
        {
            // TODO: Replace with actual data from services
            var labels = new[] { "Nhân sự", "Mua hàng", "Chi phí khác", "Thuế" };
            var data = new[] { 40, 35, 15, 10 };
            var backgroundColor = new[] { "#1D6FA4", "#22C55E", "#F59E0B", "#EF4444" };

            var chartData = new
            {
                labels,
                data,
                backgroundColor
            };

            return Ok(new { success = true, data = chartData });
        }

        /// <summary>
        /// Gets pending vouchers for approval.
        /// </summary>
        /// <returns>List of pending vouchers.</returns>
        [HttpGet("vouchers/pending")]
        public IActionResult GetPendingVouchers()
        {
            // TODO: Replace with actual data from services
            var vouchers = new[]
            {
                new
                {
                    id = Guid.NewGuid(),
                    voucherNo = "CT000123",
                    voucherDate = DateTime.Now.AddDays(-2),
                    description = "Mua nguyên vật liệu từ Công ty ABC",
                    totalDebit = 45000000
                },
                new
                {
                    id = Guid.NewGuid(),
                    voucherNo = "CT000124",
                    voucherDate = DateTime.Now.AddDays(-1),
                    description = "Thanh toán tiền lương tháng 3",
                    totalDebit = 67000000
                },
                new
                {
                    id = Guid.NewGuid(),
                    voucherNo = "CT000125",
                    voucherDate = DateTime.Now.AddDays(-3),
                    description = "Chi phí quảng cáo trên Facebook",
                    totalDebit = 12000000
                }
            };

            return Ok(vouchers);
        }

        /// <summary>
        /// Gets recent transactions.
        /// </summary>
        /// <returns>List of recent transactions.</returns>
        [HttpGet("vouchers/recent")]
        public IActionResult GetRecentTransactions()
        {
            // TODO: Replace with actual data from services
            var transactions = new[]
            {
                new
                {
                    voucherDate = DateTime.Now.AddHours(-2),
                    description = "Bán hàng cho khách hàng XYZ",
                    totalDebit = 35000000,
                    totalCredit = 35000000
                },
                new
                {
                    voucherDate = DateTime.Now.AddHours(-5),
                    description = "Mua đồ dùng văn phòng",
                    totalDebit = 2500000,
                    totalCredit = 2500000
                },
                new
                {
                    voucherDate = DateTime.Now.AddHours(-8),
                    description = "Thu tiền khách hàng trước",
                    totalDebit = 0,
                    totalCredit = 15000000
                }
            };

            return Ok(transactions);
        }
    }
}