using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskApp.Components;
using TaskApp.Data;
using TaskApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// SQLite: el archivo vive en App_Data/tasks.db. Las rutas relativas se resuelven
// contra el content root para que funcione igual con `dotnet run` o publicado.
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=App_Data/tasks.db";
var sqlite = new SqliteConnectionStringBuilder(connectionString);
if (!Path.IsPathRooted(sqlite.DataSource))
    sqlite.DataSource = Path.Combine(builder.Environment.ContentRootPath, sqlite.DataSource);
Directory.CreateDirectory(Path.GetDirectoryName(sqlite.DataSource)!);

builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(sqlite.ToString()));
builder.Services.AddScoped<TaskService>();

var app = builder.Build();

// Aplica migraciones pendientes al arrancar (crea la BD la primera vez).
await using (var scope = app.Services.CreateAsyncScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.MigrateAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
