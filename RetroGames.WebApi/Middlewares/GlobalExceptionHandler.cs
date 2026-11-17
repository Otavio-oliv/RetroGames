
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using RetroGames.Services;
using RetroGames.WebApi.ApiModels.Responses;

namespace RetroGames.WebApi.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int statusCode;
        int responseCode;
        string errorMessage;

        if (exception is BusinessRuleException businessException)
        {
            statusCode = (int)businessException.Erro.Status;
            responseCode = businessException.Erro.Codigo;
            errorMessage = businessException.Erro.Mensagem;
        }
        else
        {
            _logger.LogError(
                exception,
                "Ocorreu um erro inesperado ao processar a requisição.");

            statusCode = (int)HttpStatusCode.InternalServerError;
            responseCode = ResponseCodes.ERRO;
            errorMessage = "Ocorreu um erro interno no servidor.";
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new RetroGamesResponseApiModel
            {
                ResponseCode = responseCode,
                ErrorMessage = errorMessage
            },
            cancellationToken);

        return true;
    }
}
