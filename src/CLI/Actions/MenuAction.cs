using CLI.Commands;

namespace CLI.Actions;

public class MenuAction : AzBaeAction<MenuCommand.Settings>
{
    public override string CommandName => "Menu";
    public override string ParentCommand => string.Empty;
    public override Task<int> ExecuteAsync()
    {
        return Task.FromResult(0);
    }
}
