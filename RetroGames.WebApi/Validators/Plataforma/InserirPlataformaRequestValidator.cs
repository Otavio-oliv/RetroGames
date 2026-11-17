using FluentValidation;
using RetroGames.WebApi.ApiModels.Plataforma;

namespace RetroGames.WebApi.Validators.Plataforma;

public class InserirPlataformaRequestValidator
    : AbstractValidator<InserirPlataformaRequestApiModel>
{
    public InserirPlataformaRequestValidator()
    {
        RuleFor(request => request.Nome)
            .NotEmpty()
            .WithMessage("O nome da plataforma é obrigatório.")
            .MaximumLength(80)
            .WithMessage("O nome da plataforma deve ter no máximo 80 caracteres.");

        RuleFor(request => request.Fabricante)
            .NotEmpty()
            .WithMessage("O fabricante é obrigatório.")
            .MaximumLength(80)
            .WithMessage("O fabricante deve ter no máximo 80 caracteres.");

        RuleFor(request => request.AnoLancamento)
            .InclusiveBetween(1970, 2100)
            .WithMessage("O ano de lançamento deve estar entre 1970 e 2100.");
    }
}
