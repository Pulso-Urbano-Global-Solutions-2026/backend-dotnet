# Pulso Urbano — API .NET

**Global Solution 2026/1 · FIAP · ADS 2º ano**  
Disciplina: **Advanced .NET** · Apresentação: **REMOTA**

---

## Por que uma API .NET separada?

A API Java é o núcleo — ela ingere dados de satélites, calcula scores e serve o app mobile. A API .NET tem uma responsabilidade diferente e bem delimitada: **gerenciar o ciclo de vida de alertas históricos de qualidade do ar**.

Quando o banco Oracle detecta um score CRÍTICO via trigger, ele insere um registro em `ALERTA_HISTORICO`. Esta API é quem gerencia esses registros — confirmação, estatísticas por zona, resumo de 30 dias. Ela não emite JWT (isso é responsabilidade do Java), mas valida o JWT do Java via segredo compartilhado.

É separação de responsabilidades aplicada à arquitetura: duas APIs, um banco Oracle compartilhado sem FK cruzada entre domínios.

---

## Equipe

| Integrante | RM | Papel |
|------------|-----|-------|
| **Felipe Ferrete** | 562999 | Tech Lead · **dono desta entrega** — arquitetou e implementou a API .NET completa, Dockerfiles, deploy Azure |
| **Clayton Alves** | 562285 | Database · DevOps · Docker Compose e infraestrutura Azure |
| **Guilherme Sola** | 563674 | Mobile · Frontend |
| **Gustavo Bosak** | 566315 | QA · Arquitetura TOGAF · testes funcionais |
| **Nikolas Brisola** | 564371 | IoT · ESP32 |

---

## Arquitetura

```
App Mobile / Swagger
        │
        │ HTTP :5000
        ▼
.NET API (ASP.NET Core .NET 10)
  ├── GlobalExceptionMiddleware    ← trata todas as exceções em um único lugar
  ├── JwtValidationMiddleware      ← valida Bearer token (segredo do Java)
  ├── AlertaController             ← CRUD completo de alertas
  ├── EstatisticasController       ← stats por zona e resumo global
  ├── HealthController             ← health check do container
  ├── AlertaService                ← lógica de negócio
  ├── EstatisticasService          ← agregações
  └── AppDbContext (EF Core 8)
        │
        │ Oracle.EntityFrameworkCore 8.23.x
        ▼
Oracle 19c
  ├── ZONA_REFERENCIA_NET   (espelho de ZONA_CIDADE — seed determinístico)
  └── ALERTA_HISTORICO      (1:N com ZonaReferencia · DeleteBehavior.Restrict)

Java API (porta 8080) ──► compartilha JWT_SECRET ──► .NET API valida tokens
```

---

## Stack

| Componente | Versão |
|------------|--------|
| ASP.NET Core | .NET 10 (`net10.0`) |
| Entity Framework Core | 8.x |
| Oracle.EntityFrameworkCore | 8.23.x |
| Swashbuckle.AspNetCore | 6.6.x (Swagger com botão Bearer) |
| FluentValidation.AspNetCore | 11.x |
| System.IdentityModel.Tokens.Jwt | 8.x |
| xUnit + FluentAssertions | 2.x / 6.x |
| Microsoft.EntityFrameworkCore.Sqlite | 8.x (testes in-memory) |
| Docker | multi-stage build → imagem `mcr.microsoft.com/dotnet/aspnet:10.0` |

---

## Endpoints

| Método | Rota | Auth | Status codes |
|--------|------|------|-------------|
| `POST` | `/api/alertas` | Bearer | 201, 400, 401, 404 |
| `GET` | `/api/alertas` | Público | 200 |
| `GET` | `/api/alertas/{id}` | Público | 200, 404 |
| `PUT` | `/api/alertas/{id}/confirmar` | Bearer | 200, 401, 404 |
| `DELETE` | `/api/alertas/{id}` | Bearer | 204, 401, 404, **409** |
| `GET` | `/api/estatisticas/zona/{zonaId}` | Público | 200, 404 |
| `GET` | `/api/estatisticas/resumo` | Público | 200 |
| `GET` | `/api/health` | Público | 200 |

