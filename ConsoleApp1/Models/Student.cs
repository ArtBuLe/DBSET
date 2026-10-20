using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Age { get; set; }
        //public string Email { get; set; } = "";
        //public DateOnly Year { get; set; }
        //public bool IsActive { get; set; } = true;

        public Student(int id, string firstName, string lastName, int age/*, string email, DateOnly year, bool isActive*/)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            //Email = email;
            //Year = year;
            //IsActive = isActive;
        }
        public Student(string firstName, string lastName, int age/*, string email, DateOnly year, bool isActive*/)
        {
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            //Email = email;
            //Year = year;
            //IsActive = isActive;
        }
    }
}
