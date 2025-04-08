using CLI.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CLI.Actions;

public abstract class AzBaeAction
{
    public virtual string CommandName { get; }
    public virtual string ParentCommand { get; }
}

public abstract class AzBaeAction<T> : AzBaeAction, IAzBaeAction<T> where T : CommandSettings
{
    public IEnumerable<IAzBaeAction<CommandSettings>>? AvailableActions { get; set; }
    
    public T Settings { get; set; }
    
    public async  Task<int> RunActionAsync()
    {
        if (AvailableActions != null && AvailableActions.Any())
        {
            
            var nextAction = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(CommandName)
                    .PageSize(10)
                    .MoreChoicesText("What would you like to do?")
                    .AddChoices(AvailableActions.Select(x => x.CommandName))
            );
        
            var action = AvailableActions.First(x => x.CommandName == nextAction);
            return await action.RunActionAsync();
        }
        return await ExecuteAsync();
    }
    
    public abstract Task<int> ExecuteAsync();
}
