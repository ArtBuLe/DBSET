using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.AccessControl;
using ConsoleApp1;


var builder = Host.CreateApplicationBuilder(args);

string ConnString = builder.Configuration.GetConnectionString("Default") ?? throw new Exception();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(ConnString));

using var host = builder.Build();

using var scope = host.Services.CreateScope();

var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

bool canConnect = await db.Database.CanConnectAsync();

if (!canConnect)
{
    Console.WriteLine("Connect incomplete...");
    return;
}
Console.WriteLine("Connect complete!");
Console.WriteLine("DB : ", db.Database.GetDbConnection().Database);

var students = await db.Students.ToListAsync();

foreach (var st in students)
{
    Console.WriteLine
        (
        $"{st.Id,-5} : {st.FirstName,-10} " +
        $"{st.LastName,-10} " +
        $"Age = {st.Age}" 
        );
}


//Какова основная задача DbContext? - Управлять взаимодействием приложения с EF Core и базой данных

//Что представляет собой DbSet? - Набор сущностей Student, доступный через контекст

//Что делает AddDbContext? - Регистрирует контекст в контейнере зависимостей

//Когда выполняется запрос в выражении db.Students.ToListAsync()? - При материализации запроса через ToListAsync