O **409 Conflict** no DELETE acontece quando a zona ainda possui alertas vinculados — EF Core `DeleteBehavior.Restrict` lançado como exceção controlada pelo `GlobalExceptionMiddleware`.

Valores válidos para `nivelAlerta`: `ATENCAO`, `ALERTA`, `EMERGENCIA`.

**Swagger em produção:** `http://20.12.204.186:5000/swagger`  
**Java API (emite os JWTs):** `https://hearty-adaptation-production-6de3.up.railway.app/swagger-ui.html`

---

## Como Rodar

### Opção 1 — Docker (recomendado para demo)

```bash
git clone <url-do-repo>
cd PU-backend-dotnet

# Copiar e preencher variáveis de ambiente
cp .env.example .env
# Editar .env com seus valores Oracle e JWT_SECRET

# Build e run
docker build -t pulso-dotnet:gs ./PulsoUrbano.Net
docker run --rm --env-file .env -p 5000:5000 pulso-dotnet:gs

# Verificar
curl http://localhost:5000/api/health
# Swagger: http://localhost:5000/swagger
```

Para subir o stack completo (Oracle + Java + .NET juntos), use o `docker-compose.yml` do repositório `devops/`.

### Opção 2 — dotnet run (desenvolvimento local)

**Pré-requisitos:** .NET 10 SDK, Oracle 19c acessível, `dotnet-ef` global instalado.

```bash
git clone <url-do-repo>
cd PU-backend-dotnet

# Instalar ferramenta EF Core (uma vez)
dotnet tool install --global dotnet-ef --version 8.*

# Configurar variáveis de ambiente (PowerShell)
$env:DB_USER     = "system"
$env:DB_PASS     = "oracle"
$env:DB_HOST     = "localhost"
$env:DB_PORT     = "1521"
$env:DB_SERVICE  = "XEPDB1"
$env:JWT_SECRET  = "<mesmo valor do Java>"
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Aplicar migrations + seed automático
dotnet ef database update --project PulsoUrbano.Net

# Rodar
dotnet run --project PulsoUrbano.Net
# Swagger: http://localhost:5000/swagger
```

Em `Development`, o app executa automaticamente no boot:
1. `Database.Migrate()` — aplica todas as migrations pendentes
2. `DataSeeder.SeedAsync()` — insere 5 zonas + 40 alertas de demonstração

### Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `DB_USER` | Usuário Oracle | `system` |
| `DB_PASS` | Senha Oracle | `oracle` |
| `DB_HOST` | Host do Oracle | `localhost` |
| `DB_PORT` | Porta Oracle | `1521` |
| `DB_SERVICE` | Service name Oracle | `XEPDB1` |
| `JWT_SECRET` | Segredo compartilhado com a API Java | obrigatório |
| `ASPNETCORE_ENVIRONMENT` | `Development` ou `Production` | `Production` |

---

## Migrations (EF Core)

As migrations são a evidência exigida pela rubrica. Aqui está o ciclo completo:

```bash
# Ver migrations existentes
dotnet ef migrations list --project PulsoUrbano.Net
# → 20260529145413_InitialCreate (Applied)

# Aplicar no banco (idempotente — seguro rodar várias vezes)
dotnet ef database update --project PulsoUrbano.Net

# Adicionar nova migration (após alterar uma entidade)
dotnet ef migrations add NomeDaMigracao \
  --project PulsoUrbano.Net \
  --output-dir Data/Migrations

# Reverter para migration anterior (se necessário)
dotnet ef database update 20260529145413_InitialCreate --project PulsoUrbano.Net
```

**O que `InitialCreate` faz:**
- Cria a tabela `ZONA_REFERENCIA_NET` (ID_ZONA, NOME, MUNICIPIO)
- Cria a tabela `ALERTA_HISTORICO` (ID_ALERTA, ID_ZONA FK, NIVEL_ALERTA, SCORE_REGISTRADO, NO2_REGISTRADO, DT_ALERTA, CONFIRMADO)
- Usa sequences Oracle: `SEQ_ZONA_REFERENCIA` e `SEQ_ALERTA_HISTORICO` (HiLo, INCREMENT BY 10)
- Define FK com `DeleteBehavior.Restrict` (deletar zona com alertas → DbUpdateException → 409)

