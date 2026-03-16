using System;
using System.Collections.Generic;
using System.Linq;
using InventoryKpiSystem.Core.DataAccess;
using InventoryKpiSystem.Core.Models;

namespace InventoryKpiSystem.Core.Services
{
    public class KpiCalculator
    {
        /// <summary>
        /// Build a HashSet of valid product IDs from the catalog,
        /// using the same normalization logic as XeroDataImporter.
        /// </summary>
        private static HashSet<string> BuildCatalogLookup(List<Product> catalog)
        {
            var validIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var p in catalog)
            {
                if (!p.IsSold && !p.IsPurchased) continue;

                // Match ItemCode → product.Code (lowercased, trimmed)
                if (!string.IsNullOrWhiteSpace(p.Code))
                    validIds.Add(p.Code.Trim().ToLowerInvariant());

                // Match NormalizeDescription(Description) → for line items without ItemCode
                if (!string.IsNullOrWhiteSpace(p.Name))
                {
                    var normalizedName = XeroDataImporter.NormalizeDescription(p.Name);
                    if (!string.IsNullOrEmpty(normalizedName))
                        validIds.Add(normalizedName);
                }

                if (!string.IsNullOrWhiteSpace(p.Description))
                {
                    var normalizedDesc = XeroDataImporter.NormalizeDescription(p.Description);
                    if (!string.IsNullOrEmpty(normalizedDesc))
                        validIds.Add(normalizedDesc);
                }
            }

            return validIds;
        }

        public List<ProductKpi> CalculatePerSkuKpis(
            IReadOnlyDictionary<string, ProductAggregate> aggregates,
            DateTime? referenceDate = null)
        {
            var today = referenceDate ?? DateTime.Today;
            var results = new List<ProductKpi>();

            foreach (var kvp in aggregates)
            {
                var productId = kvp.Key;
                var agg = kvp.Value;

                // KPI 2: Stock Value
                var unsoldQty = Math.Max(0, agg.TotalPurchased - agg.TotalSold);
                var avgUnitCost = agg.TotalPurchased > 0
                    ? agg.TotalPurchaseCost / agg.TotalPurchased
                    : 0m;
                var stockValue = unsoldQty * avgUnitCost;

                // KPI 3: Out-of-Stock
                var currentStock = agg.TotalPurchased - agg.TotalSold;
                var isOutOfStock = currentStock <= 0;

                // KPI 4: Average Daily Sales
                var salesDaysCount = agg.SaleDates.Count;
                var avgDailySales = salesDaysCount > 0
                    ? (decimal)agg.TotalSold / salesDaysCount
                    : 0m;

                // KPI 5: Average Inventory Age
                var inventoryAge = 0m;
                if (unsoldQty > 0 && agg.EarliestPurchaseDate != DateTime.MaxValue)
                {
                    var days = (today - agg.EarliestPurchaseDate.Date).Days;
                    inventoryAge = Math.Max(0, days);
                }

                results.Add(new ProductKpi
                {
                    ProductId = productId,
                    CurrentStock = currentStock,
                    StockValue = Math.Round(stockValue, 2),
                    AvgDailySales = Math.Round(avgDailySales, 2),
                    InventoryAgeDays = (int)inventoryAge,
                    IsOutOfStock = isOutOfStock
                });
            }

            return results;
        }

        public SystemWideKpi CalculateSystemWideKpis(
            List<ProductKpi> perSkuKpis,
            IReadOnlyDictionary<string, ProductAggregate> aggregates,
            List<Product>? productCatalog = null)
        {
            if (perSkuKpis.Count == 0 && (productCatalog == null || productCatalog.Count == 0))
                return new SystemWideKpi();

            // ── KPI 1: Total SKUs ──────────────────────────────────────────
            var totalSkus = productCatalog != null && productCatalog.Count > 0
                ? productCatalog.Count(p => p.IsSold || p.IsPurchased)
                : perSkuKpis.Count;

            // ── KPI 2: Total Stock Value ───────────────────────────────────
            var totalStockValue = perSkuKpis.Sum(k => k.StockValue);

            // ── KPI 3: Out-of-Stock Items ──────────────────────────────────
            var outOfStockCount = perSkuKpis.Count(k => k.IsOutOfStock);

            // ── KPI 4: Average Daily Sales ─────────────────────────────────
            var allSaleDates = new HashSet<DateTime>();
            var totalSold = 0;

            foreach (var agg in aggregates.Values)
            {
                totalSold += agg.TotalSold;
                foreach (var date in agg.SaleDates)
                    allSaleDates.Add(date);
            }

            var systemAvgDailySales = allSaleDates.Count > 0
                ? (decimal)totalSold / allSaleDates.Count
                : 0m;

            // ── KPI 5: Average Inventory Age ───────────────────────────────
            var skusWithStock = perSkuKpis.Where(k => !k.IsOutOfStock).ToList();
            var avgInventoryAge = skusWithStock.Count > 0
                ? skusWithStock.Average(k => (double)k.InventoryAgeDays)
                : 0.0;

            return new SystemWideKpi
            {
                TotalSkus = totalSkus,
                TotalStockValue = Math.Round(totalStockValue, 2),
                OutOfStockCount = outOfStockCount,
                AvgDailySales = Math.Round(systemAvgDailySales, 2),
                AvgInventoryAgeDays = Math.Round(avgInventoryAge, 1)
            };
        }

        public KpiReport GenerateReport(
            IReadOnlyDictionary<string, ProductAggregate> aggregates,
            List<Product>? productCatalog = null,
            DateTime? referenceDate = null)
        {
            // ── Lọc aggregates: chỉ giữ sản phẩm có trong catalog ──────────
            IReadOnlyDictionary<string, ProductAggregate> filteredAggregates = aggregates;

            if (productCatalog != null && productCatalog.Count > 0)
            {
                var validIds = BuildCatalogLookup(productCatalog);
                var filtered = new Dictionary<string, ProductAggregate>();

                foreach (var kvp in aggregates)
                {
                    if (validIds.Contains(kvp.Key))
                        filtered[kvp.Key] = kvp.Value;
                }

                filteredAggregates = filtered;
            }

            var perSkuKpis = CalculatePerSkuKpis(filteredAggregates, referenceDate);
            var systemWideKpis = CalculateSystemWideKpis(perSkuKpis, filteredAggregates, productCatalog);

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