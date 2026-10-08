using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using EF_1.Desktop.ViewModels;
using EF_1.Desktop.Views;

namespace EF_1.Desktop.Services;

public class DialogService : IDialogService
{
    private readonly Window _owner;

    public DialogService(Window owner)
    {
        _owner = owner;
    }

    public async Task<bool> ShowStudentEditorAsync(StudentEditViewModel viewModel)
    {
        var window = new StudentEditWindow
        {
            DataContext = viewModel
        };

        // bool?, потому что при закрытии крестиком результата нет.
        bool? result = await window.ShowDialog<bool?>(_owner);

        return result == true;
    }

    public async Task<bool> ConfirmAsync(string title, string message)
    {
        var yesButton = new Button { Content = "Да", MinWidth = 80, IsDefault = true };
        var noButton = new Button { Content = "Нет", MinWidth = 80, IsCancel = true };

        var dialog = new Window
        {
            Title = title,
            Width = 400,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Thickness(16),
                Spacing = 16,
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap
                    },
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Spacing = 8,
                        Children = { yesButton, noButton }
                    }
                }
            }
        };

        yesButton.Click += (_, _) => dialog.Close(true);
        noButton.Click += (_, _) => dialog.Close(false);

        bool? result = await dialog.ShowDialog<bool?>(_owner);

        return result == true;
    }
}