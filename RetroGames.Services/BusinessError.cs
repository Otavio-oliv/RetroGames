
using System.Net;

namespace RetroGames.Services;

public class BusinessError
{
    public int Codigo { get; }

    public string Mensagem { get; }

    public HttpStatusCode Status { get; }

    public BusinessError(
        int codigo,
        string mensagem,
        HttpStatusCode status)
    {
        Codigo = codigo;
        Mensagem = mensagem;
        Status = status;
    }
}
