using InventoryKpiSystem.Infrastructure.Monitoring;
using InventoryKpiSystem.Infrastructure.Queuing;
using InventoryKpiSystem.Infrastructure.Processing;

string folder = "input";

Directory.CreateDirectory(folder);

var queue = new FileProcessingQueue();
var retryHandler = new RetryHandler(3);
var processor = new FileProcessor(retryHandler);

// WATCHER
var watcher = new InventoryWatcher(folder);

watcher.OnFileDetected += (args) =>
{
    Console.WriteLine($"[Watcher] File detected: {args.FilePath}");
    queue.Enqueue(new FileTask(args.FilePath));
};


Task.Run(async () =>
{
    while (true)
    {
        if (queue.TryDequeue(out var task))
        {
            var result = processor.Process(task!);

            if (!result.Success && result.Message == "Retry needed")
            {
                Console.WriteLine($"[Retry] {task!.FilePath}");
                queue.Enqueue(task!);
            }
            else
            {
                Console.WriteLine($"[Result] {result.FilePath} → {result.Message}");
            }
        }

        await Task.Delay(200);
    }
});

Console.WriteLine("Monitoring folder: input/");
Console.WriteLine("Drop JSON files here...");
Console.ReadLine();