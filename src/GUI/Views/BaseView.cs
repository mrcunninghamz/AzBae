using System.Diagnostics;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.Messaging;
using GUI.Models.Cosmos;
using Terminal.Gui;

namespace GUI.Views;

public abstract class BaseView<T> : Window, IRecipient<Message<T>> where T : Enum
{
    protected Dialog AzureDialog = new()
    {
        Title = "Working with Azure...",
        ButtonAlignment = MessageBox.DefaultButtonAlignment,
        ButtonAlignmentModes = AlignmentModes.StartToEnd | AlignmentModes.AddSpaceBetweenItems,
        BorderStyle = MessageBox.DefaultBorderStyle,
    };
    
    protected Dialog SettingsDialog = new()
    {
        Title = "Settings...",
        ButtonAlignment = MessageBox.DefaultButtonAlignment,
        ButtonAlignmentModes = AlignmentModes.StartToEnd | AlignmentModes.AddSpaceBetweenItems,
        BorderStyle = MessageBox.DefaultBorderStyle,
    };

    protected Button ViewSettingsDialogButton;

    protected BaseView()
    {
        WeakReferenceMessenger.Default.Register (this);
    }

    public virtual void InitializeComponent()
    {
        if (IsInitialized)
        {
            return;
        }
        
        AzureDialog.Width = Dim.Auto (DimAutoStyle.Auto,
            minimumContentDim: Dim.Func (() => (int)((Application.Screen.Width - AzureDialog.GetAdornmentsThickness ().Horizontal) * (0 / 100f))),
            maximumContentDim: Dim.Func (() => (int)((Application.Screen.Width - AzureDialog.GetAdornmentsThickness ().Horizontal) * 0.9f)));

        AzureDialog.Height = Dim.Auto (DimAutoStyle.Auto,
            minimumContentDim: Dim.Func (() => (int)((Application.Screen.Height - AzureDialog.GetAdornmentsThickness ().Vertical) * (0 / 100f))),
            maximumContentDim: Dim.Func (() => (int)((Application.Screen.Height - AzureDialog.GetAdornmentsThickness ().Vertical) * 0.9f)));
        AzureDialog.ColorScheme = Colors.ColorSchemes ["Dialog"];
    }

    public abstract void Receive(Message<T> message);
    
    protected void SetLabelAndField(Label label, string labelText, TextField textField, Label? topLabel = null)
    {
        label.Text = labelText;
        
        // Position text field adjacent to the label
        textField.X = Pos.Right(label) + 1;
        
        // Fill remaining horizontal space
        textField.Width = Dim.Fill();
        textField.Width = 100;
        
        if (topLabel != null)
        {
            label.Y = Pos.Bottom(topLabel) + 1;
            textField.Y = Pos.Bottom(topLabel) + 1;
        }
        
        
        ViewSettingsDialogButton = new Button
        {
            Text = "View Settings"
        };

        ViewSettingsDialogButton.Accepting += (_, _) =>
        {
            Add(SettingsDialog);
        };
        
        var settingsDialogCloseButton = new Button
        {
            Text = "Close"
        };
        SettingsDialog.AddButton(settingsDialogCloseButton);
                
        settingsDialogCloseButton.Accepting += (_, _) => Remove(SettingsDialog);
        
        Add(ViewSettingsDialogButton);
    }
    
    protected void ShowErrorDialog(string errorMessage)
    {
        MessageBox.ErrorQuery(title: "Error", message: errorMessage, buttons: "OK");
    }
    
    // Add this helper method to your class
    protected void OpenBrowser(string url)
    {
        // Guard against null/empty URLs
        if (string.IsNullOrEmpty(url))
            return;
        
        try
        {
            ProcessStartInfo psi;
        
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                psi = new ProcessStartInfo(url) { UseShellExecute = true };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                psi = new ProcessStartInfo("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                psi = new ProcessStartInfo("open", url);
            }
            else
            {
                throw new PlatformNotSupportedException("Cannot open browser on this platform");
            }
        
            // Set these properties to prevent UI freeze
            psi.CreateNoWindow = true;
            psi.UseShellExecute = true;
        
            // Launch without waiting
            using var process = Process.Start(psi);
        }
        catch (Exception ex)
        {
            // Handle or log any exceptions
            // Console.WriteLine($"Failed to open URL: {ex.Message}");
        }
    }
}
