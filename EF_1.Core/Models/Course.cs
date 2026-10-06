using System;
using System.Collections.Generic;
using System.Text;

namespace EF_1.Models
{
    public class Course
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public int DurationHours { get; set; }

        // Записи студентов на этот курс.
        public List<Enrollment> Enrollments { get; set; } = new();
    }
}
