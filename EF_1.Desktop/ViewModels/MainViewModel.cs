using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EF_1.Desktop.Models;
using EF_1.Desktop.Services;
using Microsoft.EntityFrameworkCore;

namespace EF_1.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // Искусственная задержка, чтобы увидеть индикатор и отмену. Потом поставьте 0.
    private const int SimulatedDelayMs = 1200;

    private readonly SchoolDataService _dataService = new();

    public ObservableCollection<StudentRow> Students { get; } = new();

    [ObservableProperty]
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
    [NotifyCanExecuteChangedFor(nameof(DeleteSelectedCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddStudentCommand))]
    private string _newFullName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddStudentCommand))]
    private string _newAge = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddStudentCommand))]
    private string _newEmail = string.Empty;

    private StudentSortOrder CurrentSortOrder => SelectedSortIndex switch
    {
        1 => StudentSortOrder.ByAgeDescending,
        2 => StudentSortOrder.ById,
        _ => StudentSortOrder.ByName
    };

    // Смена сортировки в ComboBox сразу перезагружает таблицу.
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

            // После await мы снова в UI-потоке, поэтому коллекцию менять безопасно.
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

    private bool CanAddStudent() =>
        !IsBusy &&
        !string.IsNullOrWhiteSpace(NewFullName) &&
        !string.IsNullOrWhiteSpace(NewAge) &&
        !string.IsNullOrWhiteSpace(NewEmail);

    [RelayCommand(CanExecute = nameof(CanAddStudent))]
    private async Task AddStudentAsync()
    {
        if (!int.TryParse(NewAge, out int age) || age < 1 || age > 120)
        {
            StatusMessage = "Возраст должен быть числом от 1 до 120.";
            return;
        }

        string email = NewEmail.Trim();

        if (!email.Contains('@'))
        {
            StatusMessage = "Введите корректный email.";
            return;
        }

        IsBusy = true;

        try
        {
            await _dataService.AddStudentAsync(NewFullName.Trim(), age, email);

            NewFullName = string.Empty;
            NewAge = string.Empty;
            NewEmail = string.Empty;

            await LoadStudentsAsync(CancellationToken.None);

            StatusMessage = "Студент добавлен.";
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (DbUpdateException)
        {
            StatusMessage = "Не удалось сохранить: нарушено ограничение БД.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
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