---

## Testes

Os testes rodam com **SQLite in-memory** — Oracle não é necessário. Não há mocks de banco de dados.

```bash
# Todos os testes
dotnet test tests/PulsoUrbano.Net.Tests

# Apenas integração (WebApplicationFactory — controllers reais)
dotnet test tests/PulsoUrbano.Net.Tests --filter "FullyQualifiedName~Integration"

# Suite específica
dotnet test tests/PulsoUrbano.Net.Tests --filter "FullyQualifiedName~AlertaServiceTests"

# Com output detalhado
dotnet test tests/PulsoUrbano.Net.Tests --logger "console;verbosity=detailed"
```

Saída esperada:
```
Test run for PulsoUrbano.Net.Tests.dll (.NETCoreApp, Version=10.0)
Passed! - Failed: 0, Passed: XX, Skipped: 0, Total: XX
```

### Cobertura por Classe

| Arquivo de Teste | O que testa |
|-----------------|-------------|
| `Data/DataSeederTests.cs` | Seed insere 5 zonas e 40 alertas corretamente; idempotente |
| `DTOs/AlertaCreateDTOValidatorTests.cs` | FluentValidation — campos obrigatórios, range de score, valores de nivelAlerta |
| `Middleware/JwtValidationMiddlewareTests.cs` | Rotas públicas passam sem token; rotas protegidas retornam 401 sem Bearer |
| `Exceptions/GlobalExceptionMiddlewareTests.cs` | KeyNotFoundException → 404; DbUpdateException com FK → 409; genérica → 500 |
| `Services/AlertaServiceTests.cs` | CreateAsync, GetAsync com filtros, GetByIdAsync, ConfirmarAsync, DeleteAsync |
| `Services/EstatisticasServiceTests.cs` | Stats por zona (score médio, tendência, dias críticos), resumo global |
| `Controllers/HealthControllerTests.cs` | `/api/health` retorna 200 com campos corretos |
| `Integration/AlertaControllerTests.cs` | Fluxo completo POST → GET → PUT → DELETE via HTTP real |
| `Integration/EstatisticasControllerTests.cs` | Estatísticas após seed de alertas com distribuição conhecida |
| `Integration/MigrationSmokeTests.cs` | Migrations aplicam sem erro; schema criado corretamente |

---

## Exemplos de Uso

### 1. Obter um JWT da API Java

```bash
curl -s -X POST https://hearty-adaptation-production-6de3.up.railway.app/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"usuario@exemplo.com","senha":"Senha@123"}'
# → { "token": "eyJhbGci...", "tipo": "Bearer" }

TOKEN="eyJhbGci..."
```

### 2. Criar alerta (requer Bearer)

```bash
curl -s -X POST http://localhost:5000/api/alertas \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "zonaId": 1,
    "nivelAlerta": "ALERTA",
    "scoreRegistrado": 38.5,
    "no2Registrado": 41.2,
    "textoRecomendacao": "Evite esforço físico ao ar livre entre 11h e 16h."
  }'
# → 201 Created
# {
#   "id": 42,
#   "zonaId": 1,
#   "zonaNome": "Centro",
#   "nivelAlerta": "ALERTA",
#   "scoreRegistrado": 38.5,
#   "no2Registrado": 41.2,
#   "dtAlerta": "2026-06-07T14:30:00Z",
#   "confirmado": false
# }
```

### 3. Listar alertas com filtros (público)

```bash
# Alertas da Zona Centro dos últimos 30 dias, página 1
curl -s "http://localhost:5000/api/alertas?zonaId=1&dias=30&pagina=1&tamanhoPagina=5"
# → 200 { "total": 12, "pagina": 1, "tamanhoPagina": 5, "dados": [...] }
```

### 4. Confirmar alerta (requer Bearer)

```bash
curl -s -X PUT http://localhost:5000/api/alertas/42/confirmar \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"confirmado": true}'
# → 200 { "id": 42, "confirmado": true, ... }
```

