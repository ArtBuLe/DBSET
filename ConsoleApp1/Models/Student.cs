using ConsoleApp1.Models;

namespace ConsoleApp1
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Age { get; set; }
        public int GroupId { get; set; }
        public CustomGroup Group { get; set; } = null!;
    }
}
