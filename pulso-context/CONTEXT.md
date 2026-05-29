# PULSO URBANO — CONTEXTO COMPLETO DE BACKEND
# Arquivo de contexto para qualquer LLM · Salvar na raiz do projeto

> **Como usar:** cole este arquivo inteiro no início de qualquer sessão de LLM antes de fazer perguntas técnicas.
> Para Claude Code: salve como `CLAUDE.md` na raiz do monorepo.
> Para outras sessões: cole como primeira mensagem antes de qualquer prompt.

---

## IDENTIDADE DO PROJETO

**Nome do produto:** Pulso Urbano  
**Contexto acadêmico:** Global Solution 2026/1 — FIAP · ADS 2º ano · Turmas de Fevereiro  
**Tema da GS:** Economia Espacial — uso de infraestrutura orbital para resolver problemas reais na Terra  
**Prazo de entrega:** 09/06/2026 até 23h55 · Apresentação presencial: 10–16/06/2026  
**Tech lead responsável pelo backend:** Felipe Ferrete (RM 562999)  

---

## O PROBLEMA

Todo dia, milhões de pessoas em São Paulo tomam decisões sem informação ambiental adequada:

- "Vou correr agora?" → Sem saber que o NO₂ está 3x acima do limite da OMS
- "Levo meu filho a pé?" → Sem saber que a temperatura de superfície está em 48°C
- "Saio de bike?" → Sem saber que em 2h a qualidade do ar melhora

**O dado já existe.** O satélite Sentinel-5P da ESA mede NO₂ em São Paulo diariamente, de graça.  
O ECOSTRESS da NASA mede a temperatura real do asfalto, de graça.  
**O que falta é o sistema que transforma esse dado orbital em decisão humana.**

---

## A SOLUÇÃO

**Pulso Urbano** é um app que transforma dados orbitais reais em uma frase simples:

> *"Hoje o ar está moderado. Prefira sair antes das 10h — especialmente se você corre ou tem filhos."*

**Sem jargão técnico. Sem dashboard GIS. Um score. Uma recomendação. Um mapa.**

### Por que é Economia Espacial
- Sem Sentinel-5P (ESA) → sem dado de NO₂ → produto não existe
- Sem ECOSTRESS (NASA/ISS) → sem temperatura de superfície → produto não existe
- Os dados são coletados em órbita, processados em terra, consumidos pelo nosso backend via API pública

---

## FONTES DE DADOS ORBITAIS (todas gratuitas)

| Satélite | O que mede | Resolução | Frequência | API |
|---|---|---|---|---|
| **Sentinel-5P / TROPOMI** (ESA) | NO₂, CO, ozônio, aerossóis | 3.5km × 5.5km | Diária | Copernicus Data Space |
| **ECOSTRESS** (NASA/ISS) | Temperatura real de superfície | 70m | A cada 3–5 dias por área | NASA Earthdata / AppEEARS |
| **OMI / TROPOMI** (NASA Aura) | Índice UV, aerossóis | 5km | Diária | NASA Giovanni API |
| **Open-Meteo** (fallback) | Temperatura do ar, umidade | Por coordenada | Tempo real | api.open-meteo.com (sem auth) |

### Autenticação Copernicus
```
POST https://identity.dataspace.copernicus.eu/auth/realms/CDSE/protocol/openid-connect/token
body: grant_type=password&client_id=cdse-public&username={EMAIL}&password={SENHA}
→ retorna: { access_token: "...", expires_in: 600 }
```

### Busca de produto Sentinel-5P
```
GET https://catalogue.dataspace.copernicus.eu/odata/v1/Products
  ?$filter=Collection/Name eq 'SENTINEL-5P'
    and Attributes/OData.CSC.StringAttribute/any(att:att/Name eq 'productType'
    and att/OData.CSC.StringAttribute/Value eq 'L2__NO2___')
    and OData.CSC.Intersects(area=geography'SRID=4326;POLYGON(
      (-46.83 -23.78,-46.83 -23.36,-46.36 -23.36,-46.36 -23.78,-46.83 -23.78))')
  &$orderby=ContentDate/Start desc
  &$top=1
Header: Authorization: Bearer {access_token}
```

---

## ARQUITETURA GERAL

```
┌─────────────────────────────────────────────────────┐
│                   FONTES ORBITAIS                    │
│  Sentinel-5P (ESA) · ECOSTRESS (NASA) · Open-Meteo  │
└──────────────┬──────────────────────────────────────┘
               │ OAuth2 / REST API
               ▼
┌──────────────────────────┐   ┌──────────────────────┐
│   JAVA API (PRIMÁRIA)    │   │   .NET API (SEC.)     │
│   Spring Boot 3.2        │   │   ASP.NET Core 8      │
│   Porta: 8080            │   │   Porta: 5000         │
│                          │   │                       │
│  - Ingestão orbital      │   │  - CRUD alertas       │
│  - Score engine          │   │  - Histórico usuário  │
│  - Recomendação          │   │  - Relatórios         │
│  - Mapa GeoJSON          │   │  - EF Core migrations │
│  - Auth JWT              │   │                       │
└──────────┬───────────────┘   └──────────┬────────────┘
           │ JPA / JDBC                   │ EF Core
           └──────────────┬───────────────┘
                          ▼
               ┌─────────────────┐
               │   ORACLE 19c    │
               │   (container)   │
               │   Porta: 1521   │
               └────────┬────────┘
                        │
            ┌───────────┴────────────┐
            │   PL/SQL ENGINE        │
            │  Procedures · Triggers │
            │  Cursores · Packages   │
            └────────────────────────┘
                        │
          ┌─────────────┼──────────────┐
          ▼             ▼              ▼
   [Mobile RN]    [ESP32/MQTT]   [QA/Tests]
```

