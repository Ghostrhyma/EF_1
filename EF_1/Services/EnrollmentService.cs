using EF_1.Data;
using EF_1.Models;
using Microsoft.EntityFrameworkCore;

namespace EF_1.Services
{
    public static class EnrollmentService
    {
        // ===== Вспомогательные методы =====

        private static bool TryReadId(string prompt, out int id)
        {
            Console.Write(prompt);

            if (int.TryParse(Console.ReadLine(), out id) && id > 0)
            {
                return true;
            }

            Console.WriteLine("Id должен быть положительным целым числом.");
            return false;
        }

        private static bool Confirm(string message)
        {
            Console.Write($"{message} (y/n): ");
            string answer = Console.ReadLine()?.Trim().ToLower() ?? string.Empty;
            return answer == "y";
        }

        // ===== Куратор (связь один-ко-многим) =====

        public static void AssignTeacherToStudent(AppDbContext context)
        {
            StudentService.ShowAllStudents(context);
            Console.WriteLine();

            if (!TryReadId("Введите Id студента: ", out int studentId))
            {
                return;
            }

            Student? student = context.Students
                .Include(s => s.Teacher)
                .FirstOrDefault(s => s.Id == studentId);

            if (student is null)
            {
                Console.WriteLine("Студент не найден.");
                return;
            }

            Console.WriteLine();
            TeacherService.ShowAllTeachers(context);
            Console.WriteLine();

            if (!TryReadId("Введите Id преподавателя: ", out int teacherId))
            {
                return;
            }

            Teacher? teacher = context.Teachers.Find(teacherId);

            if (teacher is null)
            {
                Console.WriteLine("Преподаватель не найден.");
                return;
            }

            if (student.TeacherId == teacher.Id)
            {
                Console.WriteLine("Этот преподаватель уже является куратором студента.");
                return;
            }

            if (student.Teacher is not null)
            {
                Console.WriteLine($"Текущий куратор: {student.Teacher.FullName}.");
                Console.WriteLine($"Новый куратор: {teacher.FullName}.");

                if (!Confirm("Заменить куратора?"))
                {
                    Console.WriteLine("Операция отменена.");
                    return;
                }
            }

            student.TeacherId = teacher.Id;
            context.SaveChanges();

            Console.WriteLine(
                $"Студенту «{student.FullName}» назначен куратор «{teacher.FullName}».");
        }

        public static void RemoveTeacherFromStudent(AppDbContext context)
        {
            ShowStudentsWithTeachers(context);
            Console.WriteLine();

            if (!TryReadId("Введите Id студента: ", out int studentId))
            {
                return;
            }

            Student? student = context.Students
                .Include(s => s.Teacher)
                .FirstOrDefault(s => s.Id == studentId);

            if (student is null)
            {
                Console.WriteLine("Студент не найден.");
                return;
            }

            if (student.Teacher is null)
            {
                Console.WriteLine("У этого студента нет куратора.");
                return;
            }

            Console.WriteLine($"Куратор студента «{student.FullName}»: {student.Teacher.FullName}.");

            if (!Confirm("Снять куратора?"))
            {
                Console.WriteLine("Операция отменена.");
                return;
            }

            student.TeacherId = null;
            context.SaveChanges();

            Console.WriteLine("Куратор снят. Студент остался в базе.");
        }

        public static void ShowStudentsWithTeachers(AppDbContext context)
        {
            var students = context.Students
                .Include(s => s.Teacher)
                .OrderBy(s => s.FullName)
                .ToList();

            Console.WriteLine("===== Студенты и кураторы =====");

            if (students.Count == 0)
            {
                Console.WriteLine("В базе нет студентов.");
                return;
            }

            foreach (var student in students)
            {
                string teacherName = student.Teacher?.FullName ?? "не назначен";

                Console.WriteLine(
                    $"#{student.Id,-2} | " +
                    $"{student.FullName,-22} | " +
                    $"Куратор: {teacherName}");
            }
        }

        // ===== Курсы (связь many-to-many) =====

