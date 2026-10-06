using EF_1.Data;
using EF_1.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EF_1.Services
{
    public static class SeedService
    {
        public static void SeedStudents(AppDbContext context)
        {
            if (context.Students.Any())
            {
                return;
            }

            context.Students.AddRange(
                new Student
                {
                    FullName = "Анна Иванова",
                    Age = 19,
                    Email = "anna.ivanova@mail.ru"
                },
                new Student
                {
                    FullName = "Борис Петров",
                    Age = 17,
                    Email = "boris.petrov@mail.ru"
                },
                new Student
                {
                    FullName = "Вера Сидорова",
                    Age = 21,
                    Email = "vera.sidorova@gmail.com"
                }
            );

            context.SaveChanges();
        }

        public static void SeedTeachers(AppDbContext context)
        {
            if (context.Teachers.Any())
            {
                return;
            }

            context.Teachers.AddRange(
                new Teacher
                {
                    FullName = "Ирина Смирнова",
                    Email = "smirnova@college.ru"
                },
                new Teacher
                {
                    FullName = "Олег Васильев",
                    Email = "vasiliev@college.ru"
                }
            );

            context.SaveChanges();
        }

        public static void SeedCourses(AppDbContext context)
        {
            if (context.Courses.Any())
            {
                return;
            }

            context.Courses.AddRange(
                new Course
                {
                    Title = "Основы C#",
                    DurationHours = 48
                },
                new Course
                {
                    Title = "Entity Framework Core",
                    DurationHours = 24
                }
            );

            context.SaveChanges();
        }
    }
}
