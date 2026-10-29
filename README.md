# Tareas

App sencilla de registro de tareas (crear, editar, marcar como hecha y eliminar) construida con **.NET 10 + Blazor Web App (Interactive Server) + EF Core + SQLite**.

## Ejecutar

```bash
dotnet run --project TaskApp
```

Abre http://localhost:5167. La base de datos se crea sola en `TaskApp/App_Data/tasks.db` la primera vez.

## Funcionalidad

- Añadir tareas con el campo de arriba (Enter o botón **Añadir**).
- Marcar/desmarcar como hecha con el círculo de la izquierda.
- Editar: doble clic sobre el título o botón del lápiz. **Enter** guarda, **Esc** cancela.
- Eliminar con confirmación en línea (botón del cubo).
- Filtros **Todas / Pendientes / Hechas** y acción **Limpiar hechas**.
- Progreso circular y fechas relativas ("hoy", "ayer").
- Tema claro/oscuro automático según el sistema.

## Estructura

```
TaskApp/
├── Components/
│   ├── Pages/Tasks.razor        # UI completa (lista, formulario, filtros)
│   └── Pages/Tasks.razor.css    # estilos de la página (CSS isolation)
├── Data/
│   ├── AppDbContext.cs          # DbContext y mapeo de la tabla
│   ├── AppDbContextFactory.cs   # solo para `dotnet ef`
│   └── Migrations/              # migraciones versionadas
├── Models/TaskItem.cs           # entidad (Id, Title, IsDone, CreatedAt, CompletedAt)
├── Services/TaskService.cs      # CRUD, usa un DbContext corto por operación
└── Program.cs                   # DI + SQLite + migraciones al arrancar

TaskApp.Tests/                   # tests de integración del CRUD (SQLite en memoria)
```

## Persistencia

SQLite fue elegido por ser cero configuración: un archivo, transacciones reales, consultas SQL y migraciones. EF Core abstrae el proveedor, así que migrar a PostgreSQL/SQL Server después es cambiar el paquete y la cadena de conexión.

- **Ubicación**: `TaskApp/App_Data/tasks.db` (configurable en `appsettings.json` → `ConnectionStrings:Default`).
- **Esquema**: se aplican las migraciones automáticamente al arrancar (`MigrateAsync`).
- **Reset**: borra `TaskApp/App_Data/tasks.db*` y vuelve a arrancar.

### Añadir una migración

```bash
dotnet ef migrations add NombreDelCambio --project TaskApp --output-dir Data/Migrations
```

### Cambiar a PostgreSQL

```bash
dotnet add TaskApp package Npgsql.EntityFrameworkCore.PostgreSQL
```

En `Program.cs`, sustituye `UseSqlite(...)` por `UsePostgres(connectionString)` y borra las migraciones de SQLite (`Data/Migrations`) antes de regenerarlas.

## Tests

```bash
dotnet test
```

9 tests de integración cubren el CRUD, el orden de la lista y las validaciones, usando SQLite en memoria.