---

## JAVA API — ESPECIFICAÇÃO COMPLETA

### Stack obrigatória (disciplina Java Advanced)
```
Spring Boot 3.2+
Spring Data JPA + JpaRepository
Spring Security 6 + JWT (JJWT 0.11+)
Spring Validation (jakarta.validation)
Lombok
Spring Boot DevTools
Swagger / OpenAPI 3 (springdoc-openapi 2.x)
HATEOAS (spring-hateoas)
Oracle JDBC Driver (ojdbc11)
Spring Scheduler (@Scheduled — para ingestão orbital)
```

### Estrutura de pacotes (seguir exatamente)
```
src/main/java/br/com/pulsourbano/
├── PulsoUrbanoApplication.java
│
├── config/
│   ├── SecurityConfig.java          # Spring Security + CORS
│   ├── JwtConfig.java               # configurações JWT
│   └── SwaggerConfig.java           # OpenAPI config
│
├── controller/
│   ├── ScoreController.java         # /api/v1/score/**
│   ├── RecomendacaoController.java  # /api/v1/recomendacao/**
│   ├── MapaController.java          # /api/v1/mapa/**
│   ├── UsuarioController.java       # /api/v1/usuario/**
│   └── AuthController.java          # /api/v1/auth/**
│
├── service/
│   ├── ScoreService.java            # algoritmo de score
│   ├── RecomendacaoService.java     # engine de templates
│   ├── IngestaoOrbitalService.java  # busca dados satélite
│   ├── CopernicusApiService.java    # client Copernicus
│   ├── NasaEarthDataService.java    # client NASA
│   └── UsuarioService.java
│
├── repository/
│   ├── ScoreDiarioRepository.java
│   ├── LeituraSateliteRepository.java
│   ├── ZonaCidadeRepository.java
│   ├── UsuarioRepository.java
│   └── RecomendacaoRepository.java
│
├── model/entity/
│   ├── Usuario.java                 # @Entity + herança se necessário
│   ├── ZonaCidade.java
│   ├── LeituraSatelite.java
│   ├── ScoreDiario.java
│   ├── Recomendacao.java
│   └── LogConsulta.java
│
├── model/dto/
│   ├── ScoreResponseDTO.java        # record
│   ├── ScoreCurrentResponseDTO.java # record
│   ├── RecomendacaoResponseDTO.java # record
│   ├── MapaCamadaDTO.java           # record — GeoJSON wrapper
│   ├── UsuarioCreateDTO.java        # record
│   ├── UsuarioResponseDTO.java      # record
│   ├── AuthRequestDTO.java          # record
│   └── AuthResponseDTO.java         # record (com token)
│
├── model/enums/
│   ├── ClassificacaoScore.java      # BOM, MODERADO, RUIM, CRITICO
│   ├── TipoDado.java                # NO2, TEMP_SUPERFICIE, UV
│   └── TipoSatelite.java            # SENTINEL_5P, ECOSTRESS, OMI
│
├── exception/
│   ├── GlobalExceptionHandler.java  # @RestControllerAdvice
│   ├── ResourceNotFoundException.java
│   ├── IngestaoException.java
│   └── ErrorResponseDTO.java        # record
│
└── scheduler/
    └── IngestaoOrbitalScheduler.java # @Scheduled diário às 6h
```

### Endpoints completos — contratos

#### AUTH
```
POST /api/v1/auth/login
Body: { "email": "string", "senha": "string" }
Response 200: { "token": "jwt...", "tipo": "Bearer", "expiraEm": "3600" }
Response 401: { "erro": "Credenciais inválidas" }

POST /api/v1/auth/register
Body: { "nome": "string", "email": "string", "senha": "string",
        "fazExercicio": boolean, "temCrianca": boolean, "temProblemaRespiratorio": boolean }
Response 201: { "id": 1, "nome": "...", "email": "...", "_links": {...} }
```

