using InventoryKpiSystem.Core.Interfaces;
using InventoryKpiSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryKpiSystem.Core.Services
{
    public class KpiCalculator : IKpiCalculator
    {
        public async Task<List<ProductKpi>> CalculatePerSkuKpisAsync(IReadOnlyDictionary<string, ProductAggregate> aggregates, DateTime? referenceDate = null)
        {
            return await Task.Run(() =>
            {
                var today = referenceDate ?? DateTime.Today;

                return aggregates.Select(kvp =>
                {
                    var productId = kvp.Key;
                    var agg = kvp.Value;

                    var unsoldQty = Math.Max(0, agg.TotalPurchased - agg.TotalSold);
                    var avgUnitCost = agg.TotalPurchased > 0
                        ? agg.TotalPurchaseCost / agg.TotalPurchased
                        : 0m;
                    var stockValue = unsoldQty * avgUnitCost;

                    int ageDays = (today - agg.EarliestPurchaseDate).Days;

                    return new ProductKpi
                    {
                        ProductId = productId,
                        StockValue = Math.Round(stockValue, 2),
                        IsOutOfStock = unsoldQty <= 0,
                        InventoryAgeDays = Math.Max(0, ageDays)
                    };
                }).ToList();
            });
        }

        public async Task<SystemWideKpi> CalculateSystemWideKpisAsync(List<ProductKpi> perSkuKpis, IReadOnlyDictionary<string, ProductAggregate> aggregates)
        {
            return await Task.Run(() =>
            {
                if (perSkuKpis == null || !perSkuKpis.Any())
                    return new SystemWideKpi();

                decimal totalStockValue = perSkuKpis.Sum(k => k.StockValue);
                int outOfStockCount = perSkuKpis.Count(k => k.IsOutOfStock);

                // Tính toán trực tiếp từ aggregates thay vì dùng qua ProductKpi để tránh lỗi thiếu định nghĩa
                double avgSales = aggregates.Any()
                    ? aggregates.Average(kvp => kvp.Value.SaleDates.Count > 0 ? (double)kvp.Value.TotalSold / kvp.Value.SaleDates.Count : 0)
                    : 0;

                var skusWithStock = perSkuKpis.Where(k => !k.IsOutOfStock).ToList();
                double avgInventoryAge = skusWithStock.Any()
                    ? skusWithStock.Average(k => (double)k.InventoryAgeDays)
                    : 0.0;

                return new SystemWideKpi
                {
                    TotalSkus = perSkuKpis.Count,
                    TotalStockValue = Math.Round(totalStockValue, 2),
                    OutOfStockCount = outOfStockCount,
                    // Ép kiểu từ double sang decimal
                    AvgDailySales = (decimal)Math.Round(avgSales, 2),
                    AvgInventoryAgeDays = Math.Round(avgInventoryAge, 1)
                };
            });
        }

        public async Task<KpiReport> GenerateReportAsync(IReadOnlyDictionary<string, ProductAggregate> aggregates, DateTime? referenceDate = null)
        {
            var perSkuKpis = await CalculatePerSkuKpisAsync(aggregates, referenceDate);
            var systemWideKpis = await CalculateSystemWideKpisAsync(perSkuKpis, aggregates);

            return new KpiReport
            {
                ReportId = Guid.NewGuid().ToString(),
                ExportedDate = DateTime.Now,
                Details = perSkuKpis,
                TotalStockValue = systemWideKpis.TotalStockValue,
                TotalProductsProcessed = systemWideKpis.TotalSkus,
                SystemWide = systemWideKpis
            };
        }
    }
}