using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace EFCore10DeepDive.Services;

/// <summary>
/// Interceptor to track and display SQL queries executed by EF Core
/// </summary>
public class QueryCounterInterceptor : DbCommandInterceptor
{
    public int SelectCount { get; private set; }
    public int InsertCount { get; private set; }
    public int UpdateCount { get; private set; }
    public int DeleteCount { get; private set; }
    public List<string> ExecutedQueries { get; } = new();
    public bool ShowSqlInConsole { get; set; } = false;

    public void Reset()
    {
        SelectCount = 0;
        InsertCount = 0;
        UpdateCount = 0;
        DeleteCount = 0;
        ExecutedQueries.Clear();
    }

    public int TotalCount => SelectCount + InsertCount + UpdateCount + DeleteCount;

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        TrackQuery(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        TrackQuery(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        TrackQuery(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        TrackQuery(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    private void TrackQuery(DbCommand command)
    {
        var sql = command.CommandText.Trim();
        
        // Skip system queries
        if (sql.Contains("INFORMATION_SCHEMA") || 
            sql.Contains("sys.") || 
            sql.StartsWith("CREATE TABLE") ||
            sql.StartsWith("DROP TABLE") ||
            sql.StartsWith("ALTER TABLE"))
        {
            return;
        }

        ExecutedQueries.Add(sql);

        // Count query types
        if (sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            SelectCount++;
        }
        else if (sql.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
        {
            InsertCount++;
        }
        else if (sql.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
        {
            UpdateCount++;
        }
        else if (sql.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
        {
            DeleteCount++;
        }

        // Optionally show SQL in console
        if (ShowSqlInConsole)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"    [SQL]: {SimplifySql(sql)}");
            Console.ResetColor();
        }
    }

    private string SimplifySql(string sql)
    {
        return sql.Replace("\r\n", " ").Replace("\n", " ");
    }

    public void PrintSummary()
    {
        Console.WriteLine($"  Total queries: {TotalCount} ({SelectCount} SELECT, {InsertCount} INSERT, {UpdateCount} UPDATE, {DeleteCount} DELETE)");
    }
}
