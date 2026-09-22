using EF_1.Data;
using EF_1.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EF_1.Services
{
    public class StudentService
    {
        public static void ShowAllStudents(AppDbContext context)
        {
            // OrderBy — сортировка по Id.
            // ToList — выполнение SQL-запроса и получение списка объектов.
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
            // Where — фильтрация.
            // В БД попадёт условие WHERE Age >= 18.
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

            // Contains — поиск фрагмента текста.
            // Пример: ввод «ан» найдёт Анну, Жанну и т.д.
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
            // Сначала сортировка по возрасту от большего к меньшему.
            // Затем при одинаковом возрасте — по имени по алфавиту.
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
            // Select — проекция.
            // Из таблицы выбираем не весь Student, а только нужные поля.
            var cards = context.Students
                .Where(s => s.Age >= 18)
                .OrderBy(s => s.FullName)
                .Select(s => new
                {
                    StudentId = s.Id,
                    Name = s.FullName,
                    Contact = s.Email,
                    IsAdult = s.Age >= 18
                })
                .ToList();

            Console.WriteLine("===== Карточки совершеннолетних студентов =====");

            foreach (var card in cards)
            {
                string adultStatus = card.IsAdult ? "Да" : "Нет";

                Console.WriteLine(
                    $"#{card.StudentId} | " +
                    $"{card.Name} | " +
                    $"{card.Contact} | " +
                    $"Совершеннолетний: {adultStatus}");
            }
        }

        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public static void ShowStudentsPage(AppDbContext context)
        {
            const int pageSize = 3;

            // Count выполняет SQL COUNT(*) и возвращает общее число студентов.
            int totalStudents = context.Students.Count();

            if (totalStudents == 0)
            {
                Console.WriteLine("В базе нет студентов.");
                return;
            }

            // Например, 10 студентов / 3 на страницу = 4 страницы.
            int pageCount = (int)Math.Ceiling(totalStudents / (double)pageSize);

            Console.Write($"Введите номер страницы от 1 до {pageCount}: ");
            string input = Console.ReadLine() ?? string.Empty;

            bool isCorrectPage = int.TryParse(input, out int page);

            if (!isCorrectPage || page < 1 || page > pageCount)
            {
                Console.WriteLine("Номер страницы введён неверно.");
                return;
            }

            // Важно: перед Skip и Take всегда нужна сортировка.
            // Иначе порядок строк, возвращаемых БД, может быть нестабильным.
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



        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        public static void ShowStatistics(AppDbContext context)
        {
            int totalCount = context.Students.Count();

            if (totalCount == 0)
            {
                Console.WriteLine("Статистика недоступна: студентов нет.");
                return;
            }

            int adultCount = context.Students.Count(s => s.Age >= 18);

            double averageAge = context.Students
                .Average(s => s.Age);

            int minAge = context.Students
                .Min(s => s.Age);

            int maxAge = context.Students
                .Max(s => s.Age);

            int gmailCount = context.Students
                .Count(s => s.Email.EndsWith("@gmail.com"));

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

            bool isCorrectAge = int.TryParse(ageInput, out int age);

            if (!isCorrectAge || age < 1 || age > 120)
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

            // Any проверяет, есть ли студент с таким email.
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

            // Add — добавляет объект в отслеживание EF Core.
            // SaveChanges — выполняет INSERT в базе данных.
            context.Students.Add(student);
            context.SaveChanges();

            Console.WriteLine($"Студент «{student.FullName}» добавлен.");
            Console.WriteLine($"Присвоенный Id: {student.Id}");
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

//test for github