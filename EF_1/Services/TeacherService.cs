using EF_1.Data;
using EF_1.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EF_1.Services
{
    public static class TeacherService
    {
        public static void AddTeacher(AppDbContext context)
        {
            Console.Write("Введите ФИО преподавателя: ");
            string fullName = Console.ReadLine()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(fullName))
            {
                Console.WriteLine("ФИО преподавателя не может быть пустым.");
                return;
            }

            Console.Write("Введите email преподавателя: ");
            string email = Console.ReadLine()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                Console.WriteLine("Введите корректный email.");
                return;
            }

            bool emailExists = context.Teachers
                .Any(t => t.Email == email);

            if (emailExists)
            {
                Console.WriteLine("Преподаватель с таким email уже существует.");
                return;
            }

            var teacher = new Teacher
            {
                FullName = fullName,
                Email = email
            };

            context.Teachers.Add(teacher);
            context.SaveChanges();

            Console.WriteLine($"Преподаватель «{teacher.FullName}» добавлен.");
            Console.WriteLine($"Присвоенный Id: {teacher.Id}");
        }

        public static void ShowAllTeachers(AppDbContext context)
        {
            var teachers = context.Teachers
                .OrderBy(t => t.FullName)
                .ToList();

            Console.WriteLine("===== Преподаватели =====");

            if (teachers.Count == 0)
            {
                Console.WriteLine("Преподавателей пока нет.");
                return;
            }

            foreach (var teacher in teachers)
            {
                Console.WriteLine(
                    $"#{teacher.Id,-2} | " +
                    $"{teacher.FullName,-25} | " +
                    $"{teacher.Email}");
            }
        }
    }

}
