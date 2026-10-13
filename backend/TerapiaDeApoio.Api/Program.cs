using Microsoft.EntityFrameworkCore;

using TerapiaDeApoio.Infrastructure.Data;

using TerapiaDeApoio.Application.Interfaces;

using TerapiaDeApoio.Application.Services;

using TerapiaDeApoio.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("TerapiaDeApoio")
    ?? throw new InvalidOperationException(
        "A conexão TerapiaDeApoio não foi configurada.");

builder.Services.AddDbContext<TerapiaDeApoioDbContext>(
    options => options.UseNpgsql(connectionString));
// Serviços da aplicação
builder.Services.AddScoped<AgendamentoService>();

builder.Services.AddScoped<ServicoService>();
builder.Services.AddScoped<PacienteService>();
builder.Services.AddScoped<ProfissionalService>();

// Repositórios
builder.Services.AddScoped<
    IPacienteRepository,
    PacienteRepository>();

builder.Services.AddScoped<
    IProfissionalRepository,
    ProfissionalRepository>();

builder.Services.AddScoped<
    IServicoRepository,
    ServicoRepository>();

builder.Services.AddScoped<
    IAgendamentoRepository,
    AgendamentoRepository>();

builder.Services.AddScoped<
    IProfissionalRepository,
    ProfissionalRepository>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet(
    "/api/health/database",
    CheckDatabaseConnection);


app.Run();

static async Task<IResult> CheckDatabaseConnection(
    TerapiaDeApoioDbContext db,
    CancellationToken cancellationToken)
{
    var conectado = await db.Database.CanConnectAsync(
        cancellationToken);

    if (!conectado)
    {
        return Results.Problem(
            title: "Não foi possível conectar ao PostgreSQL.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    return Results.Ok(new
    {
        status = "Conectado",
        banco = "PostgreSQL"
    });
}