#### SCORE
```
GET /api/v1/score/current?lat=-23.5505&lon=-46.6333
Header: Authorization: Bearer {token}  ← OBRIGATÓRIO
Response 200:
{
  "score": 62.4,
  "classificacao": "MODERADO",
  "no2Ppb": 28.4,
  "tempSuperficieC": 41.2,
  "fonteDadoNo2": "Sentinel-5P TROPOMI",
  "fonteDadoTemp": "ECOSTRESS ISS/NASA",
  "dtDadoOrbital": "2026-06-01T08:00:00Z",
  "zonaId": 3,
  "zonaNome": "Zona Leste - São Paulo",
  "_links": { "self": {...}, "recomendacao": {...} }
}

GET /api/v1/score/historico?usuarioId=42&dias=7
Header: Authorization: Bearer {token}
Response 200: { "historico": [ { "dt": "...", "score": 72, "classificacao": "BOM" }, ... ] }

GET /api/v1/score/zonas
Response 200 (público — sem auth):
{ "zonas": [ { "id": 1, "nome": "Centro", "score": 55, "lat": -23.55, "lon": -46.63 }, ... ] }
```

#### RECOMENDAÇÃO
```
GET /api/v1/recomendacao?scoreId=99
Header: Authorization: Bearer {token}
Response 200:
{
  "texto": "Prefira sair antes das 10h. Concentração de NO₂ elevada entre 11h e 16h.",
  "icone": "warning",
  "nivel": "MODERADO",
  "personalizadaPara": ["exercicio_fisico", "crianca_em_casa"],
  "dtGeracao": "2026-06-01T07:30:00Z"
}
```

#### MAPA
```
GET /api/v1/mapa/camadas?tipo=no2&cidade=sao_paulo
Response 200 (GeoJSON — público):
{
  "type": "FeatureCollection",
  "fonte": "Sentinel-5P TROPOMI",
  "dtCaptura": "2026-06-01",
  "features": [
    {
      "type": "Feature",
      "geometry": { "type": "Point", "coordinates": [-46.63, -23.55] },
      "properties": { "zonaId": 1, "zonaNome": "Centro SP", "valor": 28.4, "unidade": "ppb" }
    }
  ]
}

GET /api/v1/mapa/camadas?tipo=temperatura&cidade=sao_paulo
→ mesmo formato, valor em °C
```

#### USUÁRIO (CRUD completo — 4 verbos HTTP)
```
POST   /api/v1/usuario          → 201 Created
GET    /api/v1/usuario/{id}     → 200 OK (próprio usuário autenticado)
PUT    /api/v1/usuario/{id}     → 200 OK
DELETE /api/v1/usuario/{id}     → 204 No Content

GET    /api/v1/usuario          → 200 OK (admin only — lista todos)
```

### Algoritmo de score — implementar exatamente assim
```java
// No ScoreService.java
public double calcularScore(double no2Ppb, double tempSuperficieC) {
    // NO₂: limite OMS = 25 ppb
    // 0 ppb = 100 pontos, 50 ppb = 0 pontos (linear, bounded 0–1)
    double scorNo2 = Math.max(0, 1 - (no2Ppb / 50.0));

    // Temperatura: abaixo de 30°C = confortável = 100 pts
    // acima de 50°C = crítico = 0 pts
    double scoreTemp = Math.max(0, 1 - Math.max(0, (tempSuperficieC - 30.0) / 20.0));

    // Peso: 60% qualidade do ar + 40% temperatura
    double scoreTotal = (scorNo2 * 0.60 + scoreTemp * 0.40) * 100;

    return Math.round(scoreTotal * 10.0) / 10.0; // 1 casa decimal
}

public ClassificacaoScore classificar(double score) {
    if (score >= 80) return ClassificacaoScore.BOM;
    if (score >= 60) return ClassificacaoScore.MODERADO;
    if (score >= 40) return ClassificacaoScore.RUIM;
    return ClassificacaoScore.CRITICO;
}
```

### Engine de recomendação — template por faixa (NÃO usar LLM)
```java
// No RecomendacaoService.java
public String gerarTexto(double score, boolean fazExercicio,
                           boolean temCrianca, boolean temProblemaResp) {

    String base = switch (ClassificacaoScore.from(score)) {
        case BOM -> "Ótimo dia para atividades ao ar livre. Qualidade do ar dentro dos limites da OMS.";
        case MODERADO -> "Qualidade do ar moderada. Prefira sair antes das 10h ou após as 17h.";
        case RUIM -> "Qualidade do ar ruim. Evite esforço físico prolongado ao ar livre.";
        case CRITICO -> "Qualidade do ar crítica. Recomendamos permanecer em ambientes fechados.";
    };

    StringBuilder sb = new StringBuilder(base);
    if (fazExercicio && score < 60)
        sb.append(" Evite corrida e ciclismo entre 11h e 16h.");
    if (temCrianca && score < 80)
        sb.append(" Crianças devem ter atividades ao ar livre limitadas hoje.");
    if (temProblemaResp && score < 75)
        sb.append(" Pessoas com asma ou rinite: use máscara se precisar sair.");

    return sb.toString();
}
```

