using System;
using System.Collections.Generic;
using System.Text;

namespace EF_1.Models
{
    public class Student
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public int Age { get; set; }

        public string Email { get; set; } = string.Empty;

        // Внешний ключ: Id куратора.
        // int? означает, что у студента куратор может пока отсутствовать.
        public int? TeacherId { get; set; }

        // Навигационное свойство: объект куратора студента.
        public Teacher? Teacher { get; set; }

        // Пока оставляем для урока 12.
        public List<Enrollment> Enrollments { get; set; } = new();
    }
}