### 5. Deletar alerta (requer Bearer)

```bash
curl -s -X DELETE http://localhost:5000/api/alertas/42 \
  -H "Authorization: Bearer $TOKEN"
# → 204 No Content

# Tentar deletar ZONA que tem alertas:
curl -s -X DELETE http://localhost:5000/api/zonas/1 \
  -H "Authorization: Bearer $TOKEN"
# → 409 Conflict { "status": 409, "erro": "Operação bloqueada por integridade referencial" }
```

### 6. Estatísticas por zona (público)

```bash
curl -s "http://localhost:5000/api/estatisticas/zona/1?dias=30"
# → 200 {
#   "zonaId": 1, "zonaNome": "Centro",
#   "totalAlertas": 12,
#   "alertasPorNivel": { "ATENCAO": 5, "ALERTA": 5, "EMERGENCIA": 2 },
#   "scoreMinimo": 22.1, "scoreMaximo": 78.4, "scoreMedia": 51.3,
#   "diasComAlerta": 8,
#   "tendencia": "ESTAVEL"
# }
```

### 7. Resumo geral (público)

```bash
curl -s http://localhost:5000/api/estatisticas/resumo
# → 200 {
#   "totalZonas": 5, "totalAlertas30dias": 47,
#   "zonaComMaisAlertas": { "id": 2, "nome": "Zona Leste", "total": 18 },
#   "nivelPredominante": "ALERTA",
#   "dtAtualizacao": "2026-06-07T14:30:00Z"
# }
```

### 8. Health check

```bash
curl -s http://localhost:5000/api/health
# → 200 {
#   "status": "healthy",
#   "servico": "pulso-urbano-dotnet",
#   "versao": "1.0.0",
#   "timestamp": "2026-06-07T14:30:00Z",
#   "database": "connected"
# }
```

---

## Estrutura do Projeto

```
PU-backend-dotnet/
├── PulsoUrbano.Net/
│   ├── Controllers/
│   │   ├── AlertaController.cs          ← CRUD + Swagger annotations
│   │   ├── EstatisticasController.cs    ← /zona/{id} e /resumo
│   │   └── HealthController.cs
│   ├── Data/
│   │   ├── AppDbContext.cs              ← EF Core context + entity configs
│   │   ├── DataSeeder.cs               ← 5 zonas + 40 alertas em Development
│   │   ├── EntityConfigurations/
│   │   │   ├── ZonaReferenciaConfiguration.cs
│   │   │   └── AlertaHistoricoConfiguration.cs  ← FK + DeleteBehavior.Restrict
│   │   └── Migrations/
│   │       └── 20260529145413_InitialCreate.cs
│   ├── Exceptions/
│   │   └── GlobalExceptionMiddleware.cs ← mapa de exceção → HTTP status
│   ├── Middleware/
│   │   └── JwtValidationMiddleware.cs   ← valida Bearer sem JWT library complexa
│   ├── Models/
│   │   ├── DTOs/                        ← AlertaCreateDTO, AlertaResponseDTO, etc.
│   │   └── Entities/
│   │       ├── ZonaReferencia.cs
│   │       └── AlertaHistorico.cs
│   ├── Services/
│   │   ├── IAlertaService.cs            ← contrato de interface
│   │   ├── AlertaService.cs             ← implementação com EF Core
│   │   ├── IEstatisticasService.cs
│   │   └── EstatisticasService.cs
│   ├── Validators/
│   │   └── AlertaCreateDTOValidator.cs  ← FluentValidation rules
│   ├── Program.cs                       ← entry point, DI, middleware pipeline
│   └── Dockerfile                       ← multi-stage build
└── tests/
    └── PulsoUrbano.Net.Tests/
        ├── Infrastructure/
        │   ├── SqliteInMemoryFixture.cs  ← shared SQLite DbContext para testes
        │   └── PulsoWebAppFactory.cs     ← WebApplicationFactory<Program>
        ├── Integration/                  ← testes HTTP ponta a ponta
        ├── Services/                     ← unit tests de services
        ├── Controllers/                  ← unit tests de health controller
        ├── DTOs/                         ← validator tests
        ├── Middleware/                   ← JWT middleware tests
        └── Exceptions/                   ← exception middleware tests
```