### IngestaoOrbitalScheduler — agendamento diário
```java
@Component
@RequiredArgsConstructor
public class IngestaoOrbitalScheduler {

    private final CopernicusApiService copernicusService;
    private final NasaEarthDataService nasaService;
    private final ScoreService scoreService;
    private final ZonaCidadeRepository zonaRepo;

    // Roda todos os dias às 06:00 UTC (dados do dia anterior disponíveis)
    @Scheduled(cron = "0 0 6 * * *", zone = "UTC")
    public void ingerirDadosOrbitais() {
        List<ZonaCidade> zonas = zonaRepo.findAll();
        for (ZonaCidade zona : zonas) {
            try {
                double no2 = copernicusService.buscarNo2(zona.getLat(), zona.getLon());
                double temp = nasaService.buscarTempSuperficie(zona.getLat(), zona.getLon());
                scoreService.calcularEPersistir(zona, no2, temp);
            } catch (Exception e) {
                log.error("Falha na ingestão para zona {}: {}", zona.getId(), e.getMessage());
                // não interrompe o loop — continua para próxima zona
            }
        }
    }
}
```

### application.properties obrigatórias
```properties
# Oracle
spring.datasource.url=jdbc:oracle:thin:@localhost:1521/XEPDB1
spring.datasource.username=${DB_USER:system}
spring.datasource.password=${DB_PASS:oracle}
spring.datasource.driver-class-name=oracle.jdbc.OracleDriver

spring.jpa.database-platform=org.hibernate.dialect.OracleDialect
spring.jpa.hibernate.ddl-auto=validate
spring.jpa.show-sql=false

# JWT
jwt.secret=${JWT_SECRET:pulso-urbano-secret-key-2026-gs-fiap}
jwt.expiration=86400000

# Copernicus
copernicus.username=${COPERNICUS_USER}
copernicus.password=${COPERNICUS_PASS}
copernicus.token-url=https://identity.dataspace.copernicus.eu/auth/realms/CDSE/protocol/openid-connect/token
copernicus.catalog-url=https://catalogue.dataspace.copernicus.eu/odata/v1/Products

# NASA
nasa.earthdata.token=${NASA_EARTHDATA_TOKEN}
nasa.appeears-url=https://appeears.earthdatacloud.nasa.gov/api

# OpenAPI
springdoc.api-docs.path=/api-docs
springdoc.swagger-ui.path=/swagger-ui.html

# Server
server.port=8080
spring.application.name=pulso-urbano-java
```

---

## .NET API — ESPECIFICAÇÃO COMPLETA

### Stack obrigatória (disciplina .NET)
```
ASP.NET Core 8 (Minimal API ou MVC Controller)
Entity Framework Core 8
Oracle.EntityFrameworkCore (ODP.NET)
EF Core Migrations (obrigatório)
Swagger (Swashbuckle.AspNetCore)
BCrypt.Net-Next (hash de senha alternativa)
```

### Responsabilidade do .NET na arquitetura
O .NET gerencia dados de **alertas históricos e relatórios** — funcionalidade distinta do Java para evitar duplicação:
- CRUD completo de AlertaHistorico (evento de qualidade crítica registrado)
- Listagem de alertas por usuário
- Relatório de dias críticos por zona
- Endpoint de estatísticas agregadas

> **Regra:** o .NET não duplica o Java. Se Java faz score, .NET não faz score.
> Se Java faz auth JWT, .NET valida o mesmo token via middleware.

### Estrutura de projeto
```
PulsoUrbano.Net/
├── PulsoUrbano.Net.csproj
├── Program.cs                    # entry point + DI registration
├── appsettings.json
├── appsettings.Development.json
│
├── Controllers/
│   ├── AlertaController.cs       # /api/alertas/**
│   ├── EstatisticasController.cs # /api/estatisticas/**
│   └── HealthController.cs       # /api/health
│
├── Models/
│   ├── Entities/
│   │   ├── AlertaHistorico.cs    # entidade principal
│   │   └── ZonaReferencia.cs    # referência à zona (não FK cross-API)
│   └── DTOs/
│       ├── AlertaCreateDTO.cs
│       ├── AlertaResponseDTO.cs
│       └── EstatisticasDTO.cs
│
├── Data/
│   ├── AppDbContext.cs           # DbContext com Oracle
│   └── Migrations/               # geradas via dotnet ef migrations add
│
├── Services/
│   └── AlertaService.cs
│
└── Exceptions/
    └── GlobalExceptionMiddleware.cs
```

### Entidades e relacionamento 1:N obrigatório
```csharp
// AlertaHistorico.cs — entidade principal
public class AlertaHistorico
{
    public int Id { get; set; }                      // PK
    public int ZonaId { get; set; }                  // FK → ZonaReferencia
    public string NivelAlerta { get; set; }          // ATENCAO / ALERTA / EMERGENCIA
    public double ScoreRegistrado { get; set; }
    public double No2Registrado { get; set; }
    public string TextoRecomendacao { get; set; }
    public DateTime DtAlerta { get; set; } = DateTime.UtcNow;
    public bool Confirmado { get; set; } = false;

    // Relacionamento N:1 → ZonaReferencia (1 zona tem N alertas)
    public ZonaReferencia Zona { get; set; }
}

// ZonaReferencia.cs — lado "1" do relacionamento
public class ZonaReferencia
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Municipio { get; set; } = "São Paulo";

    // Relacionamento 1:N → AlertaHistorico
    public ICollection<AlertaHistorico> Alertas { get; set; } = new List<AlertaHistorico>();
}
```

