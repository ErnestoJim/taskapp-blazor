using Microsoft.EntityFrameworkCore;
using TaskApp.Data;
using TaskApp.Models;

namespace TaskApp.Services;

/// <summary>
/// CRUD de tareas. Usa un DbContext corto por operación (via factory) para que
/// sea seguro en Blazor Server, donde el estado vive en el circuito.
/// </summary>
public class TaskService(IDbContextFactory<AppDbContext> dbFactory)
{
    public const int MaxTitleLength = 200;

    public async Task<List<TaskItem>> GetAllAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Tasks
            .AsNoTracking()
            .OrderBy(t => t.IsDone)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<TaskItem> AddAsync(string title)
    {
        var clean = Normalize(title);

        await using var db = await dbFactory.CreateDbContextAsync();
        var item = new TaskItem { Title = clean, CreatedAt = DateTime.UtcNow };
        db.Tasks.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task<TaskItem?> SetDoneAsync(int id, bool isDone)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var item = await db.Tasks.FindAsync(id);
        if (item is null) return null;

        item.IsDone = isDone;
        item.CompletedAt = isDone ? DateTime.UtcNow : null;
        await db.SaveChangesAsync();
        return item;
    }

    public async Task<TaskItem?> RenameAsync(int id, string title)
    {
        var clean = Normalize(title);

        await using var db = await dbFactory.CreateDbContextAsync();
        var item = await db.Tasks.FindAsync(id);
        if (item is null) return null;

        item.Title = clean;
        await db.SaveChangesAsync();
        return item;
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        await db.Tasks.Where(t => t.Id == id).ExecuteDeleteAsync();
    }

    public async Task<int> ClearCompletedAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Tasks.Where(t => t.IsDone).ExecuteDeleteAsync();
    }

    private static string Normalize(string title)
    {
        var clean = (title ?? string.Empty).Trim();
        if (clean.Length == 0)
            throw new ArgumentException("El título no puede estar vacío.", nameof(title));
        return clean.Length > MaxTitleLength ? clean[..MaxTitleLength] : clean;
    }
}
