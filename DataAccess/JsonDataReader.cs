using System.Text.Json;
using InventoryKpiSystem.Core.Models;

namespace InventoryKpiSystem.Core.DataAccess
{
    public class JsonDataReader
    {
        public async Task<List<T>?> ReadDataAsync<T>(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            using FileStream openStream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<List<T>>(openStream);
        }
    }
}