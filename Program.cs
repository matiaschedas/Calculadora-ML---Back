using Matoli.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar CORS para permitir peticiones locales y desde Netlify
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Registrar controladores y servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar servicios propios como Singletons ya que son de solo lectura (cargan archivos JSON al inicio)
builder.Services.AddSingleton<ITarifasService, TarifasService>();
builder.Services.AddSingleton<ICategoriasService, CategoriasService>();
builder.Services.AddSingleton<ICalculoService, CalculoService>();

var app = builder.Build();

// Configurar Swagger en modo Desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Aplicar CORS antes de mapear controladores
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Endpoint de estado para comprobar que el backend corre bien
app.MapGet("/", () => Results.Ok(new { status = "MATOLI API is running fine!", version = "1.0.0" }));

// Endpoint de "wake up" para despertar al servicio
app.MapGet("/api/wake", () => Results.Ok(new { message = "Despierto!" }));

app.Run();
