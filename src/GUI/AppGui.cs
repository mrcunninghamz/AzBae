using GUI.Views.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Terminal.Gui;

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
        Application.Init();
        // Override the default configuration for the application to use the Light theme
        ConfigurationManager.RuntimeConfig = """{ "Theme": "Light" }""";
        Application.Run(_mainWindow);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("StopAsync");

        return Task.CompletedTask;
    }
}

public class MainWindow : Window
{
    public MainWindow(
        CreateContainerView createContainerView,
        DeleteRecordsView deleteRecordsView)
    {
        Title = "AzBae";
        
        var menu = new MenuBar();
        menu.Menus =
        [
            new MenuBarItem("Cosmos", new []
            {
                new MenuItem("Create Container", string.Empty,
                    () =>
                    {
                        RemoveAll();
                        createContainerView.InitializeComponent();
                        Add(menu);
                        createContainerView.Y = Pos.Bottom(menu) + 1;
                        Add(createContainerView);
                    }),
                new MenuItem("Delete Records", string.Empty,
                    () =>
                    {
                        RemoveAll();
                        deleteRecordsView.InitializeComponent();
                        Add(menu);
                        deleteRecordsView.Y = Pos.Bottom(menu) + 1;
                        Add(deleteRecordsView);
                    })
            }),
            new MenuBarItem("_Quit", "", () => { Application.RequestStop(); })
        ];
        
        Add(menu);

    }
}

