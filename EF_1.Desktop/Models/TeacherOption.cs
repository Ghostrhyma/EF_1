namespace EF_1.Desktop.Models;

// Элемент выпадающего списка кураторов. Id = null означает «без куратора».
public record TeacherOption(int? Id, string Name);