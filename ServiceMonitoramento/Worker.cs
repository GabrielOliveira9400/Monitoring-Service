using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using ServiceMonitoramento.DTO;
using ServiceMonitoramento.DTO.ApiAntiga;

namespace ServiceMonitoramento;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
            
            while(true)
            {
                await ExecutarTarefa();
                await Task.Delay(30000); //30 segundos
            }
        }
    }
    
    private async Task ExecutarTarefa()
    {
        Console.WriteLine("Executando tarefa...");
        try
        {
            var configs = await ObterConfig();

            foreach (var config in configs)
            {
                var urlLogin = $"{config.url}/mobile/acesso/login";
                await Request(urlLogin, config);
            }
        }
        catch (Exception ex)
        {
            await MensagemDiscordErro(ex.Message, "Erro ao executar tarefa", "");
            throw;
        }
    }

    private static async Task<List<DTOLoginAPI>> ObterConfig()
    {
        var configs = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())  // Para garantir que o caminho esteja correto
            .AddJsonFile("appsettings.json")
            .Build();

        return configs.GetSection("Apis").Get<List<DTOLoginAPI>>() ?? new List<DTOLoginAPI>();
    }
    
    private static string TratarRespostaLogin(HttpResponseMessage response)
    {
        var json = response.Content.ReadAsStringAsync().Result;
        using JsonDocument doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("token").GetString() ?? string.Empty;
    }
    
    private async Task MensagemDiscordErro(string response, string title, string urlRequest)
    {
        var url = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())  // Para garantir que o caminho esteja correto
            .AddJsonFile("appsettings.json")
            .Build()
            .GetSection("Discord")
            .GetSection("WebhookErro")
            .Value;
        
        var body = new
        {
            username = "Bot Monitoramento",
            avatar_url = "https://i.imgur.com/4M34hi2.png",
            content = "Erro" + $" - Data Hora: {DateTime.Now} + {urlRequest}",
            embeds = new[]
            {
                new
                {
                    title,
                    description = response,
                    color = 16711680
                }
            }
        };
        
        try
        {
            await RequestDiscord(url, body);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    
    private async Task MensagemDiscordSucesso(string response, string urlRequest)
    {
        var url = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())  // Para garantir que o caminho esteja correto
            .AddJsonFile("appsettings.json")
            .Build()
            .GetSection("Discord")
            .GetSection("WebhookSucesso")
            .Value;
        
        var body = new
        {
            username = "Bot Monitoramento",
            avatar_url = "https://i.imgur.com/4M34hi2.png",
            content = "Sucesso" + $" - Data Hora: {DateTime.Now}" + $" - {urlRequest}",
            embeds = new[]
            {
                new
                {
                    title = "Sucesso",
                    description = response,
                    color = 65280
                }
            }
        };
        
        await RequestDiscord(url, body);
    }
    
    private static async Task RequestDiscord(string url, object body)
    {
        using HttpClient client = new();
        var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await client.PostAsync(url, content);
    }
    
    private async Task Request(string url, object body, string token = "")
    {
        using HttpClient client = new();
        var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }
        
        long startTime = Stopwatch.GetTimestamp();
        HttpResponseMessage response = await client.PostAsync(url, content);
        var elapsedTime = Stopwatch.GetElapsedTime(startTime).Milliseconds;
        
        
        if (!response.IsSuccessStatusCode)
        {
            var responseJson = response.Content.ReadAsStringAsync().Result + $" - Datas Hora: {DateTime.Now}";
            await MensagemDiscordErro(responseJson, (int)response.StatusCode + " - " + response.ReasonPhrase, url);
        }
        else
        {
            var responseJson = response.Content.ReadAsStringAsync().Result + $" - Tempo: {elapsedTime}ms" + $" - Datas Hora: {DateTime.Now}";
            
            await MensagemDiscordSucesso(responseJson, url);
            
            if (url.Contains("login"))
            {
                var tokenApi = TratarRespostaLogin(response);
                await EnviarRegistro(tokenApi, url.Contains("api2"));
            }
        }
    }

    private async Task EnviarRegistro(string token, bool api2)
    {
        try
        {
            var bodyApiNova = new List<DTODeProjecaoRegistrosUploadMobile>();
            
            var bodyApiAntiga = new List<DTODeProjecaoRegistrosUploadMobileApiAntiga>();

            
            
            if (api2)
            {
                var urlBase = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory()) // Para garantir que o caminho esteja correto
                    .AddJsonFile("appsettings.json")
                    .Build()
                    .GetSection("Apis")
                    .GetSection("Api2")
                    .GetSection("Url")
                    .Value;
                
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "registroapi2.json");
            
                if (!File.Exists(filePath))
                {
                    Console.WriteLine(filePath);
                    Console.WriteLine("Arquivo não encontrado");
                    return;
                }
                
                urlBase +="/mobile/registro";
            
                var json = await File.ReadAllTextAsync(filePath);
            
                bodyApiNova = JsonConvert.DeserializeObject<List<DTODeProjecaoRegistrosUploadMobile>>(json);
                
                bodyApiNova.ForEach(x =>
                {
                    x.Code_controller = x.Code_controller + " - " + DateTime.Now.Ticks;
                });
                
                await Request(urlBase, bodyApiNova, token);
            }
            else
            {
                var urlBase = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory()) // Para garantir que o caminho esteja correto
                    .AddJsonFile("appsettings.json")
                    .Build()
                    .GetSection("Apis")
                    .GetSection("ApiR4")
                    .GetSection("Url")
                    .Value;
                
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "registroapir4.json");
            
                if (!File.Exists(filePath))
                {
                    Console.WriteLine(filePath);
                    Console.WriteLine("Arquivo não encontrado");
                    return;
                }
                
                urlBase += "/mobile/registro";
            
                var json = await File.ReadAllTextAsync(filePath);
            
                bodyApiAntiga = JsonConvert.DeserializeObject<List<DTODeProjecaoRegistrosUploadMobileApiAntiga>>(json);
                
                bodyApiAntiga.ForEach(x =>
                {
                    x.Code_controller = x.Code_controller + " - " + DateTime.Now.Ticks;
                });
                
                await Request(urlBase, bodyApiAntiga, token);
            }
        }
        catch (Exception e)
        {
            await MensagemDiscordErro(e.Message, "Erro no arquivo", "");
            throw;
        }
    }
    
    
}