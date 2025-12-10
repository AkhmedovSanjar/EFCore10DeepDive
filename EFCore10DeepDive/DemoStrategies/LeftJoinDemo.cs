using EFCore10DeepDive.Data;
using EFCore10DeepDive.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCore10DeepDive.DemoStrategies;

/// <summary>
/// LeftJoin/RightJoin LINQ Operators
/// Benefits: Clean syntax, readable code, less code than GroupJoin+DefaultIfEmpty
/// </summary>
public class LeftJoinDemo : DemoBase
{
    public override string FeatureName => "LeftJoin/RightJoin LINQ Operators";
    public override string Description => "Clean LEFT/RIGHT JOIN syntax replacing complex GroupJoin";

    protected override async Task ExecuteDemoAsync(AppDbContext context)
    {
        var departments = new[]
        {
            new Department { Name = "Computer Science" },
            new Department { Name = "Mathematics" },
            new Department { Name = "Physics" }
        };
        context.Departments.AddRange(departments);
        await context.SaveChangesAsync();

        var students = new[]
        {
            new Student { Name = "Alice", DepartmentId = departments[0].Id },
            new Student { Name = "Bob", DepartmentId = departments[0].Id },
            new Student { Name = "Charlie", DepartmentId = null },
            new Student { Name = "Diana", DepartmentId = departments[1].Id }
        };
        context.Students.AddRange(students);
        await context.SaveChangesAsync();

        Console.WriteLine("OLD WAY (Before .NET 10): GroupJoin + SelectMany + DefaultIfEmpty");

        var oldLeftJoin = await context.Students
            .GroupJoin(
                context.Departments,
                s => s.DepartmentId,
                d => d.Id,
                (student, departments) => new { student, departments })
            .SelectMany(
                x => x.departments.DefaultIfEmpty(),
                (x, department) => new
                {
                    StudentName = x.student.Name,
                    DepartmentName = department != null ? department.Name : "None"
                })
            .ToListAsync();

        foreach (var item in oldLeftJoin)
        {
            Console.WriteLine($"    {item.StudentName,-10} -> {item.DepartmentName}");
        }

        Console.WriteLine("\nNEW WAY (.NET 10): LeftJoin");
        var leftJoin = await context.Students
            .LeftJoin(context.Departments, s => s.DepartmentId, d => d.Id,
                (s, d) => new
                {
                    StudentName = s.Name,
                    DepartmentName = d != null ? d.Name : "None"
                })
            .ToListAsync();

        foreach (var item in leftJoin)
        {
            Console.WriteLine($"    {item.StudentName,-10} -> {item.DepartmentName}");
        }

        Console.WriteLine("\nNEW WAY: RightJoin");
        var rightJoin = await context.Departments
            .RightJoin(context.Students, d => d.Id, s => s.DepartmentId,
                (d, s) => new
                {
                    DepartmentName = d != null ? d.Name : "No Department",
                    StudentName = s.Name
                })
            .ToListAsync();

        foreach (var item in rightJoin)
        {
            Console.WriteLine($"    {item.DepartmentName,-20} <- {item.StudentName}");
        }
    }
}

