using ConsoleApp1;
using ConsoleApp1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
string connectionString = builder.Configuration.GetConnectionString("Default") ?? throw new InvalidOperationException
    (
        "Не найдена строка подключения DefaultConnection."
    );

builder.Services.AddDbContext<AppDbContext>
(
options => options.UseNpgsql(connectionString)
);

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();


try
{
    bool canConnect = await db.Database.CanConnectAsync();

    if (!canConnect)
    {
        Console.WriteLine("Не удалось подключиться к PostgreSQL.");
        return;
    }

    Console.WriteLine("Подключение к PostgreSQL успешно!");
    Console.WriteLine
    (
        $"База данных: {db.Database.GetDbConnection().Database}"
    );

    var students = await db.Students.ToListAsync();

    if (students.Count == 0)
    {
        Console.WriteLine("Студентов пока нет.");
    }
    else
    {
        Console.WriteLine("\n=== Список студентов ===");

        foreach (var student in students)
        {
            Console.WriteLine
            (
                $"{student.Id}: {student.FirstName} " +
                $"{student.LastName}, возраст: {student.Age}"
            );
        }
    }

    Console.WriteLine($"\nВсего студентов: {students.Count}");


    Console.WriteLine("=== П1 ===");
    Console.Write("Минимальный возраст: \n");
    int minAge = 1; //Convert.ToInt32(Console.ReadLine()!);

    Console.Write("Максимальный возраст: \n");
    int maxAge = 25; //Convert.ToInt32(Console.ReadLine()!);

    Console.Write("Номер страницы: \n");
    int page = 1; //Convert.ToInt32(Console.ReadLine()!);

    const int pageSize = 3;

    var searchQuery = db.Students
        .Where(s => s.Age >= minAge && s.Age <= maxAge)
        .OrderBy(s => s.LastName)
        .ThenBy(s => s.FirstName)
        .Select(s => new
        {
            s.FirstName,
            s.LastName,
            s.Age
        })
        .Skip((page - 1) * pageSize)
        .Take(pageSize);

    var searchResults = await searchQuery.ToListAsync();

    Console.WriteLine("\n=== Результат поиска ===");
    if (searchResults.Count == 0)
    {
        Console.WriteLine("Студенты не найдены.");
    }
    else
    {
        foreach (var student in searchResults)
        {
            Console.WriteLine($"{student.FirstName} {student.LastName}, {student.Age}");
        }
    }

    bool less_than_18 = await db.Students.AnyAsync(s => s.Age < 18);
    Console.WriteLine($"\nЕсть несовершеннолетние студенты: {less_than_18}");


    Console.WriteLine("\n\n=== П2 ===");

    var Stats = await db.Students
       .GroupBy(s => 1)
       .Select(g => new
       {
           TotalCount = g.Count(),
           AverageAge = g.Average(s => s.Age),
           MinAge = g.Min(s => s.Age),
           MaxAge = g.Max(s => s.Age)
       })
       .FirstOrDefaultAsync();

    if (Stats != null && Stats.TotalCount > 0)
    {
        Console.WriteLine($"Всего студентов: {Stats.TotalCount}\n");
        Console.WriteLine($"Минимальный возраст: {Stats.MinAge}");
        Console.WriteLine($"Средний возраст: {Stats.AverageAge}");
        Console.WriteLine($"Максимальный возраст: {Stats.MaxAge}");
    }
    else
    {
        Console.WriteLine("В базе нет студентов для аналитики.");
    }

    var Ages = await db.Students
        .GroupBy(s => s.Age)
        .Select(g => new 
        {
        Age = g.Key, 
        Count = g.Count()
        })
        .OrderBy(x => x.Age)
        .ToListAsync();

    Console.WriteLine("\nРаспределение по возрасту:");
    foreach (var item in Ages)
    {
        Console.WriteLine($"{item.Age} лет — {item.Count}");
    }
    bool allAdults = await db.Students.AllAsync(s => s.Age >= 18);
    Console.WriteLine($"\nВсе студенты совершеннолетние: {allAdults}");



    Console.WriteLine("\n\n=== NEXT TASK ===");
    Console.WriteLine("=== П1 ===");

    var groupsWithStudents = await db.Set<CustomGroup>()
        .Include(g => g.Students)
        .OrderBy(g => g.Name)
        .ToListAsync();

    foreach (var group in groupsWithStudents)
    {
        Console.WriteLine($"\nГруппа: {group.Name}");
        Console.WriteLine($"Количество студентов: {group.Students.Count}");

        if (group.Students.Count == 0)
        {
            Console.WriteLine("В группе пока нет студентов.");
        }
        else
        {
            Console.WriteLine("Список студентов:");
            foreach (var student in group.Students.OrderBy(s => s.LastName))
            {
                Console.WriteLine($"  - {student.FirstName} {student.LastName}");
            }
        }
    }

    Console.WriteLine("\n\n=== П2 ===");

    var statistics = await db.Set<CustomGroup>()
        .OrderBy(g => g.Name)
        .Select(g => new
        {
            GroupName = g.Name,
            StudentCount = g.Students.Count(),
            less_than_18 = g.Students.Count(s => s.Age < 18),
            AverageAge = g.Students
                .Select(s => (double?)s.Age)
                .Average(),
            Has_less_18 = g.Students.Any(s => s.Age < 18)
        })
        .ToListAsync();

    foreach (var item in statistics)
    {
        Console.WriteLine($"\nГруппа: {item.GroupName}");
        Console.WriteLine($"Студентов: {item.StudentCount}");
        Console.WriteLine($"Несовершеннолетних: {item.less_than_18}");

        Console.WriteLine
        (
            item.AverageAge.HasValue ? $"Средний возраст: {item.AverageAge.Value:F1}" : "Средний возраст: нет данных"
        );

        Console.WriteLine
        (
            item.Has_less_18 ? "В группе есть несовершеннолетние" : "Несовершеннолетних нет" 
        );
    }


    Console.WriteLine("\n\n=== NEXT TASK ===");
    Console.WriteLine("=== П1 ===");

    Console.WriteLine("1. Показать всех студентов с группами");
    Console.WriteLine("2. Добавить нового студента");
    Console.WriteLine("3. Изменить возраст студента по Id");
    Console.WriteLine("4. Удалить студента по Id");
    Console.Write("Выберите действие (1-4): ");

    string choice = Console.ReadLine()?.Trim() ?? "";

    switch (choice)
    {
        //show all
        case "1":
            var studentsWithGroups = await db.Students
                .Include(s => s.Group)
                .OrderBy(s => s.LastName)
                .ToListAsync();

            Console.WriteLine("\nСписок студентов:");
            if (studentsWithGroups.Count == 0)
            {
                Console.WriteLine("Студентов пока нет.");
            }
            else
            {
                foreach (var s in studentsWithGroups)
                {
                    Console.WriteLine($"[{s.Id}] {s.FirstName} {s.LastName}, {s.Age} лет. Группа: {s.Group?.Name ?? "Не назначена"}");
                }
            }
            break;

        //add
        case "2":
            Console.Write("Имя: ");
            string firstName = Console.ReadLine()?.Trim() ?? "";
            Console.Write("Фамилия: ");
            string lastName = Console.ReadLine()?.Trim() ?? "";
            Console.Write("Возраст: ");
            int.TryParse(Console.ReadLine(), out int age);
            Console.Write("Id группы: ");
            int.TryParse(Console.ReadLine(), out int groupId);

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                Console.WriteLine(" Ошибка: Имя и фамилия не могут быть пустыми.");
            }
            else if (age <= 0)
            {
                Console.WriteLine(" Ошибка: Возраст должен быть больше нуля.");
            }
            else
            {
                bool groupExists = await db.Groups.AnyAsync(g => g.Id == groupId);
                if (!groupExists)
                {
                    Console.WriteLine($" Ошибка: Группа с Id {groupId} не найдена.");
                }
                else
                {
                    var newStudent = new Student
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Age = age,
                        GroupId = groupId
                    };

                    db.Students.Add(newStudent);
                    await db.SaveChangesAsync();

                    Console.WriteLine($" Студент добавлен. Присвоенный Id: {newStudent.Id}");
                }
            }
            break;

        //change age
        case "3":
            Console.Write("Введите Id студента для изменения возраста: ");
            int.TryParse(Console.ReadLine(), out int updateId);

            var studentToUpdate = await db.Students.FirstOrDefaultAsync(s => s.Id == updateId);

            if (studentToUpdate == null)
            {
                Console.WriteLine(" Ошибка: Студент с таким Id не найден.");
            }
            else
            {
                Console.Write($"Текущий возраст: {studentToUpdate.Age}. Введите новый возраст: ");
                int.TryParse(Console.ReadLine(), out int newAge);

                if (newAge > 0)
                {
                    studentToUpdate.Age = newAge;
                    await db.SaveChangesAsync();
                    Console.WriteLine(" Возраст студента изменён.");
                }
                else
                {
                    Console.WriteLine(" Ошибка: Возраст должен быть больше нуля.");
                }
            }
            break;
            
        //remove
        case "4":
            Console.Write("Введите Id студента для удаления: ");
            int.TryParse(Console.ReadLine(), out int deleteId);

            var studentToDelete = await db.Students.FirstOrDefaultAsync(s => s.Id == deleteId);

            if (studentToDelete == null)
            {
                Console.WriteLine(" Ошибка: Студент с таким Id не найден.");
            }
            else
            {
                db.Students.Remove(studentToDelete);
                await db.SaveChangesAsync();
                Console.WriteLine(" Студент удалён.");
            }
            break;

        default:
            Console.WriteLine(" Неверный выбор. Введите число от 1 до 4.");
            break;
    }

    Console.WriteLine("\n\n=== П2 ===");
    Console.Write("Введите Id студента для перевода: ");
    int.TryParse(Console.ReadLine(), out int transferId);

    var studentToTransfer = await db.Students
        .Include(s => s.Group)
        .FirstOrDefaultAsync(s => s.Id == transferId);

    if (studentToTransfer == null)
    {
        Console.WriteLine(" Ошибка: Студент с таким Id не найден.");
    }
    else
    {
        Console.WriteLine($"\nСтудент: {studentToTransfer.FirstName} {studentToTransfer.LastName}");
        Console.WriteLine($"Текущая группа: {studentToTransfer.Group?.Name ?? "Не назначена"} (Id группы: {studentToTransfer.GroupId})");

        var availableGroups = await db.Groups.OrderBy(g => g.Name).ToListAsync();
        Console.WriteLine("\nДоступные группы:");
        foreach (var g in availableGroups)
        {
            Console.WriteLine($"  [{g.Id}] {g.Name}");
        }

        Console.Write("\nВведите Id новой группы: ");
        int.TryParse(Console.ReadLine(), out int newGroupId);

        bool targetGroupExists = await db.Groups.AnyAsync(g => g.Id == newGroupId);

        if (!targetGroupExists)
        {
            Console.WriteLine(" Ошибка: Указанная группа не найдена.");
        }
        else if (newGroupId == studentToTransfer.GroupId)
        {
            Console.WriteLine("Студент уже находится в этой группе.");
        }
        else
        {
            studentToTransfer.GroupId = newGroupId;
            await db.SaveChangesAsync();

            studentToTransfer.Group = availableGroups.First(g => g.Id == newGroupId);
            Console.WriteLine($"Cтудент переведён в группу \"{studentToTransfer.Group.Name}\".");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка работы с базой данных: {ex.Message}");
}