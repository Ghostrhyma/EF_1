using EF_1.Data;
using EF_1.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EF_1.Services
{
    public static class EnrollmentService
    {
        public static void AssignTeacherToStudent(AppDbContext context)
        {
            Console.Write("Введите Id студента: ");

            if (!int.TryParse(Console.ReadLine(), out int studentId) || studentId <= 0)
            {
                Console.WriteLine("Id студента должен быть положительным числом.");
                return;
            }

            Student? student = context.Students.Find(studentId);

            if (student is null)
            {
                Console.WriteLine("Студент не найден.");
                return;
            }

            Console.Write("Введите Id преподавателя: ");

            if (!int.TryParse(Console.ReadLine(), out int teacherId) || teacherId <= 0)
            {
                Console.WriteLine("Id преподавателя должен быть положительным числом.");
                return;
            }

            Teacher? teacher = context.Teachers.Find(teacherId);

            if (teacher is null)
            {
                Console.WriteLine("Преподаватель не найден.");
                return;
            }

            student.TeacherId = teacher.Id;

            context.SaveChanges();

            Console.WriteLine(
                $"Студенту «{student.FullName}» назначен куратор «{teacher.FullName}».");
        }

        public static void RemoveTeacherFromStudent(AppDbContext context)
        {
            Console.Write("Введите Id студента: ");

            if (!int.TryParse(Console.ReadLine(), out int studentId) || studentId <= 0)
            {
                Console.WriteLine("Id студента должен быть положительным числом.");
                return;
            }

            Student? student = context.Students.Find(studentId);

            if (student is null)
            {
                Console.WriteLine("Студент не найден.");
                return;
            }

            if (student.TeacherId is null)
            {
                Console.WriteLine("У этого студента уже нет куратора.");
                return;
            }

            student.TeacherId = null;
            context.SaveChanges();

            Console.WriteLine($"Куратор у студента «{student.FullName}» снят.");
        }

        public static void EnrollStudentToCourse(AppDbContext context)
        {
            Console.Write("Введите Id студента: ");

            if (!int.TryParse(Console.ReadLine(), out int studentId) || studentId <= 0)
            {
                Console.WriteLine("Id студента должен быть положительным числом.");
                return;
            }

            Student? student = context.Students.Find(studentId);

            if (student is null)
            {
                Console.WriteLine("Студент не найден.");
                return;
            }

            Console.Write("Введите Id курса: ");

            if (!int.TryParse(Console.ReadLine(), out int courseId) || courseId <= 0)
            {
                Console.WriteLine("Id курса должен быть положительным числом.");
                return;
            }

            Course? course = context.Courses.Find(courseId);

            if (course is null)
            {
                Console.WriteLine("Курс не найден.");
                return;
            }

            bool alreadyEnrolled = context.Enrollments
                .Any(e => e.StudentId == studentId && e.CourseId == courseId);

            if (alreadyEnrolled)
            {
                Console.WriteLine("Студент уже записан на этот курс.");
                return;
            }

            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CourseId = course.Id,
                EnrolledAt = DateTime.UtcNow,
                Grade = null
            };

            context.Enrollments.Add(enrollment);
            context.SaveChanges();

            Console.WriteLine($"Студент «{student.FullName}» записан на курс «{course.Title}».");
        }

        public static void SetGrade(AppDbContext context)
        {
            Console.Write("Введите Id студента: ");

            if (!int.TryParse(Console.ReadLine(), out int studentId) || studentId <= 0)
            {
                Console.WriteLine("Некорректный Id студента.");
                return;
            }

            Console.Write("Введите Id курса: ");

            if (!int.TryParse(Console.ReadLine(), out int courseId) || courseId <= 0)
            {
                Console.WriteLine("Некорректный Id курса.");
                return;
            }

            Enrollment? enrollment = context.Enrollments
                .FirstOrDefault(e => e.StudentId == studentId && e.CourseId == courseId);

            if (enrollment is null)
            {
                Console.WriteLine("Студент не записан на этот курс.");
                return;
            }

            Console.Write("Введите оценку от 2 до 5: ");

            if (!int.TryParse(Console.ReadLine(), out int grade) || grade < 2 || grade > 5)
            {
                Console.WriteLine("Оценка должна быть числом от 2 до 5.");
                return;
            }

            enrollment.Grade = grade;
            context.SaveChanges();

            Console.WriteLine("Оценка сохранена.");
        }

        public static void CancelEnrollment(AppDbContext context)
        {
            Console.Write("Введите Id студента: ");

            if (!int.TryParse(Console.ReadLine(), out int studentId) || studentId <= 0)
            {
                Console.WriteLine("Некорректный Id студента.");
                return;
            }

            Console.Write("Введите Id курса: ");

            if (!int.TryParse(Console.ReadLine(), out int courseId) || courseId <= 0)
            {
                Console.WriteLine("Некорректный Id курса.");
                return;
            }

            Enrollment? enrollment = context.Enrollments
                .FirstOrDefault(e => e.StudentId == studentId && e.CourseId == courseId);

            if (enrollment is null)
            {
                Console.WriteLine("Такая запись на курс не найдена.");
                return;
            }

            context.Enrollments.Remove(enrollment);
            context.SaveChanges();

            Console.WriteLine("Запись студента на курс отменена.");
        }
    }
}
