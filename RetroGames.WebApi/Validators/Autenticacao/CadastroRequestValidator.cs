
using FluentValidation;
using RetroGames.WebApi.ApiModels.Autenticacao;

namespace RetroGames.WebApi.Validators.Autenticacao;

public class CadastroRequestValidator
    : AbstractValidator<CadastroRequestApiModel>
{
    public CadastroRequestValidator()
    {
        RuleFor(request => request.Nome)
            .NotEmpty()
            .WithMessage("O nome é obrigatório.")
            .MaximumLength(100)
            .WithMessage("O nome deve ter no máximo 100 caracteres.");

        RuleFor(request => request.Email)
            .NotEmpty()
            .WithMessage("O e-mail é obrigatório.")
            .EmailAddress()
            .WithMessage("Informe um e-mail válido.")
            .MaximumLength(150)
            .WithMessage("O e-mail deve ter no máximo 150 caracteres.");

        RuleFor(request => request.Senha)
            .NotEmpty()
            .WithMessage("A senha é obrigatória.");
    }
}
