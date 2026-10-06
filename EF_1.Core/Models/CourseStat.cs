namespace EF_1.Models
{
    public class CourseStat
    {
        public string Title { get; set; } = string.Empty;

        public long StudentsCount { get; set; }

        public double? AverageGrade { get; set; }
    }
}