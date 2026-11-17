using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RetroGames.WebApi.ApiModels.Responses;

namespace RetroGames.WebApi.ActionFilters;

public class FluentValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public FluentValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        foreach (var argumento in context.ActionArguments)
        {
            if (argumento.Value == null)
            {
                context.Result = new BadRequestObjectResult(
                    new RetroGamesResponseApiModel<object>
                    {
                        ResponseCode = ResponseCodes.MODEL_NULL,
                        Data = null,
                        ErrorMessage = "O modelo veio nulo."
                    });

                return;
            }

            var tipoValidator = typeof(IValidator<>)
                .MakeGenericType(argumento.Value.GetType());

            var validator = _serviceProvider.GetService(tipoValidator);

            if (validator is not IValidator validatorEncontrado)
            {
                continue;
            }

            var resultado = await validatorEncontrado.ValidateAsync(
                new ValidationContext<object>(argumento.Value));

            if (resultado.IsValid)
            {
                continue;
            }

            var erros = resultado.Errors
                .GroupBy(erro => char.ToLowerInvariant(erro.PropertyName[0])
                    + erro.PropertyName[1..])
                .ToDictionary(
                    grupo => grupo.Key,
                    grupo => grupo.Select(erro => erro.ErrorMessage).ToArray());

            context.Result = new BadRequestObjectResult(
                new RetroGamesResponseApiModel<object>
                {
                    ResponseCode = ResponseCodes.MODEL_STATE_INVALIDO,
                    Data = erros,
                    ErrorMessage = "Há um erro com o modelo enviado."
                });

            return;
        }

        await next();
    }
}
