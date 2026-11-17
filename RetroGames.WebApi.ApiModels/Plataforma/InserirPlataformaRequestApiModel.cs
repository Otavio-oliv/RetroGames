namespace RetroGames.WebApi.ApiModels.Plataforma;

public class InserirPlataformaRequestApiModel
{
    public string Nome { get; set; } = string.Empty;

    public string Fabricante { get; set; } = string.Empty;

    public int AnoLancamento { get; set; }
}
