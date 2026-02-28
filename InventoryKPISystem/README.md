InventoryKpiSystem/
│
├── 📁 data/                                # DỮ LIỆU
│   ├── invoices/                          # Input: Hóa đơn bán
│   ├── purchase-orders/                   # Input: Đơn mua hàng
│   ├── reports/                           # ✨ THÊM: Output KPI reports
│   └── processed-files/                   # ✨ THÊM: File tracking registry
│
├── 📁 logs/                                # ✨ THÊM: LOGS
│   ├── app.log
│   └── errors.log
│
├── 📁 docs/                                # TÀI LIỆU
│   ├── Report.docx                        # Báo cáo 4 trang
│   ├── Presentation.pptx                  # Slides 15 phút
│   └── README.md
│
├── 📦 InventoryKpiSystem.Core/             # CORE BUSINESS LOGIC
│   │
│   ├── 📁 Models/                          # [THÀNH VIÊN 1]
│   │   ├── Invoice.cs
│   │   ├── PurchaseOrder.cs
│   │   ├── Product.cs
│   │   ├── KpiReport.cs                   # ✨ THÊM
│   │   └── ProductKpi.cs                  # ✨ THÊM
│   │
│   ├── 📁 DataAccess/                      # ✨ THÊM [THÀNH VIÊN 1]
│   │   ├── JsonDataReader.cs
│   │   ├── DataValidator.cs
│   │   └── ValidationResult.cs
│   │
│   ├── 📁 Services/                        # [THÀNH VIÊN 2]
│   │   ├── KpiCalculator.cs
│   │   └── IncrementalKpiUpdater.cs       # ✨ THÊM
│   │
│   ├── 📁 Interfaces/
│   │   ├── IKpiCalculator.cs
│   │   ├── IFileProcessor.cs
│   │   ├── IDataReader.cs                 # ✨ THÊM
│   │   ├── IFileTracker.cs                # ✨ THÊM
│   │   └── ILogger.cs                     # ✨ THÊM
│   │
│   └── 📁 Exceptions/                      # ✨ THÊM
│       ├── DataValidationException.cs
│       └── FileProcessingException.cs
│
├── 📦 InventoryKpiSystem.Infrastructure/   # INFRASTRUCTURE
│   │
│   ├── 📁 Monitoring/                      # [THÀNH VIÊN 3]
│   │   ├── InventoryWatcher.cs
│   │   └── FileEventArgs.cs               # ✨ THÊM
│   │
│   ├── 📁 Queuing/                         # [THÀNH VIÊN 3]
│   │   ├── FileProcessingQueue.cs
│   │   ├── FileTask.cs                    # ✨ THÊM
│   │   └── ProcessingResult.cs            # ✨ THÊM
│   │
│   ├── 📁 Processing/                      # ✨ THÊM [THÀNH VIÊN 3]
│   │   ├── RetryHandler.cs
│   │   └── FileProcessor.cs
│   │
│   ├── 📁 Persistence/                     # [THÀNH VIÊN 4]
│   │   ├── KpiRegistry.cs
│   │   ├── FileTracker.cs
│   │   └── processed-files.json           # Data file
│   │
│   └── 📁 Logging/                         # ✨ THÊM [THÀNH VIÊN 4]
│       ├── Logger.cs
│       └── ProcessingLogger.cs
│
├── 📦 InventoryKpiSystem.ConsoleApp/       # CONSOLE APPLICATION
│   │
│   ├── Program.cs                         # Main entry point
│   ├── appsettings.json                   # Configuration
│   │
│   ├── 📁 Display/                         # [THÀNH VIÊN 4]
│   │   ├── ConsoleFormatter.cs            # ✨ THÊM
│   │   └── JsonReportGenerator.cs         # ✨ THÊM
│   │
│   ├── 📁 Configuration/                   # ✨ THÊM [THÀNH VIÊN 4]
│   │   └── AppConfig.cs
│   │
│   └── 📁 Coordinators/                    # ✨ THÊM [THÀNH VIÊN 4]
│       └── ApplicationCoordinator.cs
│
├── 📦 InventoryKpiSystem.Tests/            # TESTING
│   │
│   ├── 📁 UnitTests/
│   │   ├── ModelTests.cs                  # [THÀNH VIÊN 1]
│   │   ├── DataAccessTests.cs             # ✨ THÊM [THÀNH VIÊN 1]
│   │   ├── KpiLogicTests.cs               # [THÀNH VIÊN 2]
│   │   ├── FileMonitoringTests.cs         # ✨ THÊM [THÀNH VIÊN 3]
│   │   └── ValidationTests.cs             # ✨ THÊM
│   │
│   ├── 📁 IntegrationTests/                # ✨ TÁCH RA
│   │   ├── EndToEndTests.cs               # [THÀNH VIÊN 4]
│   │   ├── ConcurrentProcessingTests.cs   # ✨ THÊM
│   │   └── PerformanceTests.cs            # ✨ THÊM
│   │
│   └── 📁 TestData/                        # ✨ THÊM
│       ├── sample_invoices.json
│       ├── sample_purchase_orders.json
│       └── invalid_data.json
│
├── InventoryKpiSystem.sln                 # Solution file
├── .gitignore                             # ✨ THÊM
└── README.md                              # ✨ THÊM