using EF_1.Data;
using EF_1.Models;
using Microsoft.EntityFrameworkCore;

namespace EF_1.Services
{
    public static class StudentService
    {
        public static void ShowAllStudents(AppDbContext context)
        {
            var students = context.Students
                .OrderBy(s => s.Id)
                .ToList();

            Console.WriteLine("===== Все студенты =====");

            if (students.Count == 0)
            {
                Console.WriteLine("В базе нет студентов.");
                return;
            }

            foreach (var student in students)
            {
                PrintStudent(student);
            }
        }

        public static void ShowAdultStudents(AppDbContext context)
        {
            var adults = context.Students
                .Where(s => s.Age >= 18)
                .OrderBy(s => s.FullName)
                .ToList();

            Console.WriteLine("===== Совершеннолетние студенты =====");

            if (adults.Count == 0)
            {
                Console.WriteLine("Совершеннолетних студентов не найдено.");
                return;
            }

            foreach (var student in adults)
            {
                PrintStudent(student);
            }
        }

        public static void SearchStudentsByName(AppDbContext context)
        {
            Console.Write("Введите часть имени: ");
            string searchText = Console.ReadLine()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                Console.WriteLine("Строка поиска не должна быть пустой.");
                return;
            }

            var students = context.Students
                .Where(s => s.FullName.Contains(searchText))
                .OrderBy(s => s.FullName)
                .ToList();

            Console.WriteLine();
            Console.WriteLine($"===== Результаты поиска: «{searchText}» =====");

            if (students.Count == 0)
            {
                Console.WriteLine("Совпадений не найдено.");
                return;
            }

            foreach (var student in students)
            {
                PrintStudent(student);
            }
        }

        public static void ShowStudentsSortedByAgeAndName(AppDbContext context)
        {
            var students = context.Students
                .OrderByDescending(s => s.Age)
                .ThenBy(s => s.FullName)
                .ToList();

            Console.WriteLine("===== Возраст по убыванию, имя по возрастанию =====");

            foreach (var student in students)
            {
                Console.WriteLine(
                    $"Возраст: {student.Age,-2} | " +
                    $"Имя: {student.FullName,-22} | " +
                    $"Email: {student.Email}");
            }
        }

        public static void ShowStudentCards(AppDbContext context)
        {
            var cards = context.Students
                .Where(s => s.Age >= 18)
                .OrderBy(s => s.FullName)
                .Select(s => new
                {
                    StudentId = s.Id,
                    Name = s.FullName,
                    Contact = s.Email
                })
                .ToList();

            Console.WriteLine("===== Карточки совершеннолетних студентов =====");

            if (cards.Count == 0)
            {
                Console.WriteLine("Совершеннолетних студентов не найдено.");
                return;
            }

            foreach (var card in cards)
            {
                Console.WriteLine($"#{card.StudentId} | {card.Name} | {card.Contact}");
            }
        }

        public static void ShowStudentsPage(AppDbContext context)
        {
            const int pageSize = 3;

            int totalStudents = context.Students.Count();

            if (totalStudents == 0)
            {
                Console.WriteLine("В базе нет студентов.");
                return;
            }

            int pageCount = (int)Math.Ceiling(totalStudents / (double)pageSize);

            Console.Write($"Введите номер страницы от 1 до {pageCount}: ");
            string input = Console.ReadLine() ?? string.Empty;

            if (!int.TryParse(input, out int page) || page < 1 || page > pageCount)
            {
                Console.WriteLine("Номер страницы введён неверно.");
                return;
            }

            // Перед Skip и Take всегда нужна сортировка.
            var students = context.Students
                .OrderBy(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    s.Id,
                    s.FullName,
                    s.Age,
                    s.Email
                })
                .ToList();

            Console.WriteLine();
            Console.WriteLine($"===== Страница {page} из {pageCount} =====");

            foreach (var student in students)
            {
                Console.WriteLine(
                    $"#{student.Id,-2} | " +
                    $"{student.FullName,-22} | " +
                    $"Возраст: {student.Age,-2} | " +
                    $"{student.Email}");
            }
        }

        public static void ShowStatistics(AppDbContext context)
        {
            int totalCount = context.Students.Count();

            if (totalCount == 0)
            {
                Console.WriteLine("Статистика недоступна: студентов нет.");
                return;
            }

            int adultCount = context.Students.Count(s => s.Age >= 18);
            double averageAge = context.Students.Average(s => s.Age);
            int minAge = context.Students.Min(s => s.Age);
            int maxAge = context.Students.Max(s => s.Age);
            int gmailCount = context.Students.Count(s => s.Email.EndsWith("@gmail.com"));

            Console.WriteLine("===== Статистика =====");
            Console.WriteLine($"Всего студентов: {totalCount}");
            Console.WriteLine($"Совершеннолетних: {adultCount}");
            Console.WriteLine($"Средний возраст: {averageAge:F1}");
            Console.WriteLine($"Минимальный возраст: {minAge}");
            Console.WriteLine($"Максимальный возраст: {maxAge}");
            Console.WriteLine($"Студентов с Gmail: {gmailCount}");
        }

        public static void AddStudent(AppDbContext context)
        {
            Console.Write("Введите ФИО: ");
            string fullName = Console.ReadLine()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(fullName))
            {
                Console.WriteLine("ФИО не может быть пустым.");
                return;
            }

            Console.Write("Введите возраст: ");
            string ageInput = Console.ReadLine() ?? string.Empty;

            if (!int.TryParse(ageInput, out int age) || age < 1 || age > 120)
            {
                Console.WriteLine("Возраст должен быть числом от 1 до 120.");
                return;
            }

            Console.Write("Введите email: ");
            string email = Console.ReadLine()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                Console.WriteLine("Введите корректный email.");
                return;
            }

            bool emailExists = context.Students
                .Any(s => s.Email == email);

            if (emailExists)
            {
                Console.WriteLine("Студент с таким email уже существует.");
                return;
            }

            var student = new Student
            {
                FullName = fullName,
                Age = age,
                Email = email
            };

            try
            {
                context.Students.Add(student);
                context.SaveChanges();

                Console.WriteLine($"Студент «{student.FullName}» добавлен.");
                Console.WriteLine($"Присвоенный Id: {student.Id}");
            }
            catch (DbUpdateException)
            {
                // Неудачный объект остался бы в контексте и ломал бы следующие SaveChanges().
                context.Entry(student).State = EntityState.Detached;

                Console.WriteLine("Не удалось сохранить: нарушено ограничение БД (например, email уже занят).");
            }
        }

        public static void PrintStudent(Student student)
        {
            Console.WriteLine(
                $"#{student.Id,-2} | " +
                $"{student.FullName,-22} | " +
                $"Возраст: {student.Age,-2} | " +
                $"{student.Email}");
        }
    }
}