### DbContext com Oracle
```csharp
// AppDbContext.cs
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<AlertaHistorico> AlertasHistorico { get; set; }
    public DbSet<ZonaReferencia> ZonasReferencia { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlertaHistorico>(e => {
            e.ToTable("ALERTA_HISTORICO");
            e.HasKey(a => a.Id);
            e.Property(a => a.NivelAlerta).HasMaxLength(15).IsRequired();
            e.Property(a => a.TextoRecomendacao).HasMaxLength(1000);
            e.HasOne(a => a.Zona)
             .WithMany(z => z.Alertas)
             .HasForeignKey(a => a.ZonaId)
             .OnDelete(DeleteBehavior.Restrict); // não cascade
        });

        modelBuilder.Entity<ZonaReferencia>(e => {
            e.ToTable("ZONA_REFERENCIA_NET");
            e.HasKey(z => z.Id);
            e.Property(z => z.Nome).HasMaxLength(100).IsRequired();
        });
    }
}
```

### Endpoints .NET
```
POST   /api/alertas
       Body: { "zonaId": 1, "nivelAlerta": "ALERTA", "scoreRegistrado": 38.5,
               "no2Registrado": 41.2, "textoRecomendacao": "..." }
       → 201 Created com o alerta criado

GET    /api/alertas?zonaId=1&dias=30
       → 200 OK: lista de alertas recentes da zona

GET    /api/alertas/{id}
       → 200 OK | 404 Not Found

PUT    /api/alertas/{id}/confirmar
       Body: { "confirmado": true }
       → 200 OK

DELETE /api/alertas/{id}
       → 204 No Content

GET    /api/estatisticas/zona/{zonaId}
       → { "totalAlertas30dias": 12, "diasCriticos": 3,
            "mediaScore": 58.4, "piorDia": "2026-05-15" }
```

### appsettings.json
```json
{
  "ConnectionStrings": {
    "Oracle": "User Id=${DB_USER};Password=${DB_PASS};Data Source=localhost:1521/XEPDB1;"
  },
  "Jwt": {
    "Secret": "${JWT_SECRET}",
    "Issuer": "pulso-urbano-java",
    "Audience": "pulso-urbano-clients"
  },
  "AllowedHosts": "*",
  "Logging": {
    "LogLevel": { "Default": "Information" }
  }
}
```

### Program.cs (estrutura mínima)
```csharp
var builder = WebApplication.CreateBuilder(args);

// Oracle EF Core
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseOracle(builder.Configuration.GetConnectionString("Oracle")));

// Services
builder.Services.AddScoped<AlertaService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Controllers
builder.Services.AddControllers();

// CORS — permite mobile e Java chamarem
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();
app.Run();
```

### Migration commands (Clayton executa)
```bash
# Criar migration inicial
dotnet ef migrations add InitialCreate --output-dir Data/Migrations

# Aplicar no banco Oracle
dotnet ef database update

# Adicionar nova migration (se schema mudar)
dotnet ef migrations add AddCampoX
dotnet ef database update
```

---

## ORACLE DATABASE — SCHEMA COMPARTILHADO

> **Dono:** Clayton. Ambas as APIs (Java + .NET) conectam no mesmo Oracle.
> Java gerencia: USUARIO, ZONA_CIDADE, LEITURA_SATELITE, SCORE_DIARIO, RECOMENDACAO, LOG_CONSULTA
> .NET gerencia: ALERTA_HISTORICO, ZONA_REFERENCIA_NET

