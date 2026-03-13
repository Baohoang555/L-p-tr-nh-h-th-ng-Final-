namespace InventoryKpiSystem.Core.Models
{
    public class Product
    {
        public string ItemID { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty;

        public PriceDetails? PurchaseDetails { get; set; }
        public PriceDetails? SalesDetails { get; set; }
    }

    public class PriceDetails
    {
        public decimal UnitPrice { get; set; }
        public string AccountCode { get; set; } = string.Empty;
    }
}