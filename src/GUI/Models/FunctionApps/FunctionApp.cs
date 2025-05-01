using CommunityToolkit.Mvvm.ComponentModel;

namespace GUI.Models.FunctionApps;

public partial class FunctionApp : ObservableObject
{
    [ObservableProperty] 
    private string _id;
    
    [ObservableProperty] 
    private string _name;
    
    [ObservableProperty] 
    private string _resourceGroup;
    
    [ObservableProperty] 
    private string _location;
    
    [ObservableProperty] 
    private string _status;
    
    [ObservableProperty]
    private string _uri;
    
    [ObservableProperty]
    private string _portalUri;
    
    public override string ToString()
    {
        return $"{Name} ({ResourceGroup})";
    }
}
