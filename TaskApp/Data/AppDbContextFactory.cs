using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskApp.Data;

/// <summary>
/// Solo para las herramientas de EF (`dotnet ef migrations ...`).
/// En runtime la configuración real viene de Program.cs + appsettings.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=App_Data/tasks.db")
            .Options;

        return new AppDbContext(options);
    }
}
