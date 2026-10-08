using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EF_1.Desktop.Models;
using EF_1.Desktop.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EF_1.Desktop.ViewModels;

public partial class StudentEditViewModel : ObservableValidator
{
    private readonly SchoolDataService _dataService;
    private readonly int? _studentId;
    private readonly int? _initialTeacherId;

    public event Action<bool>? CloseRequested;

    public ObservableCollection<TeacherOption> Teachers { get; } = new();

    [ObservableProperty]
    private string _windowTitle = "Новый студент";

    // ===== Поля формы с правилами проверки =====

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Укажите ФИО.")]
    [MinLength(2, ErrorMessage = "ФИО слишком короткое (минимум 2 символа).")]
    [MaxLength(100, ErrorMessage = "ФИО слишком длинное (максимум 100 символов).")]
    private string _fullName = string.Empty;

    // Возраст — строка: так TextBox не ломается на нечисловом вводе,
    // а понятное сообщение мы формируем сами.
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Укажите возраст.")]
    [RegularExpression(@"^\d{1,3}$", ErrorMessage = "Возраст — целое число.")]
    [CustomValidation(typeof(StudentEditViewModel), nameof(ValidateAge))]
    private string _age = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Укажите email.")]
    [EmailAddress(ErrorMessage = "Некорректный адрес email.")]
    [MaxLength(100, ErrorMessage = "Email слишком длинный (максимум 100 символов).")]
    private string _email = string.Empty;

    [ObservableProperty]
    private TeacherOption? _selectedTeacher;

    // ===== Тексты ошибок для вывода под полями =====

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFullNameError))]
    private string _fullNameError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAgeError))]
    private string _ageError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEmailError))]
    private string _emailError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFormError))]
    private string _formError = string.Empty;

    public bool HasFullNameError => !string.IsNullOrEmpty(FullNameError);
    public bool HasAgeError => !string.IsNullOrEmpty(AgeError);
    public bool HasEmailError => !string.IsNullOrEmpty(EmailError);
    public bool HasFormError => !string.IsNullOrEmpty(FormError);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isSaving;

    public StudentEditViewModel(SchoolDataService dataService, StudentRow? row = null)
    {
        _dataService = dataService;

        ErrorsChanged += (_, e) => RefreshErrors(e.PropertyName);

        if (row is not null)
        {
            _studentId = row.Id;
            _initialTeacherId = row.TeacherId;

            WindowTitle = $"Редактирование: {row.FullName}";
            FullName = row.FullName;
            Age = row.Age.ToString();
            Email = row.Email;

            // Для новой формы значения не присваиваем: иначе пустые поля
            // сразу показали бы ошибки, пока пользователь ничего не ввёл.
            ClearErrors();
        }
    }

    // Правило, которое нельзя выразить стандартными атрибутами.
    public static ValidationResult? ValidateAge(string? value, ValidationContext context)
    {
        if (int.TryParse(value, out int age) && (age < 1 || age > 120))
        {
            return new ValidationResult("Возраст должен быть от 1 до 120.");
        }

        return ValidationResult.Success;
    }

    public async Task InitializeAsync()
    {
        var teachers = await _dataService.GetTeachersAsync();

        Teachers.Clear();
        Teachers.Add(new TeacherOption(null, "— без куратора —"));

        foreach (var teacher in teachers)
        {
            Teachers.Add(teacher);
        }

        SelectedTeacher = Teachers.FirstOrDefault(t => t.Id == _initialTeacherId)
                          ?? Teachers[0];
    }

    private void RefreshErrors(string? propertyName)
    {
        switch (propertyName)
        {
            case nameof(FullName):
                FullNameError = FirstError(nameof(FullName));
                break;

            case nameof(Age):
                AgeError = FirstError(nameof(Age));
                break;

            case nameof(Email):
                EmailError = FirstError(nameof(Email));
                break;

            default:
                FullNameError = FirstError(nameof(FullName));
                AgeError = FirstError(nameof(Age));
                EmailError = FirstError(nameof(Email));
                break;
        }
    }

    private string FirstError(string propertyName) =>
        GetErrors(propertyName)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault(message => !string.IsNullOrEmpty(message))
        ?? string.Empty;

    private bool CanSave() => !IsSaving;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        FormError = string.Empty;

        // Проверяем все поля сразу, включая те, которых пользователь ещё не касался.
        ValidateAllProperties();

        if (HasErrors)
        {
            FormError = "Исправьте ошибки в форме.";
            return;
        }

        IsSaving = true;

        try
        {
            await _dataService.SaveStudentAsync(
                _studentId,
                FullName.Trim(),
                int.Parse(Age),
                Email.Trim(),
                SelectedTeacher?.Id);

            CloseRequested?.Invoke(true);
        }
        catch (InvalidOperationException ex)
        {
            // Правила, которые знает только БД (например, занятый email).
            FormError = ex.Message;
        }
        catch (DbUpdateException)
        {
            FormError = "Не удалось сохранить: нарушено ограничение базы данных.";
        }
        catch (Exception ex)
        {
            FormError = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(false);
}