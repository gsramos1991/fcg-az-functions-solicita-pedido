# 📦 FCG Solicita Pedidos

Uma Azure Function em .NET 8 (Isolated Worker) que agenda a geração e o envio automático de pedidos fictícios para uma API externa. A função lê um catálogo local de jogos, monta um payload de itens aleatórios e realiza um POST autenticado em intervalos regulares.

✨ ✨ ✨

## 🚀 Features Implementadas

### 🎮 Funcionalidades de Negócio
- Geração de pedidos com itens aleatórios (preço, quantidade, descrição)
- Leitura de catálogo a partir de `JsonGames/ListOfGames.json`
- Envio de solicitação autenticada (chave APIM) para API externa
- Logs de execução com detalhes de sucesso/erro e conteúdo de resposta

### 🏗️ Arquitetura & Padrões
- Azure Functions Isolated Worker (.NET 8)
- Agendamento com `TimerTrigger` a cada 10 segundos
- Separação de responsabilidades simples: Function, Models e dados estáticos
- Configuração via variáveis de ambiente (em `local.settings.json`)

✨ ✨ ✨

## 🛠️ Bibliotecas e Componentes

### ⏱️ TimerTrigger
```csharp
[Function("SolicitaPedidos")]
public async Task Run([TimerTrigger("*/10 * * * * *")] TimerInfo myTimer)
```
- Execução periódica a cada 10s
- Próxima execução registrada via `myTimer.ScheduleStatus`

### 🌐 HTTP Client
```csharp
var fullEndpoint = $"{_pedidosEndpoint}{_rotaSolicitacao}";
_httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _ApiKey);
var response = await _httpClient.PostAsJsonAsync(fullEndpoint, requestPayload);
```
- `PostAsJsonAsync` para envio do payload
- Header `Ocp-Apim-Subscription-Key` para autenticação

### 📝 Logging & Telemetria
```csharp
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();
```
- `ILogger` para logs de fluxo, sucesso e falhas
- Application Insights configurado para telemetria (local e produção)

### ⚙️ Configurações
```csharp
_pedidosEndpoint = Environment.GetEnvironmentVariable("URLAPI");
_rotaSolicitacao = Environment.GetEnvironmentVariable("RotaSolicitacao");
_ApiKey = Environment.GetEnvironmentVariable("ApiKey");
```
- Parametrização por ambiente sem hardcode de endpoints/segredos

🎯 🎯 🎯

## 🗂️ Arquitetura da Solução

```
fcg-az-functions-solicita-pedido/
├── 📁 src/
│   └── 📁 FCG.SolicitaPedidos/
│       ├── 📁 JsonGames/                # 🎮 Catálogo local de jogos
│       │   └── ListOfGames.json
│       ├── 📁 Models/                   # 🧩 DTOs do payload e jogos
│       │   ├── Games.cs
│       │   └── Request.cs
│       ├── Program.cs                   # ⚙️ Bootstrap do Worker + AI
│       ├── Solicitar.cs                 # ⏱️ Função Timer (Function1)
│       ├── host.json                    # 🔧 Configs do host Functions
│       └── local.settings.json          # 🔐 Variáveis locais
│
├── 📁 tests/
│   └── 📁 FCG.SolicitaPedidos.Tests/    # 🧪 Testes unitários
│       ├── FCG.SolicitaPedidos.Tests.csproj
│       └── Function1Tests.cs
│
├── FCG.SolicitaPedidos.sln              # 🧭 Solution
└── azure-pipelines.yml                  # 🛠️ CI (Azure Pipelines)
```

🧩 🧩 🧩

## ⚙️ Configuração de Ambiente
Defina as variáveis antes de executar localmente ou publicar:

| Variável             | Descrição                                         |
|----------------------|----------------------------------------------------|
| `AzureWebJobsStorage`| Obrigatória para Functions local (`UseDevelopmentStorage=true`) |
| `URLAPI`             | URL base do endpoint de pedidos                    |
| `RotaSolicitacao`    | Rota relativa adicionada à URL base                |
| `ApiKey`             | Chave para o header `Ocp-Apim-Subscription-Key`   |

As variáveis podem ser declaradas em `src/FCG.SolicitaPedidos/local.settings.json` para execuções locais.

🧭 🧭 🧭

## 🚀 Como Executar

### Pré-requisitos
- ✅ .NET 8 SDK
- ✅ Azure Functions Core Tools
- ✅ Git (opcional)

### 1) Restaurar Dependências
```bash
dotnet restore
```

### 2) Configurar Variáveis
Edite `src/FCG.SolicitaPedidos/local.settings.json` e ajuste os valores de `URLAPI`, `RotaSolicitacao` e `ApiKey`.

### 3) Executar a Function
```bash
cd src/FCG.SolicitaPedidos
func start --csharp
```

Durante a execução, acompanhe os logs no console para confirmar o envio dos pedidos e eventuais erros de conexão.

🚀 🚀 🚀

## 🧪 Testes
Execute a suíte de testes com:

```bash
dotnet test FCG.SolicitaPedidos.sln
```

Eles validam:
- Intervalo e cardinalidade de IDs gerados
- Ordenação/filtragem correta dos jogos carregados do JSON
- Construção do payload final (preço/quantidade e associação aos jogos)

🌟 🌟 🌟

## 👥 Idealizadores do Projeto (Discord)
- 👨‍💻 Clovis Alceu Cassaro (`cloves_93258`)
- 👨‍💻 Gabriel Santos Ramos (`_gsramos`)
- 👨‍💻 Júlio César de Carvalho (`cesarsoft`)
- 👨‍💻 Marco Antonio Araujo (`_marcoaz`)
- 👩‍💻 Yasmim Muniz Da Silva Caraça (`yasmimcaraca`)
