using InventoryKpiSystem.Core.Models;
using System;
using System.Collections.Generic;

namespace InventoryKpiSystem.Core.KpiCalculators;

public class InventoryKpiCalculator
{
    // Total SKUs
    public int CalculateTotalSkus(
        List<Invoice> invoices,
        List<PurchaseOrder> purchaseOrders)
    {
        return invoices.Select(i => i.ProductId)
            .Union(purchaseOrders.Select(p => p.ProductId))
            .Distinct()
            .Count();
    }

    // Stock Value
    public decimal CalculateStockValue(
        List<Invoice> invoices,
        List<PurchaseOrder> purchaseOrders)
    {
        return purchaseOrders
            .GroupBy(p => p.ProductId)
            .Sum(group =>
            {
                var purchased = group.Sum(p => p.Quantity);

                var sold = invoices
                    .Where(i => i.ProductId == group.Key)
                    .Sum(i => i.Quantity);

                var unitCost = group.First().UnitCost;

                var unsold = purchased - sold;

                return unsold > 0 ? unsold * unitCost : 0;
            });
    }

    // Out Of Stock Items
    public int CalculateOutOfStockItems(
        List<Invoice> invoices,
        List<PurchaseOrder> purchaseOrders)
    {
        return purchaseOrders
            .GroupBy(p => p.ProductId)
            .Count(group =>
            {
                var purchased = group.Sum(p => p.Quantity);

                var sold = invoices
                    .Where(i => i.ProductId == group.Key)
                    .Sum(i => i.Quantity);

                return purchased - sold <= 0;
            });
    }

    // Average Daily Sales
    public double CalculateAverageDailySales(List<Invoice> invoices)
    {
        if (!invoices.Any())
            return 0;

        var totalSold = invoices.Sum(i => i.Quantity);

        var salesDays = invoices
            .Select(i => i.Date.Date)
            .Distinct()
            .Count();

        return salesDays == 0 ? 0 : (double)totalSold / salesDays;
    }

    // Average Inventory Age
    public double CalculateAverageInventoryAge(
        List<Invoice> invoices,
        List<PurchaseOrder> purchaseOrders)
    {
        var now = DateTime.Now;

        var inventoryAge = purchaseOrders
            .Where(p =>
                p.Quantity >
                invoices
                    .Where(i => i.ProductId == p.ProductId)
                    .Sum(i => i.Quantity))
            .Select(p => (now - p.Date).TotalDays);

        return inventoryAge.Any() ? inventoryAge.Average() : 0;
    }
}