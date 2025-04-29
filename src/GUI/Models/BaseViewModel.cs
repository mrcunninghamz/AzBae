using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FluentValidation.Results;
using GUI.Models.Cosmos;

namespace GUI.Models;

public abstract class BaseViewModel<T> : ObservableObject, IDisposable where T : Enum
{
    private bool _disposed;
    private string _errors;

    public string Errors
    {
        get => _errors;
        set => SetProperty (ref _errors, value);
    }

    public abstract void Initialize();

    public abstract void Validate();
    
    protected void SendMessage (T actionType, string message = "")
    {
        WeakReferenceMessenger.Default.Send(new Message<T> { Value = actionType });
    }

    // Public implementation of Dispose pattern
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected abstract void DisposeManaged();
    protected abstract void DisposeUnmanaged();

    private  void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            DisposeManaged();
        }

        DisposeUnmanaged();
        
        _disposed = true;
    }

    // Finalizer
    ~BaseViewModel()
    {
        Dispose(false);
    }
}
