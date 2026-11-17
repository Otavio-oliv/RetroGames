using RetroGames.WebApi.Configuration;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["ConnectionStrings:Default"];

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "A connection string 'ConnectionStrings:Default' não foi encontrada. " +
        "Verifique se a variável ConnectionStrings__Default está configurada no arquivo .env.");
}

builder.Services.AddRepositorios(connectionString);
builder.Services.AddServicosDeNegocio();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "RetroGames API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
