using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EF_1.Desktop.Models;
using EF_1.Desktop.Services;

namespace EF_1.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // Искусственная задержка для демонстрации. Для реальной работы поставьте 0.
    private const int SimulatedDelayMs = 600;

    private readonly SchoolDataService _dataService = new();
    private readonly IDialogService _dialogs;

    public MainViewModel(IDialogService dialogs)
    {
        _dialogs = dialogs;
    }

    public ObservableCollection<StudentRow> Students { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteSelectedCommand))]
    private StudentRow? _selectedStudent;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _selectedSortIndex;

    [ObservableProperty]
    private string _statusMessage = "Готово.";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddStudentCommand))]
    [NotifyCanExecuteChangedFor(nameof(EditSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteSelectedCommand))]
    private bool _isBusy;

    private StudentSortOrder CurrentSortOrder => SelectedSortIndex switch
    {
        1 => StudentSortOrder.ByAgeDescending,
        2 => StudentSortOrder.ById,
        _ => StudentSortOrder.ByName
    };

    partial void OnSelectedSortIndexChanged(int value)
    {
        if (LoadStudentsCommand.CanExecute(null))
        {
            LoadStudentsCommand.Execute(null);
        }
    }

    // ===== Загрузка =====

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task LoadStudentsAsync(CancellationToken token)
    {
        IsBusy = true;
        StatusMessage = "Загрузка...";

        try
        {
            await _dataService.InitializeAsync(token);

            if (SimulatedDelayMs > 0)
            {
                await Task.Delay(SimulatedDelayMs, token);
            }

            var rows = await _dataService.GetStudentsAsync(SearchText, CurrentSortOrder, token);

            Students.Clear();

            foreach (var row in rows)
            {
                Students.Add(row);
            }

            StatusMessage = $"Загружено студентов: {rows.Count}.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Загрузка отменена.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ===== Добавление =====

    private bool CanAddStudent() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanAddStudent))]
    private async Task AddStudentAsync()
    {
        try
        {
            var editor = new StudentEditViewModel(_dataService);
            await editor.InitializeAsync();

            bool saved = await _dialogs.ShowStudentEditorAsync(editor);

            if (saved)
            {
                await LoadStudentsAsync(CancellationToken.None);
                StatusMessage = "Студент добавлен.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    // ===== Редактирование =====

    private bool CanEditSelected() => SelectedStudent is not null && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanEditSelected))]
    private async Task EditSelectedAsync()
    {
        var row = SelectedStudent;

        if (row is null)
        {
            return;
        }

        try
        {
            var editor = new StudentEditViewModel(_dataService, row);
            await editor.InitializeAsync();

            bool saved = await _dialogs.ShowStudentEditorAsync(editor);

            if (saved)
            {
                await LoadStudentsAsync(CancellationToken.None);
                StatusMessage = "Изменения сохранены.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    // ===== Удаление =====

    private bool CanDeleteSelected() => SelectedStudent is not null && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private async Task DeleteSelectedAsync()
    {
        var student = SelectedStudent;

        if (student is null)
        {
            return;
        }

        bool confirmed = await _dialogs.ConfirmAsync(
            "Удаление студента",
            $"Удалить студента «{student.FullName}»? Это действие нельзя отменить.");

        if (!confirmed)
        {
            return;
        }

        IsBusy = true;

        try
        {
            bool deleted = await _dataService.DeleteStudentAsync(student.Id);

            if (deleted)
            {
                Students.Remove(student);
                StatusMessage = $"Студент «{student.FullName}» удалён.";
            }
            else
            {
                StatusMessage = "Студент уже был удалён.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка удаления: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}