using System.ComponentModel.DataAnnotations;

namespace RetroGames.WebApi.ApiModels.Jogo;

public class ListarJogoRequestApiModel
{
    public string? Busca { get; set; }

    public int? IdPlataforma { get; set; }

    public int? IdGenero { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior que zero.")]
    public int Pagina { get; set; } = 1;

    [Range(1, int.MaxValue, ErrorMessage = "O tamanho da página deve ser maior que zero.")]
    public int TamanhoPagina { get; set; } = 12;
}
