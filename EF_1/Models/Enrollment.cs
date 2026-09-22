using System;
using System.Collections.Generic;
using System.Text;

namespace EF_1.Models
{
    public class Enrollment
    {
        // Внешний ключ к студенту.
        public int StudentId { get; set; }

        // Навигация: студент из данной записи.
        public Student Student { get; set; } = null!;

        // Внешний ключ к курсу.
        public int CourseId { get; set; }

        // Навигация: курс из данной записи.
        public Course Course { get; set; } = null!;

        // Дополнительные данные именно о связи.
        public DateTime EnrolledAt { get; set; }

        // Оценка может отсутствовать до завершения курса.
        public int? Grade { get; set; }
    }
}
