using Avalonia.Controls;
using EF_1.Desktop.ViewModels;

namespace EF_1.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Первая асинхронная загрузка запускается после показа окна.
        Loaded += async (_, _) =>
        {
            if (DataContext is MainViewModel viewModel)
            {
                await viewModel.LoadStudentsCommand.ExecuteAsync(null);
            }
        };
    }
}