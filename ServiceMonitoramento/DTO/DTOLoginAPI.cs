namespace ServiceMonitoramento.DTO;

public class DTOLoginAPI
{
    public string url { get; set; }
    public string usuario { get; set; }
    public string senha { get; set; }
    public string assinatura { get; set; }
    public string versao_app { get; set; }
    public string hash_licenca { get; set; }
}