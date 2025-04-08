using CLI.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CLI.Infrastructure;

public interface IActionProvider
{
    Task<int> ShowNavigationAsync(string title, string? parentName = null);
    T GetAction<T>(string commandName) where T : IAzBaeAction<CommandSettings>;
    IEnumerable<IAzBaeAction<CommandSettings>> GetAvailableActions(string parentName);
}

public class ActionProvider(IEnumerable<IAzBaeAction<CommandSettings>> actions) : IActionProvider
{
    public async Task<int> ShowNavigationAsync(string title, string? parentName = null)
    {
        var availableActions = actions.Where(x =>  x.ParentCommand == (!string.IsNullOrEmpty(parentName) ? parentName : string.Empty)).ToList();
        
        var nextAction = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(title)
                .PageSize(10)
                .MoreChoicesText("What would you like to do?")
                .AddChoices(availableActions.Select(x => x.CommandName))
        );
        
        var action = availableActions.First(x => x.CommandName == nextAction);
        return await action.RunActionAsync();
    }

    public T GetAction<T>(string commandName) where T : IAzBaeAction<CommandSettings>
    {
        var action = actions.Single(x => x.CommandName == commandName);
        action.AvailableActions = GetAvailableActions(action.CommandName);

        foreach(var availableAction in action.AvailableActions)
        {
            availableAction.AvailableActions = GetAvailableActions(availableAction.CommandName);
        }
        
        return (T)action;
    }
    
    public IEnumerable<IAzBaeAction<CommandSettings>> GetAvailableActions(string parentName)
    {
        return actions.Where(x =>  x.ParentCommand.Equals(parentName)).ToList();
    }
}
