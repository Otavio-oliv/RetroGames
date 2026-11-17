namespace RetroGames.WebApi.ApiModels.Responses;

public class RetroGamesResponseApiModel
{
    public int ResponseCode { get; set; }

    public string? ErrorMessage { get; set; }
}

public class RetroGamesResponseApiModel<T> : RetroGamesResponseApiModel
{
    public T? Data { get; set; }
}