### DDL completo
```sql
-- Sequences para PKs
CREATE SEQUENCE seq_usuario      START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_zona         START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_leitura      START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_score        START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_recomendacao START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_log          START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_alerta       START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE seq_zona_net     START WITH 1 INCREMENT BY 1;

-- Tabela 1: Usuários
CREATE TABLE usuario (
  id_usuario           NUMBER DEFAULT seq_usuario.NEXTVAL PRIMARY KEY,
  nome                 VARCHAR2(150)  NOT NULL,
  email                VARCHAR2(200)  NOT NULL UNIQUE,
  hash_senha           VARCHAR2(255)  NOT NULL,
  faz_exercicio        NUMBER(1)      DEFAULT 0,
  tem_crianca          NUMBER(1)      DEFAULT 0,
  tem_problema_resp    NUMBER(1)      DEFAULT 0,
  role                 VARCHAR2(20)   DEFAULT 'USER',
  dt_criacao           DATE           DEFAULT SYSDATE,
  ativo                NUMBER(1)      DEFAULT 1,
  CONSTRAINT chk_role CHECK (role IN ('USER', 'ADMIN'))
);

-- Tabela 2: Zonas da cidade monitoradas
CREATE TABLE zona_cidade (
  id_zona    NUMBER DEFAULT seq_zona.NEXTVAL PRIMARY KEY,
  nome       VARCHAR2(100) NOT NULL,
  municipio  VARCHAR2(100) DEFAULT 'São Paulo',
  lat        NUMBER(9,6),
  lon        NUMBER(9,6),
  ativo      NUMBER(1) DEFAULT 1
);

-- Tabela 3: Leituras orbitais ingeridas
CREATE TABLE leitura_satelite (
  id_leitura   NUMBER DEFAULT seq_leitura.NEXTVAL PRIMARY KEY,
  id_zona      NUMBER NOT NULL,
  satelite     VARCHAR2(50)   NOT NULL,  -- 'Sentinel-5P', 'ECOSTRESS'
  tipo_dado    VARCHAR2(30)   NOT NULL,  -- 'NO2', 'TEMP_SUPERFICIE', 'UV'
  valor        NUMBER(10,4)   NOT NULL,
  unidade      VARCHAR2(20),             -- 'ppb', '°C', 'W/m²'
  dt_captura   DATE           NOT NULL,
  dt_ingestao  DATE           DEFAULT SYSDATE,
  CONSTRAINT fk_leitura_zona FOREIGN KEY (id_zona) REFERENCES zona_cidade(id_zona)
);

-- Tabela 4: Scores calculados por zona/dia
CREATE TABLE score_diario (
  id_score      NUMBER DEFAULT seq_score.NEXTVAL PRIMARY KEY,
  id_zona       NUMBER NOT NULL,
  dt_score      DATE   NOT NULL,
  valor_score   NUMBER(5,2) NOT NULL,
  classificacao VARCHAR2(15) NOT NULL,   -- BOM, MODERADO, RUIM, CRITICO
  no2_valor     NUMBER(8,4),
  temp_valor    NUMBER(6,2),
  CONSTRAINT fk_score_zona  FOREIGN KEY (id_zona)  REFERENCES zona_cidade(id_zona),
  CONSTRAINT chk_score_val  CHECK (valor_score BETWEEN 0 AND 100),
  CONSTRAINT chk_classif    CHECK (classificacao IN ('BOM','MODERADO','RUIM','CRITICO'))
);

-- Tabela 5: Recomendações entregues aos usuários
CREATE TABLE recomendacao (
  id_rec      NUMBER DEFAULT seq_recomendacao.NEXTVAL PRIMARY KEY,
  id_score    NUMBER NOT NULL,
  id_usuario  NUMBER NOT NULL,
  texto       VARCHAR2(1000) NOT NULL,
  icone       VARCHAR2(30),
  dt_entrega  DATE DEFAULT SYSDATE,
  CONSTRAINT fk_rec_score   FOREIGN KEY (id_score)   REFERENCES score_diario(id_score),
  CONSTRAINT fk_rec_usuario FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario)
);

-- Tabela 6: Log de consultas (auditoria)
CREATE TABLE log_consulta (
  id_log      NUMBER DEFAULT seq_log.NEXTVAL PRIMARY KEY,
  id_usuario  NUMBER,
  id_zona     NUMBER,
  endpoint    VARCHAR2(200),
  ip_origem   VARCHAR2(45),
  dt_consulta DATE DEFAULT SYSDATE,
  CONSTRAINT fk_log_usuario FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario),
  CONSTRAINT fk_log_zona    FOREIGN KEY (id_zona)    REFERENCES zona_cidade(id_zona)
);

-- Tabela 7: Alertas (.NET — EF Core cria automaticamente via migration)
-- (não criar manualmente — deixar EF Core criar via dotnet ef database update)
```

