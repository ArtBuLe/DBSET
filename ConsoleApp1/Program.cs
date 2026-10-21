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
    Console.Write("Минимальный возраст: ");
    int minAge = Convert.ToInt32(Console.ReadLine()!);

    Console.Write("Максимальный возраст: ");
    int maxAge = Convert.ToInt32(Console.ReadLine()!);

    Console.Write("Номер страницы: ");
    int page = Convert.ToInt32(Console.ReadLine()!);

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

}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка работы с базой данных: {ex.Message}");
}