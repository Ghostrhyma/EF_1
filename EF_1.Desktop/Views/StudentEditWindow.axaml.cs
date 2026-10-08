using Avalonia.Controls;
using EF_1.Desktop.ViewModels;

namespace EF_1.Desktop.Views;

public partial class StudentEditWindow : Window
{
    public StudentEditWindow()
    {
        InitializeComponent();

        // ViewModel просит закрыть окно и передаёт результат.
        DataContextChanged += (_, _) =>
        {
            if (DataContext is StudentEditViewModel viewModel)
            {
                viewModel.CloseRequested += result => Close(result);
            }
        };
    }
}