### Procedures PL/SQL obrigatórias
```sql
-- Procedure 1: Calcula e persiste score (chamada pelo scheduler Java)
CREATE OR REPLACE PROCEDURE calcular_score_zona(p_zona_id IN NUMBER) AS
  v_no2   NUMBER;
  v_temp  NUMBER;
  v_score NUMBER;
  v_class VARCHAR2(15);
BEGIN
  SELECT valor INTO v_no2 FROM leitura_satelite
  WHERE id_zona = p_zona_id AND tipo_dado = 'NO2'
  ORDER BY dt_captura DESC FETCH FIRST 1 ROWS ONLY;

  SELECT valor INTO v_temp FROM leitura_satelite
  WHERE id_zona = p_zona_id AND tipo_dado = 'TEMP_SUPERFICIE'
  ORDER BY dt_captura DESC FETCH FIRST 1 ROWS ONLY;

  -- Mesmo algoritmo do Java (consistência obrigatória)
  v_score := (GREATEST(0, 1 - v_no2/50) * 0.60
            + GREATEST(0, 1 - GREATEST(0, (v_temp-30)/20)) * 0.40) * 100;
  v_score := ROUND(v_score, 1);

  SELECT CASE
    WHEN v_score >= 80 THEN 'BOM'
    WHEN v_score >= 60 THEN 'MODERADO'
    WHEN v_score >= 40 THEN 'RUIM'
    ELSE 'CRITICO'
  END INTO v_class FROM DUAL;

  INSERT INTO score_diario
    (id_score, id_zona, dt_score, valor_score, classificacao, no2_valor, temp_valor)
  VALUES
    (seq_score.NEXTVAL, p_zona_id, TRUNC(SYSDATE), v_score, v_class, v_no2, v_temp);

  COMMIT;
EXCEPTION
  WHEN NO_DATA_FOUND THEN
    DBMS_OUTPUT.PUT_LINE('Sem dados para zona ' || p_zona_id);
  WHEN OTHERS THEN
    ROLLBACK;
    RAISE;
END;
/

-- Procedure 2: Registra recomendação no banco
CREATE OR REPLACE PROCEDURE registrar_recomendacao(
  p_score_id   IN NUMBER,
  p_usuario_id IN NUMBER,
  p_texto      IN VARCHAR2,
  p_icone      IN VARCHAR2
) AS
BEGIN
  INSERT INTO recomendacao (id_rec, id_score, id_usuario, texto, icone)
  VALUES (seq_recomendacao.NEXTVAL, p_score_id, p_usuario_id, p_texto, p_icone);
  COMMIT;
EXCEPTION
  WHEN OTHERS THEN ROLLBACK; RAISE;
END;
/

-- Trigger 1: Loga toda consulta de score
CREATE OR REPLACE TRIGGER trg_log_score_consulta
AFTER INSERT ON score_diario
FOR EACH ROW
BEGIN
  INSERT INTO log_consulta (id_log, id_zona, endpoint, dt_consulta)
  VALUES (seq_log.NEXTVAL, :NEW.id_zona, 'SCHEDULER/calcular_score', SYSDATE);
END;
/

-- Trigger 2: Valida score antes de inserir
CREATE OR REPLACE TRIGGER trg_valida_score
BEFORE INSERT ON score_diario
FOR EACH ROW
BEGIN
  IF :NEW.valor_score < 0 OR :NEW.valor_score > 100 THEN
    RAISE_APPLICATION_ERROR(-20001, 'Score fora do range 0-100: ' || :NEW.valor_score);
  END IF;
END;
/
```

---

## DOCKER — INFRA COMPARTILHADA

> **Dono:** Clayton.

```yaml
# docker-compose.yml na raiz do monorepo
version: '3.8'

services:

  oracle:
    image: gvenzl/oracle-xe:21-slim
    container_name: pulso-oracle-562999
    ports:
      - "1521:1521"
    environment:
      ORACLE_PASSWORD: ${DB_PASS:-oracle}
      ORACLE_DATABASE: XEPDB1
    volumes:
      - oracle-data:/opt/oracle/oradata
      - ./database/init.sql:/container-entrypoint-initdb.d/init.sql
    networks:
      - pulso-net
    healthcheck:
      test: ["CMD", "healthcheck.sh"]
      interval: 30s
      timeout: 10s
      retries: 5

  java-api:
    build:
      context: ./pulso-java
      dockerfile: Dockerfile
    container_name: pulso-java-562999
    ports:
      - "8080:8080"
    environment:
      - DB_USER=system
      - DB_PASS=${DB_PASS:-oracle}
      - JWT_SECRET=${JWT_SECRET:-pulso-secret-2026}
      - COPERNICUS_USER=${COPERNICUS_USER}
      - COPERNICUS_PASS=${COPERNICUS_PASS}
      - NASA_EARTHDATA_TOKEN=${NASA_EARTHDATA_TOKEN}
    depends_on:
      oracle:
        condition: service_healthy
    networks:
      - pulso-net
    user: "1001"
    working_dir: /app

  dotnet-api:
    build:
      context: ./pulso-dotnet
      dockerfile: Dockerfile
    container_name: pulso-dotnet-562999
    ports:
      - "5000:5000"
    environment:
      - DB_USER=system
      - DB_PASS=${DB_PASS:-oracle}
      - JWT_SECRET=${JWT_SECRET:-pulso-secret-2026}
      - ASPNETCORE_URLS=http://+:5000
    depends_on:
      oracle:
        condition: service_healthy
    networks:
      - pulso-net
    user: "1001"
    working_dir: /app

networks:
  pulso-net:
    driver: bridge

volumes:
  oracle-data:
    name: pulso-oracle-data
```

```dockerfile
# pulso-java/Dockerfile
FROM eclipse-temurin:21-jre-alpine
WORKDIR /app
RUN addgroup -S pulso && adduser -S pulso -G pulso
USER pulso
COPY target/pulso-urbano-*.jar app.jar
EXPOSE 8080
ENV JAVA_OPTS="-Xmx512m -Xms256m"
ENTRYPOINT ["sh", "-c", "java $JAVA_OPTS -jar app.jar"]
```

```dockerfile
# pulso-dotnet/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
RUN addgroup -S pulso && adduser -S pulso -G pulso
COPY --from=build /app/publish .
USER pulso
EXPOSE 5000
ENTRYPOINT ["dotnet", "PulsoUrbano.Net.dll"]
```

---

## REGRAS ABSOLUTAS DO PROJETO

Essas regras valem para qualquer LLM trabalhando neste projeto. Não ignore nenhuma.

