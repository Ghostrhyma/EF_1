using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EF_1.Data;
using EF_1.Desktop.Models;
using EF_1.Models;
using EF_1.Services;
using Microsoft.EntityFrameworkCore;

namespace EF_1.Desktop.Services;

public class SchoolDataService
{
    private bool _initialized;

    // Создаёт БД и тестовые данные один раз за запуск.
    public async Task InitializeAsync(CancellationToken token)
    {
        if (_initialized)
        {
            return;
        }

        await using var context = new AppDbContext();

        await context.Database.EnsureCreatedAsync(token);

        // Seed-методы синхронные и быстрые, для учебного примера этого достаточно.
        SeedService.SeedStudents(context);
        SeedService.SeedTeachers(context);
        SeedService.SeedCourses(context);

        _initialized = true;
    }

    public async Task<List<StudentRow>> GetStudentsAsync(
        string? search,
        StudentSortOrder sortOrder,
        CancellationToken token)
    {
        // Новый контекст на каждую операцию: в десктопе так надёжнее всего.
        await using var context = new AppDbContext();

        // AsNoTracking: данные нужны только для показа.
        var query = context.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string text = search.Trim();

            query = query.Where(s =>
                s.FullName.Contains(text) ||
                s.Email.Contains(text));
        }

        var ordered = sortOrder switch
        {
            StudentSortOrder.ByAgeDescending => query
                .OrderByDescending(s => s.Age)
                .ThenBy(s => s.FullName),

            StudentSortOrder.ById => query
                .OrderBy(s => s.Id),

            _ => query
                .OrderBy(s => s.FullName)
        };

        return await ordered
            .Select(s => new StudentRow
            {
                Id = s.Id,
                FullName = s.FullName,
                Age = s.Age,
                Email = s.Email,
                TeacherName = s.Teacher != null ? s.Teacher.FullName : "не назначен",
                CoursesCount = s.Enrollments.Count
            })
            .ToListAsync(token);
    }

    public async Task AddStudentAsync(
        string fullName,
        int age,
        string email,
        CancellationToken token = default)
    {
        await using var context = new AppDbContext();

        bool emailExists = await context.Students
            .AnyAsync(s => s.Email == email, token);

        if (emailExists)
        {
            throw new InvalidOperationException("Студент с таким email уже существует.");
        }

        context.Students.Add(new Student
        {
            FullName = fullName,
            Age = age,
            Email = email
        });

        await context.SaveChangesAsync(token);
    }

    public async Task<bool> DeleteStudentAsync(int id, CancellationToken token = default)
    {
        await using var context = new AppDbContext();

        var student = await context.Students.FindAsync(new object[] { id }, token);

        if (student is null)
        {
            return false;
        }

        context.Students.Remove(student);
        await context.SaveChangesAsync(token);

        return true;
    }
}