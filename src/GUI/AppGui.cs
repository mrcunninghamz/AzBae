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
    public MainWindow(CreateContainerView createContainerView)
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
                        createContainerView.MenuBar = menu;
                        createContainerView.InitializeComponent();
                        Application.Run(createContainerView);
                    })
            }),
            new MenuBarItem("_Quit", "", () => { Application.RequestStop(); })
        ];
        
        Add(menu);

    }
}

