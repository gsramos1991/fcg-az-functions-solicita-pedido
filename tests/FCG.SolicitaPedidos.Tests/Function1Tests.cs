using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FCG.SolicitaPedidos;
using FCG.SolicitaPedidos.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FCG.SolicitaPedidos.Tests;

public class Function1Tests : IDisposable
{
    private readonly string _originalCurrentDirectory;
    private readonly Function1 _function;

    public Function1Tests()
    {
        _originalCurrentDirectory = Environment.CurrentDirectory;
        _function = new Function1(NullLoggerFactory.Instance);
    }

    public void Dispose()
    {
        Environment.CurrentDirectory = _originalCurrentDirectory;
    }

    [Fact]
    public void ObterIds_RetornaQuantidadeEIntervaloEsperados()
    {
        var ids = _function.ObterIds(10);

        Assert.Equal(10, ids.Count);
        Assert.All(ids, id => Assert.InRange(id, 1, 598));
    }

    [Fact]
    public void ObterJogos_ComIdsValidos_DeveRetornarOrdenado()
    {
        UsingGameData(_ =>
        {
            var jogos = _function.ObterJogos(new List<int> { 3, 1, 2 });

            Assert.Equal(3, jogos.Count);
            Assert.Collection(jogos,
                jogo => Assert.Equal(1, jogo.id),
                jogo => Assert.Equal(2, jogo.id),
                jogo => Assert.Equal(3, jogo.id));
            Assert.All(jogos, jogo => Assert.False(string.IsNullOrWhiteSpace(jogo.title)));
        });
    }

    [Fact]
    public void ObterJogos_ArquivoInexistente_DisparaExcecao()
    {
        var destino = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        Environment.CurrentDirectory = destino.FullName;

        try
        {
            var excecao = Assert.Throws<FileNotFoundException>(() => _function.ObterJogos(new List<int> { 1 }));
            Assert.Contains("JsonGames", excecao.Message);
        }
        finally
        {
            Environment.CurrentDirectory = _originalCurrentDirectory;
            Directory.Delete(destino.FullName, true);
        }
    }

    [Fact]
    public void MontarRequest_ComJogosValidos_DeveGerarItens()
    {
        UsingGameData(rootPath =>
        {
            var request = _function.MontarRequest(new List<int> { 1, 2, 99 });

            Assert.Equal("BRL", request.currency);
            Assert.NotEqual(Guid.Empty, request.userId);
            Assert.Equal(2, request.items.Count);

            var jogosSelecionados = _function.ObterJogos(new List<int> { 1, 2 });

            foreach (var item in request.items)
            {
                Assert.NotEqual(Guid.Empty, item.jogoId);
                Assert.InRange(item.unitPrice, 50, 350);
                Assert.InRange(item.quantity, 1, 2);
            }

            Assert.Equal(request.items.Count, request.items.Select(i => i.jogoId).Distinct().Count());

            var descricoesEsperadas = jogosSelecionados.Select(j => j.title).ToHashSet();
            Assert.All(request.items, item => Assert.Contains(item.description, descricoesEsperadas));
        });
    }

    private void UsingGameData(Action<string> test)
    {
        using var context = new GameDataContext();
        var previousDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = context.RootPath;

        try
        {
            test(context.RootPath);
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
        }
    }

    private sealed class GameDataContext : IDisposable
    {
        public string RootPath { get; }

        public GameDataContext()
        {
            RootPath = Directory.CreateTempSubdirectory("fcg-pedidos-tests").FullName;
            var jsonDir = Directory.CreateDirectory(Path.Combine(RootPath, "JsonGames"));
            var data = new[]
            {
                new
                {
                    id = 1,
                    title = "Primeiro Jogo",
                    thumbnail = "thumb1",
                    short_description = "desc",
                    game_url = "url1",
                    genre = "acao",
                    platform = "pc",
                    publisher = "pub",
                    developer = "dev",
                    release_date = "2024-01-01",
                    freetogame_profile_url = "profile1"
                },
                new
                {
                    id = 2,
                    title = "Segundo Jogo",
                    thumbnail = "thumb2",
                    short_description = "desc",
                    game_url = "url2",
                    genre = "acao",
                    platform = "pc",
                    publisher = "pub",
                    developer = "dev",
                    release_date = "2024-01-02",
                    freetogame_profile_url = "profile2"
                },
                new
                {
                    id = 3,
                    title = "Terceiro Jogo",
                    thumbnail = "thumb3",
                    short_description = "desc",
                    game_url = "url3",
                    genre = "acao",
                    platform = "pc",
                    publisher = "pub",
                    developer = "dev",
                    release_date = "2024-01-03",
                    freetogame_profile_url = "profile3"
                }
            };

            var jsonContent = JsonSerializer.Serialize(data);
            File.WriteAllText(Path.Combine(jsonDir.FullName, "ListOfGames.json"), jsonContent);
        }

        public void Dispose()
        {
            if (!Directory.Exists(RootPath))
            {
                return;
            }

            try
            {
                var fallbackDirectory = Path.GetTempPath();
                if (Environment.CurrentDirectory.StartsWith(RootPath, StringComparison.OrdinalIgnoreCase))
                {
                    Environment.CurrentDirectory = fallbackDirectory;
                }

                Directory.Delete(RootPath, true);
            }
            catch
            {
                // Ignorar falhas na limpeza para não causar falha no teste
            }
        }
    }
}
