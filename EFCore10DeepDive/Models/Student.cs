namespace EFCore10DeepDive.Models;

/// <summary>
/// Demonstrates LeftJoin/RightJoin LINQ operators and SplitQuery consistency
/// </summary>
public class Student
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    
    // For SplitQuery demo - multiple collections
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}

public class Department
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<Student> Students { get; set; } = new List<Student>();
}

/// <summary>
/// Enrollment entity for demonstrating SplitQuery with multiple includes
/// </summary>
public class Enrollment
{
    public int Id { get; set; }
    public required string CourseName { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public string? Grade { get; set; }
}
