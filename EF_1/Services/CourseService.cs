using EF_1.Data;
using EF_1.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EF_1.Services
{
    public static class CourseService
    {
        public static void AddCourse(AppDbContext context)
        {
            Console.Write("Введите название курса: ");
            string title = Console.ReadLine()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("Название курса не может быть пустым.");
                return;
            }

            bool courseExists = context.Courses
                .Any(c => c.Title == title);

            if (courseExists)
            {
                Console.WriteLine("Курс с таким названием уже существует.");
                return;
            }

            Console.Write("Введите длительность курса в часах: ");
            string hoursInput = Console.ReadLine() ?? string.Empty;

            if (!int.TryParse(hoursInput, out int durationHours) || durationHours <= 0)
            {
                Console.WriteLine("Длительность должна быть положительным целым числом.");
                return;
            }

            var course = new Course
            {
                Title = title,
                DurationHours = durationHours
            };

            context.Courses.Add(course);
            context.SaveChanges();

            Console.WriteLine($"Курс «{course.Title}» добавлен.");
            Console.WriteLine($"Присвоенный Id: {course.Id}");
        }

        public static void ShowAllCourses(AppDbContext context)
        {
            var courses = context.Courses
                .OrderBy(c => c.Title)
                .ToList();

            Console.WriteLine("===== Курсы =====");

            if (courses.Count == 0)
            {
                Console.WriteLine("Курсов пока нет.");
                return;
            }

            foreach (var course in courses)
            {
                Console.WriteLine(
                    $"#{course.Id,-2} | " +
                    $"{course.Title,-30} | " +
                    $"Длительность: {course.DurationHours} ч.");
            }
        }
    }
}
