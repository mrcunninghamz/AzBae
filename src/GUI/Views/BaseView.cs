using CommunityToolkit.Mvvm.Messaging;
using GUI.Models.Cosmos;
using Terminal.Gui;

namespace GUI.Views;

public abstract class BaseView<T> : Window, IRecipient<Message<T>> where T : Enum
{
    
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
}