---

## Rubrica Coberta

### Advanced .NET — Checklist (Apresentação Remota)

| Requisito | Status | Evidência |
|-----------|--------|-----------|
| API REST com arquitetura limpa | ✅ | Controllers finos → Services → EF Core; interfaces para injeção de dependência |
| CRUD completo | ✅ | POST, GET (lista + por ID), PUT (confirmar), DELETE — 5 operações em `AlertaHistorico` |
| Persistência relacional com relacionamento 1:N | ✅ | `ZonaReferencia` 1:N `AlertaHistorico` com FK no Oracle via EF Core |
| Migration no projeto | ✅ | `InitialCreate` — aplicada com `dotnet ef database update` |
| Validação de entrada | ✅ | FluentValidation: `nivelAlerta` obrigatório e in-list; `scoreRegistrado` entre 0 e 100 |
| Tratamento de erros padronizado | ✅ | `GlobalExceptionMiddleware`: exceções → HTTP status + `ErrorResponseDTO` |
| Autenticação JWT | ✅ | `JwtValidationMiddleware` valida Bearer em POST, PUT, DELETE |
| Swagger documentado | ✅ | Swashbuckle + anotações XML + botão "Authorize" Bearer |
| Deploy fora de localhost | ✅ | Azure VM `20.12.204.186:5000` via Docker Compose |
| Testes automatizados | ✅ | 10 classes de teste — unit + integração com WebApplicationFactory |
| README completo com How to, exemplos, links | ✅ | Este documento |
| Vídeo demonstração (até 8 min) | _gravar_ | [YouTube — preencher após gravação] |
| Vídeo pitch (até 3 min) | _gravar_ | [YouTube — preencher após gravação] |

---

## Perguntas da Banca

**"O que é uma Migration e por que é importante?"**
> Migration é um snapshot versionado do schema do banco. Em vez de executar SQL manualmente, o EF Core gera e aplica alterações de forma rastreável. A `InitialCreate` cria as duas tabelas .NET no Oracle. Para o professor verificar: `dotnet ef migrations list` mostra `(Applied)`.

**"Por que DeleteBehavior.Restrict e não Cascade?"**
> Se uma zona fosse deletada em cascata, perderíamos todo o histórico de alertas dela — dado de saúde ambiental com valor histórico. O Restrict força o consumidor da API a deletar os alertas explicitamente antes da zona, garantindo que a exclusão é intencional.

**"Por que a API .NET não emite JWT?"**
> JWT é responsabilidade da API Java (a identidade do usuário vive lá). O .NET valida o token via segredo compartilhado — é mais seguro do que ter dois emissores. Isso também significa que o professor pode testar o .NET usando o token do Swagger Java.

**"Como os testes funcionam sem Oracle?"**
> Usamos SQLite in-memory via `SqliteInMemoryFixture`. O EF Core cria as tabelas automaticamente com `EnsureCreated()`. O SQLite aceita o mesmo LINQ que o Oracle — os testes exercem o código real de service e controller sem dependência de infraestrutura externa.

**"Como demonstrar o fluxo ao vivo?"**
> 1. Logar na Java API (Swagger Railway) → copiar JWT  
> 2. Abrir .NET Swagger (Azure :5000) → colar no botão Authorize  
> 3. POST `/api/alertas` → ver 201  
> 4. GET `/api/alertas/1` → ver o alerta criado  
> 5. PUT `/api/alertas/1/confirmar` → ver `confirmado: true`  
> 6. GET `/api/estatisticas/resumo` → ver estatísticas atualizadas  
> 7. DELETE → 204

---

## Links

| Recurso | URL |
|---------|-----|
| .NET Swagger (Azure) | `http://20.12.204.186:5000/swagger` |
| Java Swagger (Railway) | `https://hearty-adaptation-production-6de3.up.railway.app/swagger-ui.html` |
| Repositório | _preencher após publicação_ |
| Vídeo demonstração | _[YouTube — preencher após gravação]_ |
| Vídeo pitch | _[YouTube — preencher após gravação]_ |
