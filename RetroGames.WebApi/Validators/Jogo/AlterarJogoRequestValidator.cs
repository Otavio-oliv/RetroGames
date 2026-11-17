using FluentValidation;
using RetroGames.WebApi.ApiModels.Jogo;

namespace RetroGames.WebApi.Validators.Jogo;

public class AlterarJogoRequestValidator
    : AbstractValidator<AlterarJogoRequestApiModel>
{
    public AlterarJogoRequestValidator()
    {
        RuleFor(request => request.Nome)
            .NotEmpty()
            .WithMessage("O nome do jogo é obrigatório.")
            .MaximumLength(150)
            .WithMessage("O nome do jogo deve ter no máximo 150 caracteres.");

        RuleFor(request => request.Descricao)
            .MaximumLength(1000)
            .WithMessage("A descrição deve ter no máximo 1000 caracteres.");

        RuleFor(request => request.AnoLancamento)
            .InclusiveBetween(1970, 2100)
            .WithMessage("O ano de lançamento deve estar entre 1970 e 2100.");

        RuleFor(request => request.Preco)
            .GreaterThan(0)
            .WithMessage("O preço deve ser maior que zero.")
            .PrecisionScale(10, 2, true)
            .WithMessage("O preço deve ter no máximo duas casas decimais.");

        RuleFor(request => request.Estoque)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O estoque não pode ser negativo.");

        RuleFor(request => request.IdPlataforma)
            .GreaterThan(0)
            .WithMessage("A plataforma é obrigatória.");

        RuleFor(request => request.IdCondicaoJogo)
            .GreaterThan(0)
            .WithMessage("A condição do jogo é obrigatória.");

        RuleFor(request => request.IdsGeneros)
            .NotEmpty()
            .WithMessage("Informe pelo menos um gênero.");

        RuleForEach(request => request.IdsGeneros)
            .GreaterThan(0)
            .WithMessage("Os identificadores dos gêneros devem ser maiores que zero.");
    }
}
