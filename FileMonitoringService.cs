using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class FileMonitoringService
{
    private readonly FileSystemWatcher watcher;
    private readonly BlockingCollection<string> queue;
    private readonly HashSet<string> processedFiles;
    private readonly int maxRetries = 3;

    public FileMonitoringService(string path)
    {
        queue = new BlockingCollection<string>();
        processedFiles = new HashSet<string>();

        watcher = new FileSystemWatcher(path)
        {
            Filter = "*.json",
            EnableRaisingEvents = false,
            IncludeSubdirectories = false
        };

        watcher.Created += OnCreated;
        watcher.Renamed += OnRenamed;
    }

    // START
    public void Start()
    {
        watcher.EnableRaisingEvents = true;
        Console.WriteLine("File monitoring started...");
    }

    public void Stop()
    {
        watcher.EnableRaisingEvents = false;
        queue.CompleteAdding();
    }

    // FILE EVENTS (PRODUCER) 
    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        EnqueueFile(e.FullPath);
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        EnqueueFile(e.FullPath);
    }

    private void EnqueueFile(string path)
    {
        lock (processedFiles)
        {
            if (processedFiles.Contains(path))
            {
                Console.WriteLine($"Duplicate ignored: {path}");
                return;
            }

            processedFiles.Add(path);
        }

        Console.WriteLine($"Queued: {path}");
        queue.Add(path);
    }

    // BACKGROUND PROCESSING
    public async Task StartProcessingAsync(int workerCount = 2)
    {
        var tasks = new List<Task>();

        for (int i = 0; i < workerCount; i++)
        {
            tasks.Add(Task.Run(ProcessQueue));
        }

        await Task.WhenAll(tasks);
    }

    // CONSUMER 
    private void ProcessQueue()
    {
        foreach (var filePath in queue.GetConsumingEnumerable())
        {
            ProcessFileWithRetry(filePath);
        }
    }

    // FILE PROCESSING 
    private void ProcessFileWithRetry(string filePath)
    {
        int attempts = 0;

        while (attempts < maxRetries)
        {
            try
            {
                ProcessFile(filePath);
                return;
            }
            catch (IOException)
            {
                attempts++;
                Console.WriteLine($" Retry {attempts} for {filePath}");
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error: {ex.Message}");
                return;
            }
        }

        Console.WriteLine($" Failed after retries: {filePath}");
    }

    private void ProcessFile(string filePath)
    {
        Console.WriteLine($"Processing: {filePath}");

        string content = File.ReadAllText(filePath);

        try
        {
            JsonDocument.Parse(content);  // kiểm tra JSON hợp lệ
            Console.WriteLine($"Done: {Path.GetFileName(filePath)}");
        }
        catch
        {
            Console.WriteLine($"ERROR: Invalid JSON -> {filePath}");
        }
    }
}