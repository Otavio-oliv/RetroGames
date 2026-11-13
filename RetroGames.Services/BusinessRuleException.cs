namespace RetroGames.Services;

public class BusinessRuleException : Exception
{
    public BusinessError Erro { get; }

    public BusinessRuleException(BusinessError erro)
        : base(erro.Mensagem)
    {
        Erro = erro;
    }
}

