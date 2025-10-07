using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FCG.SolicitaPedidos.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FCG.SolicitaPedidos;

public class Function1
{
    private const string PedidosEndpointSetting = "URLAPI";
    private const string RotaSolicitacao = "RotaSolicitacao";
    private const string ApiKey = "ApiKey";

    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly string? _pedidosEndpoint;
    private readonly string? _rotaSolicitacao;
    private readonly string? _ApiKey;

    public Function1(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<Function1>();
        _httpClient = new HttpClient();
        _pedidosEndpoint = Environment.GetEnvironmentVariable(PedidosEndpointSetting);
        _rotaSolicitacao = Environment.GetEnvironmentVariable(RotaSolicitacao);
        _ApiKey = Environment.GetEnvironmentVariable(ApiKey);
    }

    [Function("SolicitaPedidos")]
    public async Task Run([TimerTrigger("*/10 * * * * *")] TimerInfo myTimer)
    {
        try
        {
            _logger.LogInformation("Processar novos pedidos");



            var numberOfGames = ObterIds();
            var requestPayload = MontarRequest(numberOfGames);

            if (string.IsNullOrWhiteSpace(_pedidosEndpoint) || string.IsNullOrWhiteSpace(_pedidosEndpoint))
            {
                _logger.LogWarning(
                    "Variavel de ambiente {setting} nao configurada. Pedido nao enviado. Payload: {payload}",
                    PedidosEndpointSetting,
                    JsonSerializer.Serialize(requestPayload));
            }
            else
            {
                try
                {
                    var fullEndpoint = $"{_pedidosEndpoint}{_rotaSolicitacao}";
                    _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _ApiKey);
                    var response = await _httpClient.PostAsJsonAsync(fullEndpoint, requestPayload);
                    response.EnsureSuccessStatusCode();
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Pedido enviado com sucesso. StatusCode: {statusCode}", response.StatusCode);
                    _logger.LogInformation($"{responseContent}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao enviar pedido para {endpoint}", _pedidosEndpoint);
                }
            }

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pedidos");
            throw;
        }
       
    }

    internal Request MontarRequest(List<int> numberOfGames)
    {
        try
        {
            var jogosSelecionados = ObterJogos(numberOfGames);

            var items = jogosSelecionados.Select(game => new Item
            {
                jogoId = Guid.NewGuid(),
                description = game.title,
                unitPrice = Random.Shared.Next(50, 351),
                quantity = Random.Shared.Next(1, 3)
            }).ToList();

            return new Request
            {
                userId = Guid.NewGuid(),
                currency = "BRL",
                items = items
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao montar request");
            throw;
        }
        
    }

    internal IReadOnlyList<Games> ObterJogos(List<int> idsJogos)
    {
        try
        {
            
            var jsonPath = Path.Combine(AppContext.BaseDirectory, "JsonGames", "ListOfGames.json");
            var games = new List<Games>();
            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"Arquivo de jogos nao encontrado em {jsonPath}");
            }

            var json = File.ReadAllText(jsonPath);
            var jogos = JsonSerializer.Deserialize<List<Games>>(json) ?? new List<Games>();

            if (jogos.Count == 0)
            {
                return Array.Empty<Games>();
            }


            foreach (var id in idsJogos)
            {
                var jogo = jogos.FirstOrDefault(j => j.id == id);
                if (jogo != null)
                {
                    games.Add(jogo);
                }
            }
            return games.OrderBy(x => x.id).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao obter jogos");
            throw;
        }
        
    }

    internal List<int> ObterIds(int number = 15)
    {
        try
        {
            List<int> ids = new List<int>();
            for (int i = 0; i < number; i++)
            {
                ids.Add(Random.Shared.Next(1, 599));
            }

            return ids;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao geraros ids");
            throw;
        }
        
        
    }
}
