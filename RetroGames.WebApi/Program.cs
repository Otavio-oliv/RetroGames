using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RetroGames.WebApi.ActionFilters;
using RetroGames.WebApi.ApiModels.Responses;
using RetroGames.WebApi.Configuration;
using RetroGames.WebApi.Middlewares;

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

// Registra automaticamente os validators deste projeto.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Registra os controllers e o filtro de validação.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<FluentValidationFilter>();
});

// Padroniza os erros de model binding e JSON malformado.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var erros = context.ModelState
            .Where(entrada =>
                entrada.Value?.Errors.Count > 0 &&
                !string.IsNullOrWhiteSpace(entrada.Key) &&
                !entrada.Key.StartsWith('$'))
            .ToDictionary(
                entrada =>
                {
                    var nomeCampo = entrada.Key.Split('.').Last();

                    return char.ToLowerInvariant(nomeCampo[0])
                        + nomeCampo[1..];
                },
                entrada => entrada.Value!.Errors
                    .Select(erro =>
                        string.IsNullOrWhiteSpace(erro.ErrorMessage)
                            ? "Valor inválido."
                            : erro.ErrorMessage)
                    .ToArray());

        return new BadRequestObjectResult(
            new RetroGamesResponseApiModel<object>
            {
                ResponseCode = ResponseCodes.MODEL_STATE_INVALIDO,
                Data = erros.Count > 0 ? erros : null,
                ErrorMessage = "Há um erro com o modelo enviado."
            });
    };
});

builder.Services.AddOpenApi();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

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
