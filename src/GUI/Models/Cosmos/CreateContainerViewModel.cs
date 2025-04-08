using CommunityToolkit.Mvvm.ComponentModel;

namespace GUI.Models.Cosmos;

public partial class CreateContainerViewModel : ObservableObject
{
    [ObservableProperty]
    private string _accountEndpoint;
}
