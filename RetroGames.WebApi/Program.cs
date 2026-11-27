
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RetroGames.Services.Interfaces;
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

var emailAdministrador = builder.Configuration["AdminInicial:Email"];
var senhaAdministrador = builder.Configuration["AdminInicial:Senha"];

if (string.IsNullOrWhiteSpace(emailAdministrador) ||
    string.IsNullOrWhiteSpace(senhaAdministrador))
{
    throw new InvalidOperationException(
        "As configurações AdminInicial__Email e AdminInicial__Senha " +
        "devem estar preenchidas no arquivo .env.");
}

builder.Services.AddRepositorios(connectionString);
builder.Services.AddServicosDeNegocio();

builder.Services.AddAutenticacao();
builder.Services.AddAutorizacao();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<ValidarCsrfFilter>();

builder.Services.AddControllers(options =>
{
    options.Filters.AddService<ValidarCsrfFilter>();
    options.Filters.Add<FluentValidationFilter>();
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var erros = context.ModelState
            .Where(modelState => modelState.Value?.Errors.Count > 0)
            .ToDictionary(
                modelState => char.ToLowerInvariant(modelState.Key[0]) +
                              modelState.Key[1..],
                modelState => modelState.Value!.Errors
                    .Select(erro => erro.ErrorMessage)
                    .ToArray());

        return new BadRequestObjectResult(
            new RetroGamesResponseApiModel<object>
            {
                ResponseCode = ResponseCodes.MODEL_STATE_INVALIDO,
                Data = erros,
                ErrorMessage = "Há um erro com o modelo enviado."
            });
    };
});

builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var autenticacaoService = scope.ServiceProvider
        .GetRequiredService<IAutenticacaoService>();

    await autenticacaoService.GarantirAdministradorInicialAsync(
        emailAdministrador,
        senhaAdministrador);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "RetroGames API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
