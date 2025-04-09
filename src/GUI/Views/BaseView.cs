using CommunityToolkit.Mvvm.Messaging;
using GUI.Models.Cosmos;
using Terminal.Gui;

namespace GUI.Views;

public abstract class BaseView<T> : Window, IRecipient<Message<T>> where T : Enum
{
    protected Dialog ErrorDialog = new Dialog();
    protected Label ErrorDialogLabel = new Label();
    
    public virtual void Receive(Message<T> message)
    {
        throw new NotImplementedException();
    }

    public virtual void InitializeComponent()
    {
        ErrorDialog.Title = "Error:";
        ErrorDialog.ButtonAlignment = Alignment.Center;
        ErrorDialogLabel.Text = "Something went wrong.";
        ErrorDialog.Add(ErrorDialogLabel);
        var button = new Button
        {
            Text = "Close",
            Y = Pos.Bottom(ErrorDialogLabel) + 1
        };
        ErrorDialog.AddButton(button);
                
        button.Accepting += (_, _) => Remove(ErrorDialog);
    }


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

        Add(label, textField);
    }
}
