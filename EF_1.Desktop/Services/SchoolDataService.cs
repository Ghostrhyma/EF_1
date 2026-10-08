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

    public async Task InitializeAsync(CancellationToken token)
    {
        if (_initialized)
        {
            return;
        }

        await using var context = new AppDbContext();

        await context.Database.EnsureCreatedAsync(token);

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
        await using var context = new AppDbContext();

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
                TeacherId = s.TeacherId,
                TeacherName = s.Teacher != null ? s.Teacher.FullName : "не назначен",
                CoursesCount = s.Enrollments.Count
            })
            .ToListAsync(token);
    }

    public async Task<List<TeacherOption>> GetTeachersAsync(CancellationToken token = default)
    {
        await using var context = new AppDbContext();

        return await context.Teachers
            .AsNoTracking()
            .OrderBy(t => t.FullName)
            .Select(t => new TeacherOption(t.Id, t.FullName))
            .ToListAsync(token);
    }

    // id == null — создать нового студента, иначе обновить существующего.
    public async Task SaveStudentAsync(
        int? id,
        string fullName,
        int age,
        string email,
        int? teacherId,
        CancellationToken token = default)
    {
        await using var context = new AppDbContext();

        int currentId = id ?? 0;

        bool emailExists = await context.Students
            .AnyAsync(s => s.Email == email && s.Id != currentId, token);

        if (emailExists)
        {
            throw new InvalidOperationException("Студент с таким email уже существует.");
        }

        if (id is null)
        {
            context.Students.Add(new Student
            {
                FullName = fullName,
                Age = age,
                Email = email,
                TeacherId = teacherId
            });
        }
        else
        {
            var student = await context.Students
                .FindAsync(new object[] { id.Value }, token);

            if (student is null)
            {
                throw new InvalidOperationException(
                    "Студент не найден: возможно, его уже удалили.");
            }

            student.FullName = fullName;
            student.Age = age;
            student.Email = email;
            student.TeacherId = teacherId;
        }

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