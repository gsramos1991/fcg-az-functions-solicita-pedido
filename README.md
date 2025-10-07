# FCG Solicita Pedidos

Solucao baseada em Azure Functions que agenda a geracao e o envio automatico de pedidos ficticios para uma API externa. A funcao coleta metadados de jogos em um arquivo local, monta um payload com itens aleatorios e dispara um POST autenticado periodicamente.

## Visao Geral
- **Trigger**: `TimerTrigger("20 * * * * *")` executa o fluxo a cada minuto, no segundo 20.
- **Carga de dados**: Le `JsonGames/ListOfGames.json` para obter informacoes dos jogos. IDs aleatorios sao sorteados e filtrados contra o catalogo.
- **Montagem do pedido**: Para cada jogo valido gera preco (R$50-R$350), quantidade (1-2) e GUIDs individuais. O corpo final contem `userId`, `currency` e colecao de itens.
- **Envio**: Concatena `URLAPI` + `RotaSolicitacao`, inclui header `Ocp-Apim-Subscription-Key` com `ApiKey` e envia via `HttpClient.PostAsJsonAsync`.
- **Telemetria**: Utiliza `ILogger` para registrar inicio, sucesso, respostas da API e falhas.

## Estrutura do Repositorio
- `FCG.SolicitaPedidos` - Projeto principal com a funcao `Function1`, modelos (`Models/`) e arquivo de dados estatico.
- `FCG.SolicitaPedidos.Tests` - Suite de testes unitarios cobrindo geracao de IDs, leitura do catalogo e montagem do payload.
- `JsonGames/ListOfGames.json` - Catalogo base usado na montagem dos pedidos.

## Configuracao de Ambiente
Defina as variaveis antes de executar localmente ou publicar:

| Variavel          | Descricao                                |
|-------------------|-------------------------------------------|
| `URLAPI`          | URL base do endpoint de pedidos           |
| `RotaSolicitacao` | Rota relativa adicionada a URL base       |
| `ApiKey`          | Chave para o header `Ocp-Apim-Subscription-Key` |

As variaveis podem ser declaradas em `local.settings.json` para execucoes locais.

## Execucao Local
1. Instale o Azure Functions Core Tools e o .NET 8 SDK.
2. Restaure dependencias: `dotnet restore`.
3. Ajuste as variaveis no `local.settings.json`.
4. Rode a funcao: `func start --csharp`.

Durante a execucao, observe os logs no console para confirmar o envio dos pedidos ou possiveis erros de conexao.

## Testes
Os testes unitarios podem ser executados com:

```bash
dotnet test FCG.SolicitaPedidos/FCG.SolicitaPedidos.sln
```

Eles validam:
- Intervalo e cardinalidade de IDs gerados.
- Ordenacao e filtragem de jogos carregados do JSON.
- Construcao do payload final, incluindo faixas de preco/quantidade e associacao correta aos jogos selecionados.

## Proximos Passos
- Tratar avisos de nulabilidade nos modelos (`Models/`), definindo valores padrao ou propriedades opcionais.
- Avaliar uso de `IHttpClientFactory` via DI para facilitar testes e resiliencia (retry/circuit breaker).
