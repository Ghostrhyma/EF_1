using EF_1.Data;
using EF_1.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EF_1.Services
{
    public static class ReliabilityService
    {
        private static bool TryReadInt(string prompt, out int value)
        {
            Console.Write(prompt);

            if (int.TryParse(Console.ReadLine(), out value) && value > 0)
            {
                return true;
            }

            Console.WriteLine("Нужно ввести положительное целое число.");
            return false;
        }

        // ===== 1. Конкуренция: два контекста меняют одну запись =====

        public static void ConcurrencyDemo(AppDbContext context)
        {
            var first = context.Enrollments.AsNoTracking().FirstOrDefault();

            if (first is null)
            {
                Console.WriteLine("Нет записей на курсы. Сначала запишите студента на курс (пункт 8).");
                return;
            }

            int studentId = first.StudentId;
            int courseId = first.CourseId;

            // Два независимых контекста — два «пользователя».
            using var contextA = new AppDbContext();
            using var contextB = new AppDbContext();

            var enrollA = contextA.Enrollments
                .Single(e => e.StudentId == studentId && e.CourseId == courseId);

            var enrollB = contextB.Enrollments
                .Single(e => e.StudentId == studentId && e.CourseId == courseId);

            Console.WriteLine($"Оба пользователя прочитали запись. Version = {enrollA.Version}.");

            enrollA.Grade = 4;
            enrollA.Version++;
            contextA.SaveChanges();
            Console.WriteLine("Пользователь A сохранил оценку 4.");

            enrollB.Grade = 5;
            enrollB.Version++;

            try
            {
                contextB.SaveChanges();
                Console.WriteLine("Пользователь B сохранил оценку 5 (конфликта не было).");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("Пользователь B: КОНФЛИКТ — запись уже изменили.");

                var entry = ex.Entries.Single();
                var dbValues = entry.GetDatabaseValues();

                if (dbValues is null)
                {
                    Console.WriteLine("Запись была удалена другим пользователем.");
                    return;
                }

                int dbVersion = dbValues.GetValue<int>(nameof(Enrollment.Version));
                int? dbGrade = dbValues.GetValue<int?>(nameof(Enrollment.Grade));

                Console.WriteLine($"В базе сейчас: оценка {dbGrade}, Version {dbVersion}.");

                // Стратегия «клиент побеждает»: обновляем исходные значения и повторяем.
                entry.OriginalValues.SetValues(dbValues);
                enrollB.Version = dbVersion + 1;

                contextB.SaveChanges();
                Console.WriteLine("B повторил сохранение: итоговая оценка 5.");
            }
        }

        // ===== 2. Транзакция: перевод студента на другой курс =====

        public static void TransferStudent(AppDbContext context)
        {
            if (!TryReadInt("Id студента: ", out int studentId)) return;
            if (!TryReadInt("Id курса, с которого переводим: ", out int fromCourseId)) return;
            if (!TryReadInt("Id курса, на который переводим: ", out int toCourseId)) return;

            using var transaction = context.Database.BeginTransaction();

            try
            {
                var oldEnrollment = context.Enrollments
                    .FirstOrDefault(e => e.StudentId == studentId && e.CourseId == fromCourseId);

                if (oldEnrollment is null)
                {
                    Console.WriteLine("Исходная запись не найдена.");
                    return; // transaction.Dispose() откатит изменения
                }

                context.Enrollments.Remove(oldEnrollment);
                context.SaveChanges();

                // Целевой курс намеренно не проверяем: введите несуществующий Id,
                // и вторая операция упадёт, а первая откатится.
                context.Enrollments.Add(new Enrollment
                {
                    StudentId = studentId,
                    CourseId = toCourseId,
                    EnrolledAt = DateTime.UtcNow
                });
                context.SaveChanges();

                transaction.Commit();
                Console.WriteLine("Перевод выполнен.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                context.ChangeTracker.Clear();

                Console.WriteLine("Перевод отменён, изменения откатены.");
                Console.WriteLine($"Причина: {ex.GetBaseException().Message}");
            }
        }

        // ===== 3. Raw SQL =====

        // FromSql возвращает сущности. Значения в {} превращаются в параметры.
        public static void ShowStudentsOlderThanRawSql(AppDbContext context)
        {
            if (!TryReadInt("Минимальный возраст: ", out int minAge)) return;

            var students = context.Students
                .FromSql($"SELECT * FROM Students WHERE Age >= {minAge}")
                .OrderBy(s => s.FullName)
                .ToList();

            Console.WriteLine($"===== Студенты от {minAge} лет (Raw SQL) =====");

            if (students.Count == 0)
            {
                Console.WriteLine("Никого не найдено.");
                return;
            }

            foreach (var student in students)
            {
                StudentService.PrintStudent(student);
            }
        }

        // SqlQuery возвращает не-сущности, здесь — DTO CourseStat.
        public static void ShowCourseReportRawSql(AppDbContext context)
        {
            var report = context.Database
                .SqlQuery<CourseStat>($@"
                    SELECT c.Title AS Title,
                           COUNT(e.StudentId) AS StudentsCount,
                           AVG(e.Grade) AS AverageGrade
                    FROM Courses c
                    LEFT JOIN Enrollments e ON e.CourseId = c.Id
                    GROUP BY c.Id, c.Title
                    ORDER BY c.Title")
                .ToList();

            Console.WriteLine("===== Отчёт по курсам (Raw SQL) =====");

            foreach (var item in report)
            {
                string average = item.AverageGrade?.ToString("F1") ?? "нет оценок";
                Console.WriteLine($"{item.Title,-25} | студентов: {item.StudentsCount} | средняя оценка: {average}");
            }
        }

        // ExecuteSql выполняет команду и возвращает число изменённых строк.
        public static void ClearTeacherFromAllStudentsRawSql(AppDbContext context)
        {
            TeacherService.ShowAllTeachers(context);
            Console.WriteLine();

            if (!TryReadInt("Id преподавателя: ", out int teacherId)) return;

            int affected = context.Database.ExecuteSql(
                $"UPDATE Students SET TeacherId = NULL WHERE TeacherId = {teacherId}");

            // Контекст не знает об изменениях, сделанных мимо него.
            context.ChangeTracker.Clear();

            Console.WriteLine($"Куратор снят у студентов: {affected}.");
        }

        // ===== 4. Ограничения БД =====

        public static void ConstraintDemo(AppDbContext context)
        {
            Console.WriteLine("Попытка 1: студент с возрастом 200 (нарушает CHECK).");
            TryInsertStudent(context, "Тест Возраст", 200, "age.test@example.com");

            Console.WriteLine();
            Console.WriteLine("Попытка 2: повторный email (нарушает UNIQUE).");
            TryInsertStudent(context, "Тест Email", 20, "anna.ivanova@mail.ru");
        }

        private static void TryInsertStudent(AppDbContext context, string fullName, int age, string email)
        {
            try
            {
                context.Database.ExecuteSql(
                    $"INSERT INTO Students (FullName, Age, Email) VALUES ({fullName}, {age}, {email})");

                Console.WriteLine("Запись добавлена (для этого теста так быть не должно).");
            }
            catch (SqliteException ex)
            {
                Console.WriteLine($"База отклонила запись: {ex.Message}");
            }
        }

        // ===== 5. Индексы: план выполнения запроса =====

        public static void ShowQueryPlans(AppDbContext context)
        {
            Console.WriteLine("Запрос по Email (есть уникальный индекс):");
            PrintPlan(context, "SELECT * FROM Students WHERE Email = 'anna.ivanova@mail.ru'");

            Console.WriteLine();
            Console.WriteLine("Запрос по FullName (есть обычный индекс):");
            PrintPlan(context, "SELECT * FROM Students WHERE FullName = 'Анна Иванова'");

            Console.WriteLine();
            Console.WriteLine("Запрос по Age (индекса нет):");
            PrintPlan(context, "SELECT * FROM Students WHERE Age = 19");
        }

        // SQL здесь — константы из кода. Строки от пользователя так подставлять нельзя.
        private static void PrintPlan(AppDbContext context, string sql)
        {
            var connection = context.Database.GetDbConnection();
            context.Database.OpenConnection();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "EXPLAIN QUERY PLAN " + sql;

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Console.WriteLine("   " + reader.GetString(3));
                }
            }
            finally
            {
                context.Database.CloseConnection();
            }
        }
    }
}