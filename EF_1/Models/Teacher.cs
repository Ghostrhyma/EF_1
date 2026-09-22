using System;
using System.Collections.Generic;
using System.Text;

namespace EF_1.Models
{
    public class Teacher
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        // Один преподаватель может быть куратором многих студентов.
        public List<Student> Students { get; set; } = new();
    }
}
