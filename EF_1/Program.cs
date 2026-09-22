using EF_1.Data;
using EF_1.Models;
using EF_1.Services;
using Microsoft.EntityFrameworkCore;

using var context = new AppDbContext();

context.Database.EnsureCreated();

SeedService.SeedStudents(context);
SeedService.SeedTeachers(context);
SeedService.SeedCourses(context);

while (true)
{
    ShowMenu();

    Console.Write("Выберите пункт: ");
    string command = Console.ReadLine() ?? string.Empty;

    Console.Clear();

    switch (command)
    {
        case "1":
            StudentService.ShowAllStudents(context);
            break;

        case "2":
            StudentService.AddStudent(context);
            break;

        case "3":
            TeacherService.ShowAllTeachers(context);
            break;

        case "4":
            TeacherService.AddTeacher(context);
            break;

        case "5":
            CourseService.ShowAllCourses(context);
            break;

        case "6":
            CourseService.AddCourse(context);
            break;

        case "7":
            EnrollmentService.AssignTeacherToStudent(context);
            break;

        case "8":
            EnrollmentService.EnrollStudentToCourse(context);
            break;

        case "9":
            EnrollmentService.SetGrade(context);
            break;

        case "10":
            EnrollmentService.CancelEnrollment(context);
            break;

        case "0":
            return;

        default:
            Console.WriteLine("Такого пункта нет.");
            break;
    }

    Console.WriteLine("\nНажмите любую клавишу...");
    Console.ReadKey();
    Console.Clear();
}

static void ShowMenu()
{
    Console.WriteLine("===== Учёт студентов =====");
    Console.WriteLine("1. Показать студентов");
    Console.WriteLine("2. Добавить студента");
    Console.WriteLine("3. Показать преподавателей");
    Console.WriteLine("4. Добавить преподавателя");
    Console.WriteLine("5. Показать курсы");
    Console.WriteLine("6. Добавить курс");
    Console.WriteLine("7. Назначить куратора студенту");
    Console.WriteLine("8. Записать студента на курс");
    Console.WriteLine("9. Поставить оценку");
    Console.WriteLine("10. Отменить запись на курс");
    Console.WriteLine("0. Выход");
}