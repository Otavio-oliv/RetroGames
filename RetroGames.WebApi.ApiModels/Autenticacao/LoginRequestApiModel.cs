
namespace RetroGames.WebApi.ApiModels.Autenticacao;

public class LoginRequestApiModel
{
    public string Email { get; set; } = string.Empty;

    public string Senha { get; set; } = string.Empty;
}
