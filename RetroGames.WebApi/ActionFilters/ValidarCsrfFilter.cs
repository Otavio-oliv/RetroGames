
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RetroGames.WebApi.ApiModels.Responses;

namespace RetroGames.WebApi.ActionFilters;

public class ValidarCsrfFilter : IAsyncAuthorizationFilter
{
    private readonly IAntiforgery _antiforgery;

    public ValidarCsrfFilter(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    public async Task OnAuthorizationAsync(
        AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;

        // Requisições que não alteram dados não precisam de CSRF.
        if (HttpMethods.IsGet(request.Method) ||
            HttpMethods.IsHead(request.Method) ||
            HttpMethods.IsOptions(request.Method) ||
            HttpMethods.IsTrace(request.Method))
        {
            return;
        }

        try
        {
            await _antiforgery.ValidateRequestAsync(
                context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            context.Result = new ObjectResult(
                new RetroGamesResponseApiModel<object>
                {
                    ResponseCode = ResponseCodes.TOKEN_CSRF_INVALIDO,
                    Data = null,
                    ErrorMessage = "Token CSRF ausente ou inválido."
                })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }
    }
}
