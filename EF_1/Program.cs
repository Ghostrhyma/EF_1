using EF_1.Data;
using EF_1.Services;

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
        case "1": StudentService.ShowAllStudents(context); break;
        case "2": StudentService.AddStudent(context); break;
        case "3": TeacherService.ShowAllTeachers(context); break;
        case "4": TeacherService.AddTeacher(context); break;
        case "5": CourseService.ShowAllCourses(context); break;
        case "6": CourseService.AddCourse(context); break;
        case "7": EnrollmentService.AssignTeacherToStudent(context); break;
        case "8": EnrollmentService.EnrollStudentToCourse(context); break;
        case "9": EnrollmentService.SetGrade(context); break;
        case "10": EnrollmentService.CancelEnrollment(context); break;
        case "11": EnrollmentService.ShowStudentsWithTeachers(context); break;
        case "12": EnrollmentService.ShowStudentCourses(context); break;
        case "13": EnrollmentService.ShowCourseStudents(context); break;
        case "14": EnrollmentService.RemoveTeacherFromStudent(context); break;
        case "15": StudentService.ShowAdultStudents(context); break;
        case "16": StudentService.SearchStudentsByName(context); break;
        case "17": StudentService.ShowStudentsSortedByAgeAndName(context); break;
        case "18": StudentService.ShowStudentCards(context); break;
        case "19": StudentService.ShowStudentsPage(context); break;
        case "20": StudentService.ShowStatistics(context); break;
        case "21": ReliabilityService.ConcurrencyDemo(context); break;
        case "22": ReliabilityService.TransferStudent(context); break;
        case "23": ReliabilityService.ShowStudentsOlderThanRawSql(context); break;
        case "24": ReliabilityService.ShowCourseReportRawSql(context); break;
        case "25": ReliabilityService.ClearTeacherFromAllStudentsRawSql(context); break;
        case "26": ReliabilityService.ConstraintDemo(context); break;
        case "27": ReliabilityService.ShowQueryPlans(context); break;

        case "0":
            Console.WriteLine("Программа завершена.");
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
    Console.WriteLine("========== Учёт студентов ==========");
    Console.WriteLine("--- Студенты ---");
    Console.WriteLine(" 1. Показать студентов");
    Console.WriteLine(" 2. Добавить студента");
    Console.WriteLine("--- Преподаватели ---");
    Console.WriteLine(" 3. Показать преподавателей");
    Console.WriteLine(" 4. Добавить преподавателя");
    Console.WriteLine("--- Курсы ---");
    Console.WriteLine(" 5. Показать курсы");
    Console.WriteLine(" 6. Добавить курс");
    Console.WriteLine("--- Связи ---");
    Console.WriteLine(" 7. Назначить куратора студенту");
    Console.WriteLine(" 8. Записать студента на курс");
    Console.WriteLine(" 9. Поставить оценку");
    Console.WriteLine("10. Отменить запись на курс");
    Console.WriteLine("11. Студенты с кураторами");
    Console.WriteLine("12. Курсы студента");
    Console.WriteLine("13. Студенты курса");
    Console.WriteLine("14. Снять куратора со студента");
    Console.WriteLine("--- LINQ (урок 10) ---");
    Console.WriteLine("15. Совершеннолетние студенты");
    Console.WriteLine("16. Поиск студентов по имени");
    Console.WriteLine("17. Сортировка по возрасту и имени");
    Console.WriteLine("18. Карточки студентов");
    Console.WriteLine("19. Студенты постранично");
    Console.WriteLine("20. Статистика");
    Console.WriteLine("--- Надёжность (урок 14) ---");
    Console.WriteLine("21. Демо конфликта конкуренции");
    Console.WriteLine("22. Перевести студента на другой курс (транзакция)");
    Console.WriteLine("23. Студенты от N лет (Raw SQL)");
    Console.WriteLine("24. Отчёт по курсам (Raw SQL)");
    Console.WriteLine("25. Снять преподавателя у всех студентов (Raw SQL)");
    Console.WriteLine("26. Демо ограничений БД");
    Console.WriteLine("27. План выполнения запросов (индексы)");
    Console.WriteLine(" 0. Выход");
    Console.WriteLine("====================================");
}