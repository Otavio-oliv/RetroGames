
using FluentValidation;
using RetroGames.WebApi.ApiModels.Pedido;

namespace RetroGames.WebApi.Validators.Pedido;

public class CriarPedidoRequestValidator
    : AbstractValidator<CriarPedidoRequestApiModel>
{
    public CriarPedidoRequestValidator()
    {
        RuleFor(request => request.Itens)
            .NotNull()
            .WithMessage("A lista de itens é obrigatória.")
            .NotEmpty()
            .WithMessage("O pedido deve possuir pelo menos um item.");

        RuleForEach(request => request.Itens)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.IdJogo)
                    .GreaterThan(0)
                    .WithMessage("Informe um jogo válido.");

                item.RuleFor(i => i.Quantidade)
                    .InclusiveBetween(1, 10)
                    .WithMessage(
                        "A quantidade de cada jogo deve estar entre 1 e 10.");
            });

        RuleFor(request => request.Itens)
            .Must(itens =>
                itens == null ||
                itens.Select(item => item.IdJogo).Distinct().Count() == itens.Count)
            .WithMessage(
                "Não é permitido repetir o mesmo jogo no pedido.");
    }
}
