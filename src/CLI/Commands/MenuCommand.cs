using CLI.Actions;
using CLI.Infrastructure;
using CLI.Models;
using Spectre.Console.Cli;

namespace CLI.Commands;

public class MenuCommand : AsyncCommand<MenuCommand.Settings>, IAzBaeCommand
{
    private readonly IActionProvider _actionProvider;

    public string CommandName => "Menu";
    public string ParentCommand => string.Empty;
    
    public class Settings : CommandSettings
    {
    }
    
    public MenuCommand(IActionProvider actionProvider)
    {
        _actionProvider = actionProvider;
    }

    public async override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var action = _actionProvider.GetAction<MenuAction>(CommandName);
        
        return await action.RunActionAsync();
    }
}
