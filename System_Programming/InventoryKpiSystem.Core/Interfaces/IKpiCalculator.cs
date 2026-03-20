using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryKpiSystem.Core.Models;

namespace InventoryKpiSystem.Core.Interfaces
{
    public interface IKpiCalculator
    {
        // Kiểu trả về phải là Task để khớp với file KpiCalculator.cs
        Task<List<ProductKpi>> CalculatePerSkuKpisAsync(
            IReadOnlyDictionary<string, ProductAggregate> aggregates,
            DateTime? referenceDate = null);

        Task<SystemWideKpi> CalculateSystemWideKpisAsync(
            List<ProductKpi> perSkuKpis,
            IReadOnlyDictionary<string, ProductAggregate> aggregates);

        Task<KpiReport> GenerateReportAsync(
            IReadOnlyDictionary<string, ProductAggregate> aggregates,
            DateTime? referenceDate = null);
    }
}