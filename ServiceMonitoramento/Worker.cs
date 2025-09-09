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
    private readonly IHttpClientFactory _httpClientFactory;

    public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            await Task.Delay(1000, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await ExecutarTarefa();
                await Task.Delay(30000, stoppingToken); //30 segundos
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
            await MensagemDiscordErro(ex.ToString(), "Erro ao executar tarefa", "");
            throw;
        }
    }

    private static async Task<List<DTOLoginAPI>> ObterConfig()
    {
        var configs = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
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
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build()
            .GetSection("Discord:WebhookErro").Value;

        var body = new
        {
            username = "Bot Monitoramento",
            avatar_url = "https://i.imgur.com/4M34hi2.png",
            content = $"Erro - Data Hora: {DateTime.Now} - {urlRequest}",
            embeds = new[]
            {
                new { title, description = response, color = 16711680 }
            }
        };

        await RequestDiscord(url!, body);
    }

    private async Task MensagemDiscordSucesso(string response, string urlRequest)
    {
        var url = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build()
            .GetSection("Discord:WebhookSucesso").Value;

        var body = new
        {
            username = "Bot Monitoramento",
            avatar_url = "https://i.imgur.com/4M34hi2.png",
            content = $"Sucesso - Data Hora: {DateTime.Now} - {urlRequest}",
            embeds = new[]
            {
                new { title = "Sucesso", description = response, color = 65280 }
            }
        };

        await RequestDiscord(url!, body);
    }

    private async Task RequestDiscord(string url, object body)
    {
        var client = _httpClientFactory.CreateClient("discord");
        var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(url, content);
    }

    private async Task Request(string url, object body, string token = "")
    {
        var client = _httpClientFactory.CreateClient("default");

        var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        long startTime = Stopwatch.GetTimestamp();
        using var response = await client.PostAsync(url, content);
        var elapsedTime = Stopwatch.GetElapsedTime(startTime).Milliseconds;

        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            await MensagemDiscordErro(responseJson + $" - Data Hora: {DateTime.Now}", $"{(int)response.StatusCode} - {response.ReasonPhrase}", url);
        }
        else
        {
            await MensagemDiscordSucesso(responseJson + $" - Tempo: {elapsedTime}ms - Data Hora: {DateTime.Now}", url);

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
            if (api2)
            {
                var urlBase = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build()
                    .GetSection("Apis:Api2:Url").Value;

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "registroapi2.json");
                if (!File.Exists(filePath)) return;

                var json = await File.ReadAllTextAsync(filePath);
                var bodyApiNova = JsonConvert.DeserializeObject<List<DTODeProjecaoRegistrosUploadMobile>>(json)!;

                bodyApiNova.ForEach(x => x.Code_controller += $" - {DateTime.Now.Ticks}");

                await Request($"{urlBase}/mobile/registro", bodyApiNova, token);
            }
            else
            {
                var urlBase = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build()
                    .GetSection("Apis:ApiR4:Url").Value;

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "registroapir4.json");
                if (!File.Exists(filePath)) return;

                var json = await File.ReadAllTextAsync(filePath);
                var bodyApiAntiga = JsonConvert.DeserializeObject<List<DTODeProjecaoRegistrosUploadMobileApiAntiga>>(json)!;

                bodyApiAntiga.ForEach(x => x.Code_controller += $" - {DateTime.Now.Ticks}");

                await Request($"{urlBase}/mobile/registro", bodyApiAntiga, token);
            }
        }
        catch (Exception e)
        {
            await MensagemDiscordErro(e.ToString(), "Erro no arquivo", "");
            throw;
        }
    }
}