### O que NUNCA fazer
```
❌ Não use LLM/OpenAI/Gemini para gerar texto de recomendação — use templates if/elif
❌ Não implemente rota saudável por rua — resolução do satélite não suporta
❌ Não crie dashboard web separado — o frontend é apenas mobile (React Native)
❌ Não use localhost como URL de deploy — a entrega precisa de URL pública acessível
❌ Não duplique responsabilidades Java/NET — cada API tem seu domínio
❌ Não use dados mockados para o score se Copernicus estiver disponível
❌ Não use ddl-auto=create-drop em produção — use validate
❌ Não execute container como root — sempre USER pulso/appuser
❌ Não processe arquivo NetCDF raw — use produtos Level-3 pré-processados da Copernicus
❌ Não adicione features ao escopo sem aprovação do Felipe (tech lead)
```

### O que SEMPRE fazer
```
✅ Usar variáveis de ambiente para credenciais — nunca hardcode
✅ Tratar exceção de API orbital sem derrubar o sistema (catch + log + continue)
✅ Documentar todo endpoint no Swagger
✅ Usar DTOs (Records) — nunca expor entidade JPA diretamente na response
✅ Adicionar HATEOAS nos endpoints Java (pelo menos _links: self + related)
✅ Validar entrada com @Valid + @NotNull/@Size nos controllers Java
✅ Ter fallback para Open-Meteo se Copernicus não responder
✅ Incluir nome do container com RM 562999 (regra de pontuação DevOps)
✅ Commitar docker-compose.yml e Dockerfiles no repositório
```

---

## SOBRE OS ARQUIVOS DA GS

Quando o usuário enviar os arquivos oficiais da GS (PDF com entregas por disciplina), use-os para:

1. **Verificar requisitos técnicos por disciplina** — cada disciplina tem critérios específicos de avaliação (ex: Java exige HATEOAS, .NET exige Migration, DevOps exige volume nomeado)

2. **Validar o escopo** — se uma feature não está nos requisitos de nenhuma disciplina, ela não deve ser implementada

3. **Mapear entregas** — cada disciplina tem entrega no portal (70%) + apresentação (30%). As specs acima já foram calibradas para atender todos os critérios

4. **Penalidades a evitar** (extraídas do documento GS):
   - DevOps: solução em localhost → zero na disciplina
   - DevOps: sem Dockerfile ou docker-compose → zero
   - Mobile: app não executa após git clone → -50 pontos
   - .NET: sem migration → perda significativa de nota
   - Java: sem deploy público acessível → perda de pontos em documentação

5. **Critérios de premiação** — para concorrer ao prize (shape + camiseta), precisa ≥9.0 em TODAS as disciplinas + nota ≥9.0 no vídeo pitch Java Advanced

---

## ESTADO ATUAL DO PROJETO

```
Decisão: TOMADA — Pulso Urbano é o projeto
Stack: Java Spring Boot + .NET Core + Oracle + React Native + ESP32 Wokwi
Dados: Sentinel-5P + ECOSTRESS + Open-Meteo
Município alvo: São Paulo
Features cortadas (definitivo): rota saudável, LLM, dashboard web

Status por pessoa:
  Felipe   → [  ] Setup repositório  [  ] Conta Copernicus validada
  Clayton  → [  ] DDL Oracle criado  [  ] Docker Compose local
  Guilherme→ [  ] Expo setup         [  ] Estrutura de pastas
  Bosak    → [  ] Archi instalado    [  ] Stakeholders mapeados
  Brisola  → [  ] Wokwi criado       [  ] Circuito ESP32 montado
```

---

## COMO USAR ESTE CONTEXTO EM DIFERENTES LLMs

### Claude (claude.ai ou Claude Code)
- Salve este arquivo como `CLAUDE.md` na raiz do monorepo
- Claude Code lê automaticamente ao iniciar sessão na pasta
- Para sessões web: cole o arquivo inteiro antes de qualquer pergunta

### ChatGPT / GPT-4
- Cole como primeira mensagem do chat
- Comece com: "Leia este contexto e confirme que entendeu antes de responder qualquer pergunta:"

### Gemini / Copilot / outros
- Cole como bloco de contexto inicial
- Adicione ao final: "Agora responda apenas perguntas relacionadas a este projeto."

### Para sessões específicas de código
Após colar o contexto, especifique a tarefa assim:
```
CONTEXTO: [cole o arquivo acima]

TASK: [descreva o que precisa — ex: "Implementar o CopernicusApiService.java
que autentica via OAuth2 e busca o produto Sentinel-5P mais recente para
as coordenadas de São Paulo (-23.5505, -46.6333)"]

RESTRIÇÕES: [o que não pode mudar — ex: "Usar RestTemplate, não WebClient.
Seguir a estrutura de pacotes do contexto."]

ENTREGUE: apenas o código do arquivo. Sem explicação desnecessária.
```

---

*Arquivo gerado em 27/05/2026 · Versão 1.0*  
*Manter atualizado conforme o projeto evolui — especialmente a seção "Estado atual"*
