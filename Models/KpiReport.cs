using System.Text.Json.Serialization;

namespace InventoryKPI.Models
{
    public class PurchaseOrder
    {
        [JsonPropertyName("OrderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("ProductId")]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("QuantityPurchased")]
        public int Quantity { get; set; } 

        [JsonPropertyName("UnitCost")]
        public decimal UnitCost { get; set; }

        [JsonPropertyName("Date")]
        public DateTime Date { get; set; }
    }
}