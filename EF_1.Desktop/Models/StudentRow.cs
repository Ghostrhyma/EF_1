namespace EF_1.Desktop.Models;

// Строка таблицы: плоская проекция студента.
public class StudentRow
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public int Age { get; set; }

    public string Email { get; set; } = string.Empty;

    public string TeacherName { get; set; } = string.Empty;

    public int CoursesCount { get; set; }
}