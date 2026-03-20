using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using InventoryKpiSystem.Core.Models;

namespace InventoryKpiSystem.Core.Services
{
    public class ReportExporter
    {
        // Khóa để đảm bảo an toàn đa luồng
        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        public async Task ExportReportAsync(string filePath, KpiReport report)
        {
            await _fileLock.WaitAsync();
            try
            {
                using (FileStream createStream = File.Create(filePath))
                {
                    await JsonSerializer.SerializeAsync(createStream, report, new JsonSerializerOptions { WriteIndented = true });
                }
                Console.WriteLine($"Đã xuất file JSON thành công tại: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Lỗi] Không thể ghi file: {ex.Message}");
            }
            finally
            {
                _fileLock.Release();
            }
        }
    }
}