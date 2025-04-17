using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FluentValidation.Results;
using GUI.Models.Cosmos;

namespace GUI.Models;

public abstract class BaseViewModel<T> : ObservableObject where T : Enum
{
    public string Errors
    {
        get => _errors;
        set => SetProperty (ref _errors, value);
    }

    public abstract void Initialize();

    public abstract void Validate();
    
    private string _errors;
    
    protected void SendMessage (T actionType, string message = "")
    {
        WeakReferenceMessenger.Default.Send(new Message<T> { Value = actionType });
    }
}
