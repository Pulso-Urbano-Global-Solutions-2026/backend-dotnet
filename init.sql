-- =============================================================================
--  Pulso Urbano — .NET API (ASP.NET Core 10, porta 5000)
--  Schema: tabelas de propriedade exclusiva desta API
--
--  DIVISÃO DE DOMÍNIOS (nunca duplicar):
--    Java API  →  USUARIO, ZONA_CIDADE, LEITURA_SATELITE,
--                 SCORE_DIARIO, RECOMENDACAO, LOG_CONSULTA
--    .NET API  →  ZONA_REFERENCIA_NET, ALERTA_HISTORICO  (este arquivo)
--
--  ZONA_REFERENCIA_NET é uma tabela local desta API que espelha as zonas
--  relevantes vindas da Java API (sync via DataSeeder). Não há FK cruzada
--  entre as APIs — cada lado mantém sua própria integridade referencial.
--
--  Em produção este script é executado uma única vez antes de
--  `dotnet ef database update`. Em testes, EF Core (SQLite in-memory)
--  cria o schema automaticamente via EnsureCreated().
-- =============================================================================

-- ── Sequences HiLo (incrementBy: 10, padrão EF Core Oracle) ─────────────────

CREATE SEQUENCE SEQ_ZONA_REFERENCIA
    START WITH 1
    INCREMENT BY 10
    NOCACHE
    NOCYCLE;

CREATE SEQUENCE SEQ_ALERTA_HISTORICO
    START WITH 1
    INCREMENT BY 10
    NOCACHE
    NOCYCLE;

-- ── ZONA_REFERENCIA_NET ──────────────────────────────────────────────────────
--  Espelho local das zonas de São Paulo. Populada pelo DataSeeder na
--  inicialização; sincronizada com ZONA_CIDADE do Java API via seed determinístico
--  (Random seed 562999 — mesmo seed dos dois lados).

CREATE TABLE ZONA_REFERENCIA_NET (
    ID_ZONA   NUMBER(10)    NOT NULL,
    NOME      NVARCHAR2(100) NOT NULL,
    MUNICIPIO NVARCHAR2(100) NOT NULL,
    CONSTRAINT PK_ZONA_REFERENCIA_NET PRIMARY KEY (ID_ZONA)
);

-- ── ALERTA_HISTORICO ─────────────────────────────────────────────────────────
--  Histórico de alertas gerados pelo .NET API.
--  NIVEL_ALERTA: BOM | MODERADO | RUIM | CRITICO  (espelha ClassificacaoScore do Java)
--  SCORE_REGISTRADO: 0.0 – 100.0
--  NO2_REGISTRADO: ppb
--  CONFIRMADO: 0 = pendente, 1 = confirmado

CREATE TABLE ALERTA_HISTORICO (
    ID_ALERTA          NUMBER(10)      NOT NULL,
    ID_ZONA            NUMBER(10)      NOT NULL,
    NIVEL_ALERTA       NVARCHAR2(15)   NOT NULL,
    SCORE_REGISTRADO   NUMBER(5,2)     NOT NULL,
    NO2_REGISTRADO     NUMBER(8,4)     NOT NULL,
    TEXTO_RECOMENDACAO NVARCHAR2(1000) NOT NULL,
    DT_ALERTA          DATE            NOT NULL,
    CONFIRMADO         NUMBER(1)       NOT NULL,
    CONSTRAINT PK_ALERTA_HISTORICO
        PRIMARY KEY (ID_ALERTA),
    CONSTRAINT FK_ALERTA_HISTORICO_ZONA
        FOREIGN KEY (ID_ZONA)
        REFERENCES ZONA_REFERENCIA_NET (ID_ZONA)
        ON DELETE RESTRICT
);

-- ── Índice composto (zona + data) para queries de estatísticas ───────────────

CREATE INDEX IX_ALERTA_ZONA_DT
    ON ALERTA_HISTORICO (ID_ZONA, DT_ALERTA);

COMMIT;
