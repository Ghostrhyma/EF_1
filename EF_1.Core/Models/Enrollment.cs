namespace EF_1.Models
{
    public class Enrollment
    {
        public int StudentId { get; set; }

        public Student Student { get; set; } = null!;

        public int CourseId { get; set; }

        public Course Course { get; set; } = null!;

        public DateTime EnrolledAt { get; set; }

        public int? Grade { get; set; }

        // Токен конкуренции: увеличивается при каждом изменении записи.
        public int Version { get; set; }
    }
}