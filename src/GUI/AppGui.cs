using GUI.Views.Cosmos;
using GUI.Views.FunctionApps;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Terminal.Gui;
using ILogger = Microsoft.Extensions.Logging.ILogger;

public class AppGui : IHostedService
{
    private readonly ILogger<AppGui> _logger;
    private readonly MainWindow _mainWindow;

    public AppGui(ILogger<AppGui> logger, MainWindow mainWindow)
    {
        _logger = logger;
        _mainWindow = mainWindow;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logging.Logger = _logger;
        Application.Init();
        // Override the default configuration for the application to use the Light theme
        ConfigurationManager.RuntimeConfig = """{ "Theme": "Light" }""";
        Application.Run(_mainWindow);

        // Before the application exits, reset Terminal.Gui for clean shutdown
        Application.Shutdown ();
        _logger.LogInformation("Application shutdown");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("StopAsync");
        return Task.CompletedTask;
    }
    
    private static ILogger CreateLogger()
    {
        // Configure Serilog to write logs to a file
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose() // Verbose includes Trace and Debug
            .WriteTo.File ("logs/logfile.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Create a logger factory compatible with Microsoft.Extensions.Logging
        using var loggerFactory = LoggerFactory.Create (builder =>
        {
            builder
                .AddSerilog(dispose: true) // Integrate Serilog with ILogger
                .SetMinimumLevel(LogLevel.Trace); // Set minimum log level
        });
        // Get an ILogger instance
        return loggerFactory.CreateLogger ("Global Logger");
    }
}

public class MainWindow : Window
{
    private View _currentView;
    private readonly MenuBar _menu;

    public MainWindow(
        CreateContainerView createContainerView,
        DeleteRecordsView deleteRecordsView,
        FunctionAppView functionAppView)
    {
        Title = "AzBae";
        Width = Dim.Fill();
        Height = Dim.Fill();

        // Create menu once
        _menu = new MenuBar();
        _menu.Menus =
        [
            new MenuBarItem("Cosmos", new []
            {
                CreateMenuItem("Create Container", () => SwitchView(createContainerView)),
                CreateMenuItem("Delete Records", () => SwitchView(deleteRecordsView))
            }),
            new MenuBarItem("Function Apps", new []
            {
                CreateMenuItem("My Function Apps", () =>
                {
                    SwitchView(functionAppView);
                    functionAppView.LoadData();
                })
            }),
            new MenuBarItem("_Quit", "", () => Application.RequestStop())
        ];

        // Initialize all views once
        createContainerView.InitializeComponent();
        deleteRecordsView.InitializeComponent();
        functionAppView.InitializeComponent(); 

        Add(_menu);
    }

    private MenuItem CreateMenuItem(string title, Action action)
    {
        return new MenuItem(title, string.Empty, action);
    }

    private void SwitchView(View view)
    {
        // Remove only the current content view if it exists
        if (_currentView != null)
            Remove(_currentView);
            
        // Position and add the new view
        view.Y = Pos.Bottom(_menu) + 1;
        Add(view);
        _currentView = view;
    }
}

