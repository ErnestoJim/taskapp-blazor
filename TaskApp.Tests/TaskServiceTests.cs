using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskApp.Data;
using TaskApp.Services;

namespace TaskApp.Tests;

public sealed class TaskServiceTests : IAsyncLifetime
{
    private SqliteConnection _connection = default!;
    private ServiceProvider _provider = default!;
    private TaskService _service = default!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(_connection));
        _provider = services.BuildServiceProvider();

        _service = new TaskService(_provider.GetRequiredService<IDbContextFactory<AppDbContext>>());

        await using var db = await _provider
            .GetRequiredService<IDbContextFactory<AppDbContext>>()
            .CreateDbContextAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_GuardaLaTareaYLaDevuelve()
    {
        var created = await _service.AddAsync("  Comprar café  ");

        Assert.True(created.Id > 0);
        Assert.Equal("Comprar café", created.Title);
        Assert.False(created.IsDone);
        Assert.Null(created.CompletedAt);

        var all = await _service.GetAllAsync();
        Assert.Single(all);
        Assert.Equal("Comprar café", all[0].Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddAsync_RechazaTitulosVacios(string title)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(title));
        Assert.Empty(await _service.GetAllAsync());
    }

    [Fact]
    public async Task SetDoneAsync_MarcaYDesmarcaConTimestamp()
    {
        var task = await _service.AddAsync("Regar plantas");

        var done = await _service.SetDoneAsync(task.Id, true);
        Assert.NotNull(done);
        Assert.True(done!.IsDone);
        Assert.NotNull(done.CompletedAt);

        var pending = await _service.SetDoneAsync(task.Id, false);
        Assert.NotNull(pending);
        Assert.False(pending!.IsDone);
        Assert.Null(pending.CompletedAt);
    }

    [Fact]
    public async Task SetDoneAsync_DevuelveNullSiNoExiste()
    {
        Assert.Null(await _service.SetDoneAsync(999, true));
    }

    [Fact]
    public async Task RenameAsync_ActualizaYRecortaLosEspacios()
    {
        var task = await _service.AddAsync("Título viejo");

        var updated = await _service.RenameAsync(task.Id, "  Título nuevo  ");

        Assert.NotNull(updated);
        Assert.Equal("Título nuevo", updated!.Title);

        var all = await _service.GetAllAsync();
        Assert.Equal("Título nuevo", all.Single().Title);
    }

    [Fact]
    public async Task DeleteAsync_EliminaSoloLaTareaIndicada()
    {
        var keep = await _service.AddAsync("Se queda");
        var remove = await _service.AddAsync("Se va");

        await _service.DeleteAsync(remove.Id);

        var all = await _service.GetAllAsync();
        Assert.Single(all);
        Assert.Equal(keep.Id, all[0].Id);
    }

    [Fact]
    public async Task ClearCompletedAsync_BorraSoloLasHechas()
    {
        var done = await _service.AddAsync("Hecha");
        await _service.AddAsync("Pendiente");
        await _service.SetDoneAsync(done.Id, true);

        var deleted = await _service.ClearCompletedAsync();

        Assert.Equal(1, deleted);
        var all = await _service.GetAllAsync();
        Assert.Single(all);
        Assert.Equal("Pendiente", all[0].Title);
    }

    [Fact]
    public async Task GetAllAsync_OrdenaPendientesPrimeroYMasRecientesAntes()
    {
        var older = await _service.AddAsync("Antigua");
        var newer = await _service.AddAsync("Nueva");
        var completed = await _service.AddAsync("Completada");

        await _service.SetDoneAsync(completed.Id, true);
        await Task.Delay(10);
        await _service.RenameAsync(older.Id, "Antigua renombrada");

        var all = await _service.GetAllAsync();

        Assert.Equal([newer.Id, older.Id, completed.Id], all.Select(t => t.Id));
    }
}
