using ConsoleApp1;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

string connectionString =
    builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Не найдена строка подключения DefaultConnection.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var db = scope.ServiceProvider
    .GetRequiredService<AppDbContext>();

try
{
    bool canConnect = await db.Database.CanConnectAsync();

    if (!canConnect)
    {
        Console.WriteLine("Не удалось подключиться к PostgreSQL.");
        return;
    }

    Console.WriteLine("Подключение к PostgreSQL успешно!");
    Console.WriteLine(
        $"База данных: {db.Database.GetDbConnection().Database}");

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
            Console.WriteLine(
                $"{student.Id}: {student.FirstName} " +
                $"{student.LastName}, возраст: {student.Age}");
        }
    }

    Console.WriteLine($"\nВсего студентов: {students.Count}");
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка работы с базой данных: {ex.Message}");
}


//Какова основная задача DbContext? - Управлять взаимодействием приложения с EF Core и базой данных

//Что представляет собой DbSet? - Набор сущностей Student, доступный через контекст

//Что делает AddDbContext? - Регистрирует контекст в контейнере зависимостей

//Когда выполняется запрос в выражении db.Students.ToListAsync()? - При материализации запроса через ToListAsync