        public static void EnrollStudentToCourse(AppDbContext context)
        {
            StudentService.ShowAllStudents(context);
            Console.WriteLine();

            if (!TryReadId("Введите Id студента: ", out int studentId))
            {
                return;
            }

            Student? student = context.Students.Find(studentId);

            if (student is null)
            {
                Console.WriteLine("Студент не найден.");
                return;
            }

            Console.WriteLine();
            CourseService.ShowAllCourses(context);
            Console.WriteLine();

            if (!TryReadId("Введите Id курса: ", out int courseId))
            {
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
            if (!TryReadId("Введите Id студента: ", out int studentId))
            {
                return;
            }

            if (!TryReadId("Введите Id курса: ", out int courseId))
            {
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
                Console.WriteLine("Оценка должна быть целым числом от 2 до 5.");
                return;
            }

            enrollment.Grade = grade;
            enrollment.Version++;

            try
            {
                context.SaveChanges();
                Console.WriteLine("Оценка сохранена.");
            }
            catch (DbUpdateConcurrencyException)
            {
                context.ChangeTracker.Clear();
                Console.WriteLine("Запись уже изменили или удалили. Повторите операцию.");
            }
        }

        public static void CancelEnrollment(AppDbContext context)
        {
            if (!TryReadId("Введите Id студента: ", out int studentId))
            {
                return;
            }

            if (!TryReadId("Введите Id курса: ", out int courseId))
            {
                return;
            }

            Enrollment? enrollment = context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefault(e => e.StudentId == studentId && e.CourseId == courseId);

            if (enrollment is null)
            {
                Console.WriteLine("Такая запись на курс не найдена.");
                return;
            }

            Console.WriteLine("Будет отменена запись:");
            Console.WriteLine($"Студент: {enrollment.Student.FullName}");
            Console.WriteLine($"Курс: {enrollment.Course.Title}");
            Console.WriteLine($"Дата записи: {enrollment.EnrolledAt:d}");

            if (!Confirm("Подтвердить отмену?"))
            {
                Console.WriteLine("Операция отменена.");
                return;
            }

            try
            {
                context.Enrollments.Remove(enrollment);
                context.SaveChanges();

                Console.WriteLine("Запись отменена. Студент и курс остались в базе.");
            }
            catch (DbUpdateConcurrencyException)
            {
                context.ChangeTracker.Clear();
                Console.WriteLine("Запись уже изменили или удалили. Обновите данные и повторите.");
            }
        }

        public static void ShowStudentCourses(AppDbContext context)
        {
            StudentService.ShowAllStudents(context);
            Console.WriteLine();

            if (!TryReadId("Введите Id студента: ", out int studentId))
            {
                return;
            }

            // Include загружает записи, ThenInclude — курс в каждой записи.
            var student = context.Students
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefault(s => s.Id == studentId);

            if (student is null)
            {
                Console.WriteLine("Студент не найден.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"===== Курсы студента: {student.FullName} =====");

            if (student.Enrollments.Count == 0)
            {
                Console.WriteLine("Студент пока не записан ни на один курс.");
                return;
            }

            foreach (var enrollment in student.Enrollments)
            {
                string grade = enrollment.Grade?.ToString() ?? "ещё нет";

                Console.WriteLine(
                    $"- {enrollment.Course.Title}, " +
                    $"{enrollment.Course.DurationHours} ч., " +
                    $"записан: {enrollment.EnrolledAt:d}, " +
                    $"оценка: {grade}");
            }
        }

        public static void ShowCourseStudents(AppDbContext context)
        {
            CourseService.ShowAllCourses(context);
            Console.WriteLine();

            if (!TryReadId("Введите Id курса: ", out int courseId))
            {
                return;
            }

            var course = context.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .FirstOrDefault(c => c.Id == courseId);

            if (course is null)
            {
                Console.WriteLine("Курс не найден.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"===== Студенты курса: {course.Title} =====");

            if (course.Enrollments.Count == 0)
            {
                Console.WriteLine("На курс пока никто не записан.");
                return;
            }

            foreach (var enrollment in course.Enrollments)
            {
                string grade = enrollment.Grade?.ToString() ?? "ещё нет";

                Console.WriteLine(
                    $"- {enrollment.Student.FullName}, " +
                    $"{enrollment.Student.Age} лет, " +
                    $"оценка: {grade}");
            }
        }
    }
}