using System.Threading.Tasks;
using EF_1.Desktop.ViewModels;

namespace EF_1.Desktop.Services;

public interface IDialogService
{
    // true — студент сохранён, false — окно закрыто без сохранения.
    Task<bool> ShowStudentEditorAsync(StudentEditViewModel viewModel);

    Task<bool> ConfirmAsync(string title, string message);
}