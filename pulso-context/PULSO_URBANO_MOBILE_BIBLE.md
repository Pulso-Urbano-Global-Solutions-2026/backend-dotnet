# PULSO URBANO — BÍBLIA DO FRONT-END MOBILE
# Contexto completo para qualquer agente de IA desenvolver o app React Native
# Versão 1.0 · 27/05/2026 · Owner: Felipe Ferrete (RM 562999)

> **Como usar este documento:**
> - Para Claude Code: salvar como `CLAUDE.md` na raiz de `pulso-mobile/`
> - Para sessões web (ChatGPT, Claude.ai, Gemini): colar inteiro como primeira mensagem
> - Para tasks específicas: colar apenas as seções relevantes (cada uma é autocontida)

---

## 📑 ÍNDICE

### PARTE 1 — VISÃO E REGRAS
1. [Identidade do projeto](#1-identidade-do-projeto)
2. [O problema e a solução](#2-o-problema-e-a-solução)
3. [Arquitetura macro do sistema](#3-arquitetura-macro-do-sistema)
4. [Posição do mobile na arquitetura](#4-posição-do-mobile-na-arquitetura)
5. [Cronograma crítico do mobile](#5-cronograma-crítico-do-mobile)
6. [Regras absolutas — NUNCA / SEMPRE](#6-regras-absolutas)

### PARTE 2 — STACK E ARQUITETURA TÉCNICA
7. [Stack obrigatória](#7-stack-obrigatória)
8. [Versões travadas de dependências](#8-versões-travadas)
9. [Estrutura de pastas](#9-estrutura-de-pastas)
10. [Convenções de código e nomenclatura](#10-convenções-de-código)
11. [Convenções de Git e commits](#11-convenções-git)

### PARTE 3 — CONTRATOS COM A API
12. [URL base e configuração de ambiente](#12-url-base)
13. [Autenticação JWT — fluxo completo](#13-autenticação-jwt)
14. [Catálogo de endpoints com JSON literal](#14-catálogo-de-endpoints)
15. [Mapeamento de erros HTTP](#15-mapeamento-de-erros)
16. [Mock API local (MSW) — desenvolvimento sem backend](#16-mock-api-local)

### PARTE 4 — DESIGN SYSTEM
17. [Tokens visuais (cores, tipografia, espaçamento)](#17-tokens-visuais)
18. [Componentes reutilizáveis](#18-componentes-reutilizáveis)
19. [Iconografia](#19-iconografia)
20. [Acessibilidade](#20-acessibilidade)

### PARTE 5 — TELAS E NAVEGAÇÃO
21. [Mapa de navegação completo](#21-mapa-de-navegação)
22. [Specs detalhadas das 7 telas](#22-specs-das-telas)
23. [Estados universais (loading, erro, vazio)](#23-estados-universais)

### PARTE 6 — FEATURES CRÍTICAS
24. [Autenticação: secure storage + interceptor](#24-autenticação)
25. [Geolocalização: permissão + fallback](#25-geolocalização)
26. [Mapa: react-native-maps setup](#26-mapa)
27. [Forms: react-hook-form + zod](#27-forms)
28. [Estado global: Context API](#28-estado-global)

### PARTE 7 — QUALIDADE
29. [Performance e otimizações obrigatórias](#29-performance)
30. [Testes: Jest + RNTL](#30-testes)
31. [Lint, format, type-check](#31-lint-format)

### PARTE 8 — DEVOPS MOBILE
32. [Setup pós-`git clone` (PROIBIDO falhar)](#32-setup-pós-clone)
33. [Build: Expo Go vs Development Build vs EAS](#33-build)
34. [README obrigatório](#34-readme)

### PARTE 9 — RUBRICA FIAP MAPEADA
35. [Cobertura de cada critério](#35-rubrica-fiap)
36. [Penalidades e como evitá-las](#36-penalidades)
37. [Apresentação presencial (30%)](#37-apresentação)

### PARTE 10 — PROMPTS PRONTOS
38. [Template universal de task mobile](#38-template-task)
39. [Prompts por feature](#39-prompts-feature)

---

# PARTE 1 — VISÃO E REGRAS

## 1. Identidade do projeto

| Campo | Valor |
|---|---|
| **Produto** | Pulso Urbano |
| **Contexto acadêmico** | Global Solution 2026/1 — FIAP · ADS 2º ano · Turmas de Fevereiro |
| **Tema da GS** | Economia Espacial — uso de infraestrutura orbital para resolver problemas reais na Terra |
| **Disciplina coberta** | Mobile Application Development |
| **Prazo entrega** | **09/06/2026 até 23h55** (portal FIAP) |
| **Apresentação presencial** | 10–16/06/2026 |
| **Owner mobile** | Guilherme (a confirmar nome completo + RM) |
| **Tech lead geral** | Felipe Ferrete — RM 562999 |
| **Repositório** | GitHub Classroom (obrigatório, -20 pts se não usar) |
| **Pontuação total** | 100 pts (70 entrega + 30 apresentação) |

---

## 2. O problema e a solução

### O problema (transcrição literal do CONTEXT.md do backend)

Todo dia, milhões de pessoas em São Paulo tomam decisões sem informação ambiental adequada:

- *"Vou correr agora?"* → sem saber que o NO₂ está 3× acima do limite da OMS
- *"Levo meu filho a pé?"* → sem saber que a temperatura de superfície está em 48°C
- *"Saio de bike?"* → sem saber que em 2h a qualidade do ar melhora

**O dado já existe.** O satélite Sentinel-5P da ESA mede NO₂ em São Paulo diariamente, de graça. O ECOSTRESS da NASA mede a temperatura real do asfalto, de graça. O que falta é o sistema que transforma esse dado orbital em decisão humana.

### A solução

**Pulso Urbano** é um app mobile que transforma dados orbitais reais em uma frase simples:

> *"Hoje o ar está moderado. Prefira sair antes das 10h — especialmente se você corre ou tem filhos."*

Sem jargão técnico. Sem dashboard GIS. Um score (0–100). Uma recomendação personalizada. Um mapa.

### Por que o mobile importa para a tese "Economia Espacial"

O dado de satélite é inerte. Ele só vira valor quando alguém o consome e age. **O mobile é o canal de consumo desse dado.** Sem mobile, o produto não existe — sobra um dashboard que ninguém abre.

---

## 3. Arquitetura macro do sistema

```
┌─────────────────────────────────────────────────────┐
│                   FONTES ORBITAIS                    │
│  Sentinel-5P (ESA) · ECOSTRESS (NASA) · Open-Meteo  │
└──────────────┬──────────────────────────────────────┘
               │ OAuth2 / REST (consumido pela Java API)
               ▼
┌──────────────────────────┐   ┌──────────────────────┐
│   JAVA API (PRIMÁRIA)    │   │   .NET API (SEC.)     │
│   Spring Boot 3.2        │   │   ASP.NET Core 8      │
│   Porta: 8080            │   │   Porta: 5000         │
│   ★ MOBILE CONSOME ESTA  │   │   (não usar no MVP)   │
└──────────┬───────────────┘   └──────────────────────┘
           │ HTTP/JSON
           ▼
┌────────────────────────────────────────┐
│       MOBILE — REACT NATIVE (Expo)     │
│       ★ ESSE PROJETO ★                 │
│                                        │
│       Telas: Login, Home, Mapa,        │
│              Histórico, Perfil...      │
└────────────────────────────────────────┘
```

---

## 4. Posição do mobile na arquitetura

### O que o mobile FAZ
- Captura geolocalização do usuário (lat/lon)
- Consulta `GET /api/v1/score/current?lat=X&lon=Y` da Java API
- Mostra score + recomendação textual personalizada
- Mostra mapa com pins de zonas e seus scores
- Mostra histórico de scores dos últimos 7 dias
- Permite registro/login com JWT
- Permite editar perfil (flags `fazExercicio`, `temCrianca`, `temProblemaResp`)

### O que o mobile **NÃO** faz
- ❌ NÃO consome diretamente APIs orbitais (Sentinel, NASA, Open-Meteo) — quem faz é a Java API
- ❌ NÃO calcula score localmente — quem calcula é o backend
- ❌ NÃO armazena dado orbital — só cacheia score já calculado
- ❌ NÃO usa LLM/IA para gerar recomendação — backend já entrega texto pronto
- ❌ NÃO implementa "rota saudável" — fora do escopo (resolução de satélite não suporta)
- ❌ NÃO conversa com a .NET API — `.NET` é domínio de alertas históricos, não exposto ao mobile no MVP

### Contrato de dependência: o mobile depende da Java API

Toda funcionalidade do mobile que envolve dado depende de pelo menos um endpoint da Java API. Mas, **crítico:** a Java API só estará deployada em ~06/06. O mobile **deve começar contra mock local** (ver seção [16](#16-mock-api-local)) e só plugar a API real na última semana.

---

## 5. Cronograma crítico do mobile

```
DIA 1 (27/05): Setup Expo + estrutura de pastas + theme + Mock API rodando
DIA 2 (28/05): Login + Register telas (mock auth)
DIA 3 (29/05): Home (score atual) + AuthContext + interceptor Axios
DIA 4 (30/05): Mapa com react-native-maps + pins de zonas
DIA 5 (31/05): Histórico + gráfico simples
DIA 6 (01/06): Perfil (CRUD usuario) + logout
DIA 7 (02/06): Detalhes da zona ao tocar pin + Recomendação personalizada
DIA 8 (03/06): Polimento visual + empty states + error states
DIA 9 (04/06): ★ INTEGRAÇÃO REAL com Java API (se backend disponível)
DIA 10 (05/06): Testes manuais + correção de bugs
DIA 11 (06/06): Build EAS + teste no celular físico
DIA 12 (07/06): Buffer para regressões
DIA 13 (08/06): Vídeo de até 5 min com todas as funcionalidades
ENTREGA (09/06 23h55) — Portal FIAP + GitHub Classroom
```

### Gates de decisão

- **04/06 (DIA 9):** Java API funciona em URL pública? Se NÃO, manter mock até última hora e documentar isso no README como "limitação assumida da janela da GS"
- **07/06 (DIA 12):** App roda do zero após `git clone`? Se NÃO, parar tudo e debugar — é -50 pts.

---

## 6. Regras absolutas

### ❌ NUNCA
```
❌ NUNCA commitar token JWT em código
❌ NUNCA usar AsyncStorage para guardar JWT (use expo-secure-store)
❌ NUNCA chamar APIs externas direto do app (sempre via Java API)
❌ NUNCA hardcodar URLs (sempre via constants/env)
❌ NUNCA ignorar erro de rede (sempre Alert ou Toast)
❌ NUNCA armazenar senha em texto claro nem em memória após login
❌ NUNCA usar `any` em TypeScript (sempre tipar — vale ponto em arquitetura)
❌ NUNCA fazer commit "WIP" ou "ajustes" sem contexto (-40 pts: histórico Git confuso)
❌ NUNCA bloquear UI thread (sempre async/await + loading state)
❌ NUNCA esquecer empty state em listas (sempre "Nenhum dado disponível")
❌ NUNCA renderizar erro genérico ("Algo deu errado") — sempre mensagem útil
❌ NUNCA esquecer keyExtractor em FlatList (warning + degrade performance)
❌ NUNCA tocar no node_modules manualmente
❌ NUNCA upar build (.apk/.ipa) no repositório
```

### ✅ SEMPRE
```
✅ SEMPRE travar versões no package.json (sem ^ ou ~)
✅ SEMPRE testar `npm install && npx expo start` em pasta limpa antes de commitar
✅ SEMPRE adicionar tipo TypeScript explícito em props
✅ SEMPRE usar componentes do design system (não estilizar inline ad-hoc)
✅ SEMPRE adicionar testID em elementos críticos para teste
✅ SEMPRE refletir loading + erro + vazio em toda tela que faz fetch
✅ SEMPRE mostrar feedback ao usuário em ações (toast, alert, loading)
✅ SEMPRE limpar token e estado ao logout
✅ SEMPRE renovar token quando 401 (interceptor)
✅ SEMPRE pedir permissão de geolocalização ANTES de chamar a API que precisa dela
✅ SEMPRE escrever commit no padrão Conventional Commits
✅ SEMPRE rodar o app no celular físico antes da entrega (não só emulador)
```

---

# PARTE 2 — STACK E ARQUITETURA TÉCNICA

## 7. Stack obrigatória

| Camada | Tecnologia | Versão | Por quê |
|---|---|---|---|
| Framework | **Expo SDK** | 52 | Reduz risco de "não roda após clone" (-50 pts) |
| Linguagem | **TypeScript** | 5.3.x | +pontos em arquitetura, LLMs erram menos |
| Navegação | **Expo Router** | 4.x | File-based routing, padrão Expo SDK 52 |
| HTTP | **Axios** | 1.7.x | Interceptor JWT mais limpo que fetch |
| Token seguro | **expo-secure-store** | 13.x | Cifrado no Keychain (iOS) / Keystore (Android) |
| Cache de dados | **AsyncStorage** | 2.x | Para dados não-sensíveis (último score em cache) |
| Forms | **react-hook-form** + **zod** | 7.x / 3.x | Validação tipada + performance |
| Mapa | **react-native-maps** | 1.18.x | Compatível com Expo via prebuild |
| Geolocalização | **expo-location** | 17.x | API oficial Expo |
| Ícones | **@expo/vector-icons** | 14.x | Já vem com Expo, MaterialIcons |
| Gráfico (opcional) | **react-native-chart-kit** | 6.x | Só se sobrar tempo para tela de histórico |
| Toast | **react-native-toast-message** | 2.x | Feedback de ações |
| Mock API | **msw** + **@mswjs/native** | 2.x | Mesmos contratos da Java API |
| Testes | **Jest** + **@testing-library/react-native** | 29 / 12 | Padrão React Native |
| Lint | **ESLint** + **eslint-config-expo** | 8.x | Configuração oficial Expo |
| Format | **Prettier** | 3.x | Formatação consistente |

### O que NÃO usar (decisão final, sem discussão)

- ❌ Redux / Redux Toolkit (overkill para 5–7 telas — use Context)
- ❌ NativeWind / TailwindCSS RN (mais uma dependência que pode quebrar build)
- ❌ React Navigation puro (use Expo Router que já abstrai)
- ❌ Fetch direto (sempre via axios)
- ❌ React Native CLI puro (use Expo)
- ❌ TanStack Query / SWR (Context + axios bastam aqui)

---

## 8. Versões travadas

**Arquivo `package.json` — copiar exatamente:**

```json
{
  "name": "pulso-urbano-mobile",
  "version": "1.0.0",
  "main": "expo-router/entry",
  "scripts": {
    "start": "expo start",
    "android": "expo start --android",
    "ios": "expo start --ios",
    "web": "expo start --web",
    "test": "jest --watchAll",
    "test:ci": "jest --ci --coverage",
    "lint": "eslint . --ext .ts,.tsx",
    "type-check": "tsc --noEmit",
    "format": "prettier --write \"**/*.{ts,tsx,json,md}\""
  },
  "dependencies": {
    "expo": "52.0.11",
    "expo-router": "4.0.9",
    "expo-status-bar": "2.0.0",
    "expo-secure-store": "14.0.0",
    "expo-location": "18.0.2",
    "expo-constants": "17.0.3",
    "expo-linking": "7.0.3",
    "expo-splash-screen": "0.29.13",
    "expo-font": "13.0.1",
    "react": "18.3.1",
    "react-native": "0.76.3",
    "react-native-safe-area-context": "4.12.0",
    "react-native-screens": "4.1.0",
    "react-native-gesture-handler": "2.20.2",
    "react-native-reanimated": "3.16.1",
    "react-native-maps": "1.18.0",
    "@react-native-async-storage/async-storage": "2.1.0",
    "@expo/vector-icons": "14.0.4",
    "axios": "1.7.9",
    "react-hook-form": "7.54.0",
    "zod": "3.23.8",
    "@hookform/resolvers": "3.9.1",
    "react-native-toast-message": "2.2.1"
  },
  "devDependencies": {
    "@babel/core": "7.25.2",
    "@types/react": "18.3.12",
    "typescript": "5.3.3",
    "jest": "29.7.0",
    "jest-expo": "52.0.2",
    "@testing-library/react-native": "12.9.0",
    "@testing-library/jest-native": "5.4.3",
    "eslint": "8.57.1",
    "eslint-config-expo": "8.0.1",
    "prettier": "3.4.1",
    "msw": "2.6.6"
  },
  "private": true
}
```

### Por que sem `^` nem `~`?

A rubrica tem penalidade explícita: **"O aplicativo não executa corretamente após o `git clone`, devido a erros de build, incompatibilidade de dependências ou versões incorretas de bibliotecas (-50 pontos)"**. Travar versões elimina esse risco.

---

## 9. Estrutura de pastas

```
pulso-mobile/
├── app/                          # Expo Router — file-based routes
│   ├── _layout.tsx               # Layout raiz (providers, fonts)
│   ├── index.tsx                 # Splash / redirect inicial
│   ├── (auth)/                   # Grupo: rotas de autenticação
│   │   ├── _layout.tsx
│   │   ├── login.tsx
│   │   └── register.tsx
│   └── (tabs)/                   # Grupo: app autenticado com bottom tabs
│       ├── _layout.tsx           # Tab navigator
│       ├── home.tsx              # Score atual + recomendação
│       ├── mapa.tsx              # Mapa de zonas
│       ├── historico.tsx         # 7 últimos dias
│       └── perfil.tsx            # Editar dados / logout
│
├── src/
│   ├── api/                      # Cliente HTTP e endpoints
│   │   ├── client.ts             # Axios instance + interceptors
│   │   ├── auth.api.ts           # login, register
│   │   ├── score.api.ts          # current, historico, zonas
│   │   ├── recomendacao.api.ts   # gerar
│   │   ├── usuario.api.ts        # CRUD usuario
│   │   └── mapa.api.ts           # GeoJSON camadas
│   │
│   ├── components/               # Componentes reutilizáveis
│   │   ├── Button.tsx
│   │   ├── Input.tsx
│   │   ├── Card.tsx
│   │   ├── ScoreGauge.tsx        # mostra score 0–100 visualmente
│   │   ├── LoadingView.tsx
│   │   ├── ErrorView.tsx
│   │   ├── EmptyView.tsx
│   │   ├── Header.tsx
│   │   └── ZonePin.tsx           # pin customizado do mapa
│   │
│   ├── contexts/                 # Estado global via Context
│   │   ├── AuthContext.tsx
│   │   └── LocationContext.tsx
│   │
│   ├── hooks/                    # Hooks customizados
│   │   ├── useAuth.ts
│   │   ├── useScoreAtual.ts
│   │   ├── useLocation.ts
│   │   └── useApiError.ts
│   │
│   ├── types/                    # Tipos TypeScript
│   │   ├── api.types.ts          # Reflete DTOs da Java API
│   │   ├── domain.types.ts       # Tipos de domínio (Score, Zona, etc)
│   │   └── navigation.types.ts
│   │
│   ├── constants/                # Tokens e configurações
│   │   ├── theme.ts              # Cores, espaçamentos, tipografia
│   │   ├── config.ts             # URLs de API por ambiente
│   │   └── strings.ts            # Textos da UI (preparar i18n futuro)
│   │
│   ├── utils/                    # Funções utilitárias
│   │   ├── format.ts             # formatar score, data, etc
│   │   ├── validation.ts         # schemas zod compartilhados
│   │   └── storage.ts            # wrapper de expo-secure-store
│   │
│   └── mocks/                    # Mock API local (MSW)
│       ├── handlers.ts           # Handlers MSW
│       ├── fixtures.ts           # Dados fake
│       └── server.ts             # Setup do server MSW
│
├── assets/                       # Imagens, fontes, ícones
│   ├── images/
│   │   ├── logo.png
│   │   └── splash.png
│   └── fonts/                    # se usar fonts customizadas
│
├── __tests__/                    # Testes
│   ├── components/
│   ├── hooks/
│   └── screens/
│
├── .env.example                  # Variáveis de ambiente
├── .gitignore
├── .eslintrc.js
├── .prettierrc
├── app.json                      # Configuração Expo
├── babel.config.js
├── tsconfig.json
├── package.json
├── package-lock.json             # SEMPRE commitar (Yarn lock se usar yarn)
└── README.md                     # OBRIGATÓRIO com instruções de execução
```

### Justificativas

- **`app/` separado de `src/`**: Expo Router exige `app/` na raiz; lógica fica em `src/` para clareza
- **Grupos `(auth)` e `(tabs)`**: Expo Router permite agrupar rotas com parênteses (não viram URL)
- **`src/api/` separa por recurso**: facilita encontrar endpoints e refletir contratos da Java API 1:1
- **`src/mocks/` no source code**: MSW intercepta requests em dev, contratos batem com o real

---

## 10. Convenções de código

### Nomenclatura

| Tipo | Padrão | Exemplo |
|---|---|---|
| Arquivo de componente | `PascalCase.tsx` | `ScoreGauge.tsx` |
| Arquivo de hook | `useCamelCase.ts` | `useScoreAtual.ts` |
| Arquivo de api | `recurso.api.ts` | `score.api.ts` |
| Arquivo de tipo | `dominio.types.ts` | `api.types.ts` |
| Variável/função | `camelCase` | `buscarScoreAtual` |
| Constante global | `SCREAMING_SNAKE_CASE` | `API_BASE_URL` |
| Tipo/Interface | `PascalCase` | `ScoreCurrentResponse` |
| Enum | `PascalCase` valores `UPPER_CASE` | `ClassificacaoScore.BOM` |

### TypeScript: regras inegociáveis

```typescript
// ❌ ERRADO
const buscar = async (id: any) => {
  const r = await axios.get('/score/' + id);
  return r.data;
}

// ✅ CERTO
import type { ScoreCurrentResponse } from '@/types/api.types';

export async function buscarScore(id: number): Promise<ScoreCurrentResponse> {
  const r = await api.get<ScoreCurrentResponse>(`/score/${id}`);
  return r.data;
}
```

Regras:
- **Sempre** tipar parâmetros e retornos de funções públicas
- **Sempre** usar `type` para alias de tipos, `interface` apenas para extensão
- **Nunca** usar `any` — use `unknown` se necessário e narrow depois
- **Nunca** usar `as` para forçar cast — refatore o tipo
- **Sempre** usar `import type` para tipos puros (compilação mais rápida)

### Componente padrão

```typescript
// src/components/Button.tsx
import React from 'react';
import { Pressable, Text, StyleSheet, ActivityIndicator, ViewStyle } from 'react-native';
import { theme } from '@/constants/theme';

type ButtonVariant = 'primary' | 'secondary' | 'ghost';

interface ButtonProps {
  label: string;
  onPress: () => void;
  variant?: ButtonVariant;
  loading?: boolean;
  disabled?: boolean;
  style?: ViewStyle;
  testID?: string;
}

export function Button({
  label,
  onPress,
  variant = 'primary',
  loading = false,
  disabled = false,
  style,
  testID,
}: ButtonProps) {
  const isDisabled = disabled || loading;

  return (
    <Pressable
      onPress={onPress}
      disabled={isDisabled}
      style={[styles.base, styles[variant], isDisabled && styles.disabled, style]}
      testID={testID}
      accessibilityRole="button"
      accessibilityLabel={label}
      accessibilityState={{ disabled: isDisabled, busy: loading }}
    >
      {loading ? (
        <ActivityIndicator color={variant === 'primary' ? '#fff' : theme.colors.primary} />
      ) : (
        <Text style={[styles.textBase, styles[`${variant}Text`]]}>{label}</Text>
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  base: {
    paddingHorizontal: theme.spacing.lg,
    paddingVertical: theme.spacing.md,
    borderRadius: theme.radius.md,
    alignItems: 'center',
    justifyContent: 'center',
    minHeight: 48,
  },
  primary: { backgroundColor: theme.colors.primary },
  secondary: { backgroundColor: theme.colors.surface, borderWidth: 1, borderColor: theme.colors.primary },
  ghost: { backgroundColor: 'transparent' },
  disabled: { opacity: 0.5 },
  textBase: { fontSize: theme.fontSize.md, fontWeight: '600' },
  primaryText: { color: '#fff' },
  secondaryText: { color: theme.colors.primary },
  ghostText: { color: theme.colors.primary },
});
```

Padrões aplicados:
- Props tipadas com interface
- Variants tipados
- `testID` para teste
- Atributos de acessibilidade (`accessibilityRole`, `accessibilityLabel`, `accessibilityState`)
- StyleSheet no fim do arquivo, nunca inline
- Tokens do theme (nunca cor hardcoded)

---

## 11. Convenções Git

### Padrão de commit (Conventional Commits)

```
<type>(<scope>): <descrição curta em português ou inglês>

[corpo opcional]

[footer opcional]
```

**Types permitidos:**
- `feat` — nova funcionalidade
- `fix` — correção de bug
- `chore` — manutenção, deps, build
- `docs` — documentação
- `style` — formatação, sem mudança de código
- `refactor` — refatoração sem mudança de comportamento
- `test` — adicionar/corrigir testes
- `perf` — melhoria de performance

**Scopes do projeto:**
- `auth`, `score`, `mapa`, `historico`, `perfil`, `mock`, `api`, `theme`, `deps`

### Exemplos

```bash
✅ feat(auth): implement login screen with JWT storage
✅ feat(score): add score gauge component on home screen
✅ fix(mapa): correct zone pin coordinates on initial load
✅ chore(deps): pin all dependencies to exact versions
✅ test(auth): add login flow integration test

❌ "ajustes"          → -40 pts: histórico Git confuso
❌ "WIP"              → idem
❌ "Update App.tsx"   → idem
❌ "fix"              → muito vago
```

### Branch strategy

Como é um time pequeno e prazo apertado, o ideal é **trunk-based**:

- `main` é a branch única, sempre deployable
- Commits diretos em `main` permitidos para hotfix
- Para features: branch `feat/nome-curto` → PR → merge squash em `main`

### Frequência de commits

**Mínimo:** 1 commit por dia útil de desenvolvimento (rubrica avalia "envolvimento prático")
**Ideal:** 2–4 commits/dia por feature concluída

A rubrica diz: *"É esperado que o repositório tenha uma árvore de commits sequencial e evolutiva, com mensagens claras e representando a construção real do app."*

---

# PARTE 3 — CONTRATOS COM A API

## 12. URL base

### Configuração por ambiente

`src/constants/config.ts`:

```typescript
import Constants from 'expo-constants';

interface EnvConfig {
  apiBaseUrl: string;
  useMockApi: boolean;
}

const ENV: Record<string, EnvConfig> = {
  development: {
    // Usar IP da máquina dev no Wi-Fi LAN (não localhost — celular não enxerga localhost)
    // No Windows: ipconfig → IPv4
    // No Mac/Linux: ifconfig | grep "inet "
    apiBaseUrl: 'http://192.168.1.100:8080/api/v1',
    useMockApi: true, // ★ true até a Java API estar deployada
  },
  staging: {
    apiBaseUrl: 'https://pulso-urbano-562999.fly.dev/api/v1',
    useMockApi: false,
  },
  production: {
    apiBaseUrl: 'https://pulso-urbano-562999.fly.dev/api/v1',
    useMockApi: false,
  },
};

const releaseChannel = Constants.expoConfig?.extra?.releaseChannel ?? 'development';
export const config: EnvConfig = ENV[releaseChannel] ?? ENV.development;
```

### Por que IP da LAN, não localhost?

Quando você roda `npx expo start` e abre no celular físico via QR Code, o app está rodando NO CELULAR. Ele NÃO enxerga `localhost` do seu PC — `localhost` no celular é o próprio celular.

Solução: usar o **IP da sua máquina no Wi-Fi local**. Tanto seu PC quanto o celular precisam estar na mesma rede.

```bash
# Windows
ipconfig
# Procure "IPv4 Address" da sua conexão Wi-Fi

# Mac/Linux
ifconfig | grep "inet " | grep -v 127.0.0.1
# ou:
ipconfig getifaddr en0   # Mac
```

Cole esse IP em `apiBaseUrl`. Exemplo: `http://192.168.0.42:8080/api/v1`.

---

## 13. Autenticação JWT

### Fluxo completo

```
┌─────────┐                ┌─────────┐                ┌──────────┐
│ Mobile  │                │  API    │                │ Storage  │
└────┬────┘                └────┬────┘                └────┬─────┘
     │                          │                          │
     │ POST /auth/login         │                          │
     │ {email, senha}           │                          │
     ├─────────────────────────>│                          │
     │                          │                          │
     │     200 { token, ... }   │                          │
     │<─────────────────────────┤                          │
     │                          │                          │
     │ SecureStore.setItemAsync('jwt', token)             │
     ├───────────────────────────────────────────────────>│
     │                          │                          │
     │                          │                          │
     │ Próximo request:         │                          │
     │ GET /score/current       │                          │
     │ Authorization: Bearer {token}                       │
     ├─────────────────────────>│                          │
     │                          │                          │
     │      200 { score, ... }  │                          │
     │<─────────────────────────┤                          │
     │                          │                          │
     │                          │                          │
     │ Se 401 (token expirado): │                          │
     │ - Limpa storage          │                          │
     │ - Redireciona p/ login   │                          │
     ├───────────────────────────────────────────────────>│
```

### Implementação do client HTTP

`src/api/client.ts`:

```typescript
import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import * as SecureStore from 'expo-secure-store';
import { router } from 'expo-router';
import Toast from 'react-native-toast-message';
import { config } from '@/constants/config';

const TOKEN_KEY = 'pulso_jwt';

export const api = axios.create({
  baseURL: config.apiBaseUrl,
  timeout: 10_000,
  headers: { 'Content-Type': 'application/json' },
});

// Interceptor de REQUEST: injeta token em todo request
api.interceptors.request.use(async (cfg: InternalAxiosRequestConfig) => {
  const token = await SecureStore.getItemAsync(TOKEN_KEY);
  if (token && cfg.headers) {
    cfg.headers.Authorization = `Bearer ${token}`;
  }
  return cfg;
});

// Interceptor de RESPONSE: trata erros globais
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    if (!error.response) {
      // erro de rede — sem internet, timeout, etc.
      Toast.show({
        type: 'error',
        text1: 'Sem conexão',
        text2: 'Verifique sua internet e tente novamente.',
      });
      return Promise.reject(error);
    }

    const status = error.response.status;

    if (status === 401) {
      // Token expirado ou inválido — desloga
      await SecureStore.deleteItemAsync(TOKEN_KEY);
      Toast.show({
        type: 'info',
        text1: 'Sessão expirada',
        text2: 'Por favor faça login novamente.',
      });
      router.replace('/(auth)/login');
    } else if (status === 403) {
      Toast.show({ type: 'error', text1: 'Acesso negado' });
    } else if (status === 404) {
      // 404 frequentemente é esperado em /score/current quando não há dado — não fazer toast global
      // a tela trata
    } else if (status >= 500) {
      Toast.show({
        type: 'error',
        text1: 'Erro no servidor',
        text2: 'Tente novamente em alguns instantes.',
      });
    }

    return Promise.reject(error);
  }
);

// Helpers de storage
export async function setAuthToken(token: string): Promise<void> {
  await SecureStore.setItemAsync(TOKEN_KEY, token);
}

export async function clearAuthToken(): Promise<void> {
  await SecureStore.deleteItemAsync(TOKEN_KEY);
}

export async function getAuthToken(): Promise<string | null> {
  return SecureStore.getItemAsync(TOKEN_KEY);
}
```

---

## 14. Catálogo de endpoints

### POST /api/v1/auth/login

**Request:**
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "felipe@fiap.com.br",
  "senha": "minha-senha-secreta"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJmZWxpcGVAZmlhcC5jb20uYnIiLCJ1c3VhcmlvSWQiOjEsInJvbGUiOiJVU0VSIiwiaWF0IjoxNzE2ODAwMDAwLCJleHAiOjE3MTY4ODY0MDB9.signature",
  "tipo": "Bearer",
  "expiraEmMs": 86400000
}
```

**Response 401:**
```json
{
  "status": 401,
  "erro": "Unauthorized",
  "mensagem": "Credenciais inválidas",
  "camposInvalidos": [],
  "timestamp": "2026-05-27T10:30:00"
}
```

**TypeScript:**
```typescript
// src/types/api.types.ts
export interface LoginRequest {
  email: string;
  senha: string;
}

export interface LoginResponse {
  token: string;
  tipo: 'Bearer';
  expiraEmMs: number;
}

// src/api/auth.api.ts
import { api, setAuthToken } from './client';
import type { LoginRequest, LoginResponse } from '@/types/api.types';

export async function login(req: LoginRequest): Promise<LoginResponse> {
  const r = await api.post<LoginResponse>('/auth/login', req);
  await setAuthToken(r.data.token);
  return r.data;
}
```

---

### POST /api/v1/auth/register

**Request:**
```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "nome": "Felipe Ferrete",
  "email": "felipe@fiap.com.br",
  "senha": "minha-senha-secreta",
  "fazExercicio": true,
  "temCrianca": false,
  "temProblemaResp": false
}
```

**Response 201:**
```json
{
  "id": 1,
  "nome": "Felipe Ferrete",
  "email": "felipe@fiap.com.br",
  "role": "USER",
  "fazExercicio": true,
  "temCrianca": false,
  "temProblemaResp": false,
  "dtCriacao": "2026-05-27T10:30:00"
}
```

**Response 409 (email duplicado):**
```json
{
  "status": 409,
  "erro": "Conflict",
  "mensagem": "Email já cadastrado: felipe@fiap.com.br",
  "camposInvalidos": [],
  "timestamp": "2026-05-27T10:30:00"
}
```

---

### GET /api/v1/score/current

**Request:**
```http
GET /api/v1/score/current?lat=-23.5505&lon=-46.6333
Authorization: Bearer eyJhbGciOiJIUzI1NiJ9...
```

**Response 200:**
```json
{
  "score": 62.4,
  "classificacao": "MODERADO",
  "no2Ppb": 28.4,
  "tempSuperficieC": 41.2,
  "fonteDadoNo2": "Sentinel-5P TROPOMI",
  "fonteDadoTemp": "ECOSTRESS ISS/NASA",
  "dtDadoOrbital": "2026-05-27T08:00:00Z",
  "zonaId": 3,
  "zonaNome": "Zona Leste - São Paulo",
  "_links": {
    "self": { "href": "https://api/api/v1/score/current?lat=-23.55&lon=-46.63" },
    "recomendacao": { "href": "https://api/api/v1/recomendacao?scoreId=99&usuarioId=1" }
  }
}
```

**Response 404 (sem dado para zona):**
```json
{
  "status": 404,
  "erro": "Not Found",
  "mensagem": "Sem score para coordenadas",
  "camposInvalidos": [],
  "timestamp": "2026-05-27T10:30:00"
}
```

**TypeScript:**
```typescript
export type ClassificacaoScore = 'BOM' | 'MODERADO' | 'RUIM' | 'CRITICO';

export interface HateoasLink {
  href: string;
}

export interface ScoreCurrentResponse {
  score: number;
  classificacao: ClassificacaoScore;
  no2Ppb: number;
  tempSuperficieC: number;
  fonteDadoNo2: string;
  fonteDadoTemp: string;
  dtDadoOrbital: string; // ISO 8601
  zonaId: number;
  zonaNome: string;
  _links: {
    self: HateoasLink;
    recomendacao: HateoasLink;
  };
}

// src/api/score.api.ts
export async function buscarScoreAtual(lat: number, lon: number): Promise<ScoreCurrentResponse> {
  const r = await api.get<ScoreCurrentResponse>('/score/current', { params: { lat, lon } });
  return r.data;
}
```

---

### GET /api/v1/score/historico

**Request:**
```http
GET /api/v1/score/historico?zonaId=3&dias=7
Authorization: Bearer ...
```

**Response 200:**
```json
{
  "usuarioId": 3,
  "historico": [
    { "dt": "2026-05-27", "score": 62.4, "classificacao": "MODERADO" },
    { "dt": "2026-05-26", "score": 71.8, "classificacao": "MODERADO" },
    { "dt": "2026-05-25", "score": 84.2, "classificacao": "BOM" },
    { "dt": "2026-05-24", "score": 78.5, "classificacao": "MODERADO" },
    { "dt": "2026-05-23", "score": 55.1, "classificacao": "RUIM" },
    { "dt": "2026-05-22", "score": 48.3, "classificacao": "RUIM" },
    { "dt": "2026-05-21", "score": 39.2, "classificacao": "CRITICO" }
  ]
}
```

---

### GET /api/v1/score/zonas (PÚBLICO — sem auth)

**Request:**
```http
GET /api/v1/score/zonas
```

**Response 200:**
```json
{
  "zonas": [
    { "id": 1, "nome": "Centro", "score": 55.3, "lat": -23.5505, "lon": -46.6333 },
    { "id": 2, "nome": "Zona Oeste - Pinheiros", "score": 72.1, "lat": -23.5645, "lon": -46.7025 },
    { "id": 3, "nome": "Zona Leste - Tatuapé", "score": 62.4, "lat": -23.5410, "lon": -46.5763 },
    { "id": 4, "nome": "Zona Sul - Vila Mariana", "score": 68.9, "lat": -23.5870, "lon": -46.6361 },
    { "id": 5, "nome": "Zona Norte - Santana", "score": 59.2, "lat": -23.5040, "lon": -46.6280 }
  ]
}
```

---

### GET /api/v1/recomendacao

**Request:**
```http
GET /api/v1/recomendacao?scoreId=99&usuarioId=1
Authorization: Bearer ...
```

**Response 200:**
```json
{
  "texto": "Qualidade do ar moderada. Prefira sair antes das 10h ou após as 17h. Evite corrida e ciclismo entre 11h e 16h.",
  "icone": "warning",
  "nivel": "MODERADO",
  "personalizadaPara": ["exercicio_fisico"],
  "dtGeracao": "2026-05-27T10:30:00"
}
```

**Mapeamento de ícones (backend usa Material Icons):**

| `icone` (string) | Mapeia para `@expo/vector-icons` MaterialIcons |
|---|---|
| `check_circle` | `<MaterialIcons name="check-circle" />` |
| `warning` | `<MaterialIcons name="warning" />` |
| `error` | `<MaterialIcons name="error" />` |
| `dangerous` | `<MaterialIcons name="dangerous" />` |

---

### GET /api/v1/mapa/camadas (PÚBLICO)

**Request:**
```http
GET /api/v1/mapa/camadas?tipo=no2&cidade=sao_paulo
```

**Response 200 (GeoJSON):**
```json
{
  "type": "FeatureCollection",
  "fonte": "Sentinel-5P TROPOMI",
  "dtCaptura": "2026-05-27",
  "features": [
    {
      "type": "Feature",
      "geometry": { "type": "Point", "coordinates": [-46.6333, -23.5505] },
      "properties": { "zonaId": 1, "zonaNome": "Centro SP", "valor": 28.4, "unidade": "ppb" }
    },
    {
      "type": "Feature",
      "geometry": { "type": "Point", "coordinates": [-46.7025, -23.5645] },
      "properties": { "zonaId": 2, "zonaNome": "Pinheiros", "valor": 19.7, "unidade": "ppb" }
    }
  ]
}
```

⚠️ **Atenção GeoJSON:** o array `coordinates` é `[longitude, latitude]` (padrão GeoJSON). Mas `react-native-maps` usa `{ latitude, longitude }`. Cuidado na conversão.

---

### CRUD /api/v1/usuario

#### GET /api/v1/usuario/{id}
```json
{
  "id": 1,
  "nome": "Felipe Ferrete",
  "email": "felipe@fiap.com.br",
  "role": "USER",
  "fazExercicio": true,
  "temCrianca": false,
  "temProblemaResp": false,
  "dtCriacao": "2026-05-27T10:30:00"
}
```

#### PUT /api/v1/usuario/{id}
Request body (todos opcionais):
```json
{
  "nome": "Felipe F. Ferrete",
  "fazExercicio": false,
  "temCrianca": true,
  "temProblemaResp": false
}
```
Response 200: mesmo formato do GET.

#### DELETE /api/v1/usuario/{id}
Response 204 (sem body).

---

## 15. Mapeamento de erros

### Tabela canônica

| Status | Significado | Ação no mobile |
|---|---|---|
| 200/201/204 | Sucesso | Atualiza UI |
| 400 | Validação falhou | Mostrar `camposInvalidos[]` próximo aos inputs |
| 401 | Não autenticado / token expirado | Limpar token + redirecionar `/(auth)/login` |
| 403 | Sem permissão | Toast "Acesso negado" + voltar tela |
| 404 | Recurso não existe | Empty state contextual ("Sem dados para essa zona") |
| 409 | Conflito (ex: email duplicado) | Mostrar mensagem inline no form |
| 503 | API orbital indisponível | Toast "Serviço temporariamente indisponível, tente novamente" |
| ≥500 | Erro de servidor | Toast genérico + Sentry/log |
| Network error (sem response) | Sem internet ou timeout | Toast "Sem conexão" |

### Hook `useApiError` para padronizar

`src/hooks/useApiError.ts`:

```typescript
import { useCallback } from 'react';
import { AxiosError } from 'axios';
import Toast from 'react-native-toast-message';

interface ApiErrorPayload {
  status: number;
  erro: string;
  mensagem: string;
  camposInvalidos?: string[];
  timestamp: string;
}

export function useApiError() {
  return useCallback((error: unknown, context?: string) => {
    if (error instanceof AxiosError) {
      const payload = error.response?.data as ApiErrorPayload | undefined;
      const msg = payload?.mensagem ?? error.message ?? 'Erro desconhecido';

      // 401, 5xx já tratados no interceptor — só logar
      if (error.response?.status && error.response.status !== 401 && error.response.status < 500) {
        Toast.show({
          type: 'error',
          text1: context ?? 'Erro',
          text2: msg,
        });
      }
      return { campos: payload?.camposInvalidos ?? [], mensagem: msg };
    }
    Toast.show({ type: 'error', text1: 'Erro inesperado' });
    return { campos: [], mensagem: 'Erro inesperado' };
  }, []);
}
```

---

## 16. Mock API local

**Por que MSW e não JSON fake estático?**

Você precisa que o mobile **consuma os mesmos contratos** que a Java API vai retornar. Se você fizer fixtures soltos em variáveis, no dia da integração nada bate. MSW intercepta as chamadas axios e devolve respostas iguaisinhas às reais.

### Setup MSW

**1. Instalar:**
```bash
npm install --save-dev msw@2.6.6
```

**2. Handlers — `src/mocks/handlers.ts`:**

```typescript
import { http, HttpResponse } from 'msw';
import { config } from '@/constants/config';

const BASE = config.apiBaseUrl;

let usuarios = [
  {
    id: 1,
    nome: 'Felipe Teste',
    email: 'teste@pulso.com',
    senha: 'senha123', // só no mock
    role: 'USER',
    fazExercicio: true,
    temCrianca: false,
    temProblemaResp: false,
    dtCriacao: '2026-05-27T10:00:00',
  },
];

const zonas = [
  { id: 1, nome: 'Centro', score: 55.3, lat: -23.5505, lon: -46.6333 },
  { id: 2, nome: 'Pinheiros', score: 72.1, lat: -23.5645, lon: -46.7025 },
  { id: 3, nome: 'Tatuapé', score: 62.4, lat: -23.5410, lon: -46.5763 },
  { id: 4, nome: 'Vila Mariana', score: 68.9, lat: -23.5870, lon: -46.6361 },
  { id: 5, nome: 'Santana', score: 59.2, lat: -23.5040, lon: -46.6280 },
];

export const handlers = [
  // POST /auth/login
  http.post(`${BASE}/auth/login`, async ({ request }) => {
    const body = (await request.json()) as { email: string; senha: string };
    const user = usuarios.find((u) => u.email === body.email && u.senha === body.senha);
    if (!user) {
      return HttpResponse.json(
        {
          status: 401,
          erro: 'Unauthorized',
          mensagem: 'Credenciais inválidas',
          camposInvalidos: [],
          timestamp: new Date().toISOString(),
        },
        { status: 401 }
      );
    }
    return HttpResponse.json({
      token: 'mock-jwt-token-' + user.id,
      tipo: 'Bearer',
      expiraEmMs: 86_400_000,
    });
  }),

  // POST /auth/register
  http.post(`${BASE}/auth/register`, async ({ request }) => {
    const body = (await request.json()) as any;
    if (usuarios.find((u) => u.email === body.email)) {
      return HttpResponse.json(
        {
          status: 409,
          erro: 'Conflict',
          mensagem: `Email já cadastrado: ${body.email}`,
          camposInvalidos: [],
          timestamp: new Date().toISOString(),
        },
        { status: 409 }
      );
    }
    const novo = { ...body, id: usuarios.length + 1, role: 'USER', dtCriacao: new Date().toISOString() };
    usuarios.push(novo);
    return HttpResponse.json(novo, { status: 201 });
  }),

  // GET /score/current?lat&lon
  http.get(`${BASE}/score/current`, ({ request }) => {
    const url = new URL(request.url);
    const lat = Number(url.searchParams.get('lat'));
    const lon = Number(url.searchParams.get('lon'));
    // acha zona mais próxima (mock simples)
    const zona = zonas.reduce((acc, z) => {
      const dist = Math.hypot(z.lat - lat, z.lon - lon);
      const distAcc = Math.hypot(acc.lat - lat, acc.lon - lon);
      return dist < distAcc ? z : acc;
    });
    const classificacao =
      zona.score >= 80 ? 'BOM' : zona.score >= 60 ? 'MODERADO' : zona.score >= 40 ? 'RUIM' : 'CRITICO';
    return HttpResponse.json({
      score: zona.score,
      classificacao,
      no2Ppb: 28.4,
      tempSuperficieC: 41.2,
      fonteDadoNo2: 'Sentinel-5P TROPOMI (MOCK)',
      fonteDadoTemp: 'ECOSTRESS ISS/NASA (MOCK)',
      dtDadoOrbital: new Date().toISOString(),
      zonaId: zona.id,
      zonaNome: zona.nome,
      _links: {
        self: { href: `${BASE}/score/current?lat=${lat}&lon=${lon}` },
        recomendacao: { href: `${BASE}/recomendacao?scoreId=99&usuarioId=1` },
      },
    });
  }),

  // GET /score/zonas
  http.get(`${BASE}/score/zonas`, () => HttpResponse.json({ zonas })),

  // GET /score/historico
  http.get(`${BASE}/score/historico`, ({ request }) => {
    const url = new URL(request.url);
    const dias = Number(url.searchParams.get('dias') ?? 7);
    const historico = Array.from({ length: dias }, (_, i) => {
      const d = new Date();
      d.setDate(d.getDate() - i);
      const score = 50 + Math.random() * 40;
      const cls = score >= 80 ? 'BOM' : score >= 60 ? 'MODERADO' : score >= 40 ? 'RUIM' : 'CRITICO';
      return { dt: d.toISOString().split('T')[0], score: Math.round(score * 10) / 10, classificacao: cls };
    });
    return HttpResponse.json({ usuarioId: 1, historico });
  }),

  // GET /recomendacao
  http.get(`${BASE}/recomendacao`, () => {
    return HttpResponse.json({
      texto: 'Qualidade do ar moderada. Prefira sair antes das 10h ou após as 17h.',
      icone: 'warning',
      nivel: 'MODERADO',
      personalizadaPara: ['exercicio_fisico'],
      dtGeracao: new Date().toISOString(),
    });
  }),

  // GET /usuario/{id}
  http.get(`${BASE}/usuario/:id`, ({ params }) => {
    const u = usuarios.find((u) => u.id === Number(params.id));
    if (!u) return HttpResponse.json({ status: 404 }, { status: 404 });
    const { senha, ...rest } = u;
    return HttpResponse.json(rest);
  }),

  // PUT /usuario/{id}
  http.put(`${BASE}/usuario/:id`, async ({ params, request }) => {
    const idx = usuarios.findIndex((u) => u.id === Number(params.id));
    if (idx < 0) return HttpResponse.json({ status: 404 }, { status: 404 });
    const body = (await request.json()) as any;
    usuarios[idx] = { ...usuarios[idx], ...body };
    const { senha, ...rest } = usuarios[idx];
    return HttpResponse.json(rest);
  }),

  // DELETE /usuario/{id}
  http.delete(`${BASE}/usuario/:id`, ({ params }) => {
    usuarios = usuarios.filter((u) => u.id !== Number(params.id));
    return new HttpResponse(null, { status: 204 });
  }),
];
```

**3. Inicialização (em `app/_layout.tsx`):**

```typescript
import { config } from '@/constants/config';

if (config.useMockApi && __DEV__) {
  // Importação dinâmica para não bundlear MSW em produção
  import('@/mocks/server').then(({ startMockServer }) => startMockServer());
}
```

**4. `src/mocks/server.ts`:**

```typescript
import { setupServer } from 'msw/native';
import { handlers } from './handlers';

const server = setupServer(...handlers);

export function startMockServer() {
  server.listen({ onUnhandledRequest: 'bypass' });
  console.log('[MSW] Mock server iniciado');
}
```

⚠️ MSW para React Native (msw/native) ainda é experimental. Plano B: criar um axios mock manual em `src/api/__mock__/` que intercepta antes do axios real quando `config.useMockApi === true`.

### Quando desligar o mock

No dia da integração real (DIA 9 — 04/06), mude `useMockApi: false` em `config.development` E aponte `apiBaseUrl` para a Java API real. Faça commit dessa mudança.

---

# PARTE 4 — DESIGN SYSTEM

## 17. Tokens visuais

`src/constants/theme.ts`:

```typescript
export const theme = {
  colors: {
    // Primária — verde/azul orbital
    primary: '#0EA5E9',     // sky-500
    primaryDark: '#0284C7', // sky-600

    // Score classifications (mapeia direto para BOM/MODERADO/RUIM/CRITICO)
    scoreBom: '#22C55E',       // green-500
    scoreModerado: '#FBBF24',  // amber-400
    scoreRuim: '#F97316',      // orange-500
    scoreCritico: '#EF4444',   // red-500

    // Neutros
    background: '#FFFFFF',
    surface: '#F8FAFC',     // slate-50
    surfaceAlt: '#F1F5F9',  // slate-100
    border: '#E2E8F0',      // slate-200
    text: '#0F172A',        // slate-900
    textMuted: '#64748B',   // slate-500
    textInverse: '#FFFFFF',

    // Feedback
    success: '#22C55E',
    warning: '#FBBF24',
    error: '#EF4444',
    info: '#3B82F6',
  },

  spacing: {
    xs: 4,
    sm: 8,
    md: 12,
    lg: 16,
    xl: 24,
    xxl: 32,
    xxxl: 48,
  },

  radius: {
    sm: 4,
    md: 8,
    lg: 12,
    xl: 16,
    full: 9999,
  },

  fontSize: {
    xs: 11,
    sm: 13,
    md: 15,
    lg: 18,
    xl: 22,
    xxl: 28,
    xxxl: 36,
    display: 48,
  },

  fontWeight: {
    regular: '400',
    medium: '500',
    semibold: '600',
    bold: '700',
  } as const,

  shadow: {
    sm: {
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 1 },
      shadowOpacity: 0.05,
      shadowRadius: 2,
      elevation: 1,
    },
    md: {
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 2 },
      shadowOpacity: 0.1,
      shadowRadius: 4,
      elevation: 3,
    },
    lg: {
      shadowColor: '#000',
      shadowOffset: { width: 0, height: 4 },
      shadowOpacity: 0.15,
      shadowRadius: 8,
      elevation: 6,
    },
  },
} as const;

export type Theme = typeof theme;

// Helper para pegar cor do score
export function corDoScore(classificacao: 'BOM' | 'MODERADO' | 'RUIM' | 'CRITICO'): string {
  return {
    BOM: theme.colors.scoreBom,
    MODERADO: theme.colors.scoreModerado,
    RUIM: theme.colors.scoreRuim,
    CRITICO: theme.colors.scoreCritico,
  }[classificacao];
}
```

### Por que essa paleta

- **Score colors batem com semáforos universais** — verde/amarelo/laranja/vermelho é intuitivo
- **Primária azul (sky)** sugere céu/orbital sem ser cliché de logo de tech
- **Slate** como neutro é mais quente que cinza puro
- Todas as combinações primary/text passam contraste AA (WCAG)

---

## 18. Componentes reutilizáveis

### Lista mínima a implementar

```
components/
├── Button.tsx           ★ (já especificado na seção 10)
├── Input.tsx            ★ — text field com label, erro, helper
├── Card.tsx             ★ — container com sombra e padding
├── ScoreGauge.tsx       ★ — gauge circular ou barra do score
├── ScoreBadge.tsx       — pill colorida com classificação (BOM/MOD/RUIM/CRIT)
├── LoadingView.tsx      ★ — full-screen spinner com texto
├── ErrorView.tsx        ★ — ilustração + mensagem + botão "Tentar novamente"
├── EmptyView.tsx        ★ — ilustração + mensagem "Nada por aqui"
├── Header.tsx           ★ — top bar com title e botão back opcional
├── ZonePin.tsx          ★ — marker customizado para o mapa (cor por score)
├── HistoricoItem.tsx    — item de lista de score histórico
└── RecomendacaoCard.tsx — card com ícone + texto da recomendação
```

★ = OBRIGATÓRIO antes de qualquer tela ser construída.

### Input.tsx (exemplo de componente bem feito)

```typescript
import React, { forwardRef, useState } from 'react';
import { View, TextInput, Text, StyleSheet, TextInputProps } from 'react-native';
import { theme } from '@/constants/theme';

interface InputProps extends Omit<TextInputProps, 'style'> {
  label: string;
  error?: string;
  helper?: string;
  testID?: string;
}

export const Input = forwardRef<TextInput, InputProps>(
  ({ label, error, helper, testID, ...rest }, ref) => {
    const [focused, setFocused] = useState(false);

    return (
      <View style={styles.container}>
        <Text style={styles.label}>{label}</Text>
        <TextInput
          ref={ref}
          style={[
            styles.input,
            focused && styles.inputFocused,
            error && styles.inputError,
          ]}
          placeholderTextColor={theme.colors.textMuted}
          onFocus={() => setFocused(true)}
          onBlur={() => setFocused(false)}
          testID={testID}
          accessibilityLabel={label}
          {...rest}
        />
        {(error || helper) && (
          <Text style={[styles.helper, error && styles.errorText]}>
            {error ?? helper}
          </Text>
        )}
      </View>
    );
  }
);

Input.displayName = 'Input';

const styles = StyleSheet.create({
  container: { marginBottom: theme.spacing.md },
  label: {
    fontSize: theme.fontSize.sm,
    fontWeight: '500',
    color: theme.colors.text,
    marginBottom: theme.spacing.xs,
  },
  input: {
    borderWidth: 1,
    borderColor: theme.colors.border,
    borderRadius: theme.radius.md,
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.sm,
    fontSize: theme.fontSize.md,
    color: theme.colors.text,
    backgroundColor: theme.colors.background,
    minHeight: 48,
  },
  inputFocused: { borderColor: theme.colors.primary, borderWidth: 2 },
  inputError: { borderColor: theme.colors.error },
  helper: {
    fontSize: theme.fontSize.xs,
    color: theme.colors.textMuted,
    marginTop: theme.spacing.xs,
  },
  errorText: { color: theme.colors.error },
});
```

---

## 19. Iconografia

Use **MaterialIcons** de `@expo/vector-icons`. Já vem com Expo, não adiciona dep.

```typescript
import { MaterialIcons } from '@expo/vector-icons';

<MaterialIcons name="check-circle" size={24} color={theme.colors.scoreBom} />
```

### Ícones canônicos do app

| Contexto | Ícone | Cor |
|---|---|---|
| Score BOM | `check-circle` | `scoreBom` |
| Score MODERADO | `warning` | `scoreModerado` |
| Score RUIM | `error` | `scoreRuim` |
| Score CRÍTICO | `dangerous` | `scoreCritico` |
| Tab Home | `home` | primary |
| Tab Mapa | `map` | primary |
| Tab Histórico | `bar-chart` | primary |
| Tab Perfil | `person` | primary |
| Logout | `logout` | error |
| Voltar | `arrow-back` | text |
| Geolocalização | `my-location` | primary |
| Refresh | `refresh` | primary |
| Sem dados | `info-outline` | textMuted |
| Sem rede | `wifi-off` | error |

---

## 20. Acessibilidade

Vale 0 pts diretos na rubrica, mas **vale pontos em "Arquitetura" e em "boas práticas"**. Requisitos mínimos:

- Todo `Pressable`/`TouchableOpacity` tem `accessibilityRole` e `accessibilityLabel`
- Todo `Image` decorativa tem `accessibilityElementsHidden`; semântica tem `accessibilityLabel`
- Todo `TextInput` tem `accessibilityLabel`
- Contraste de texto/fundo passa AA (já garantido pela paleta da seção 17)
- Tamanho mínimo de área tocável: 48×48 (já está nos componentes)
- `testID` em elementos críticos (botões de submit, inputs principais)

---

# PARTE 5 — TELAS E NAVEGAÇÃO

## 21. Mapa de navegação

```
                    ┌─────────────────┐
                    │   app/index.tsx │
                    │  (verifica jwt) │
                    └────────┬────────┘
                             │
                  ┌──────────┴──────────┐
                  ▼                     ▼
        ┌──────────────────┐  ┌──────────────────┐
        │   (auth) Stack   │  │   (tabs) Tabs    │
        ├──────────────────┤  ├──────────────────┤
        │ login.tsx        │  │ home.tsx         │
        │ register.tsx     │  │ mapa.tsx         │
        └──────────────────┘  │ historico.tsx    │
                              │ perfil.tsx       │
                              └────────┬─────────┘
                                       │
                              ┌────────▼─────────┐
                              │ Stacks de detalhe│
                              │ (modal/push):    │
                              │ - zona/[id].tsx  │
                              │ - editar-perfil  │
                              └──────────────────┘
```

### Configuração Expo Router

`app/_layout.tsx`:

```typescript
import { Stack } from 'expo-router';
import { AuthProvider } from '@/contexts/AuthContext';
import { LocationProvider } from '@/contexts/LocationContext';
import Toast from 'react-native-toast-message';

export default function RootLayout() {
  return (
    <AuthProvider>
      <LocationProvider>
        <Stack screenOptions={{ headerShown: false }}>
          <Stack.Screen name="index" />
          <Stack.Screen name="(auth)" />
          <Stack.Screen name="(tabs)" />
        </Stack>
        <Toast />
      </LocationProvider>
    </AuthProvider>
  );
}
```

`app/index.tsx`:

```typescript
import { useEffect } from 'react';
import { router } from 'expo-router';
import { useAuth } from '@/hooks/useAuth';
import { LoadingView } from '@/components/LoadingView';

export default function Index() {
  const { autenticado, carregando } = useAuth();

  useEffect(() => {
    if (carregando) return;
    if (autenticado) router.replace('/(tabs)/home');
    else router.replace('/(auth)/login');
  }, [autenticado, carregando]);

  return <LoadingView mensagem="Iniciando Pulso Urbano..." />;
}
```

`app/(tabs)/_layout.tsx`:

```typescript
import { Tabs } from 'expo-router';
import { MaterialIcons } from '@expo/vector-icons';
import { theme } from '@/constants/theme';

export default function TabsLayout() {
  return (
    <Tabs
      screenOptions={{
        tabBarActiveTintColor: theme.colors.primary,
        tabBarInactiveTintColor: theme.colors.textMuted,
        headerShown: false,
      }}
    >
      <Tabs.Screen
        name="home"
        options={{
          title: 'Hoje',
          tabBarIcon: ({ color, size }) => <MaterialIcons name="home" color={color} size={size} />,
        }}
      />
      <Tabs.Screen
        name="mapa"
        options={{
          title: 'Mapa',
          tabBarIcon: ({ color, size }) => <MaterialIcons name="map" color={color} size={size} />,
        }}
      />
      <Tabs.Screen
        name="historico"
        options={{
          title: 'Histórico',
          tabBarIcon: ({ color, size }) => <MaterialIcons name="bar-chart" color={color} size={size} />,
        }}
      />
      <Tabs.Screen
        name="perfil"
        options={{
          title: 'Perfil',
          tabBarIcon: ({ color, size }) => <MaterialIcons name="person" color={color} size={size} />,
        }}
      />
    </Tabs>
  );
}
```

---

## 22. Specs das telas

### TELA 1 · LOGIN (`app/(auth)/login.tsx`)

**Wireframe:**
```
┌─────────────────────────────┐
│                             │
│        [Logo Pulso]         │
│                             │
│       Pulso Urbano          │
│  Qualidade do ar em SP      │
│                             │
│  ┌───────────────────────┐  │
│  │ Email                 │  │
│  └───────────────────────┘  │
│  ┌───────────────────────┐  │
│  │ Senha          [👁]   │  │
│  └───────────────────────┘  │
│                             │
│  ┌───────────────────────┐  │
│  │       Entrar          │  │
│  └───────────────────────┘  │
│                             │
│   Não tem conta? Cadastre-se│
│                             │
└─────────────────────────────┘
```

**Componentes:** `Input`, `Button`

**Estados:**
- `idle` — formulário vazio
- `validating` — após submit, antes do response
- `error` — email/senha inválidos (mostrar abaixo do form)
- `success` — redireciona para `/(tabs)/home`

**Validação (zod):**
```typescript
const loginSchema = z.object({
  email: z.string().email('Email inválido'),
  senha: z.string().min(6, 'Senha deve ter no mínimo 6 caracteres'),
});
```

**Comportamento:**
- Botão "Entrar" desabilita enquanto loading
- Toque em "Cadastre-se" → `router.push('/(auth)/register')`
- Botão olho 👁 alterna `secureTextEntry`
- Após sucesso: salva token via `setAuthToken`, redireciona para `/(tabs)/home`

---

### TELA 2 · REGISTER (`app/(auth)/register.tsx`)

**Wireframe:**
```
┌─────────────────────────────┐
│ ←  Criar conta              │
├─────────────────────────────┤
│                             │
│  ┌───────────────────────┐  │
│  │ Nome                  │  │
│  └───────────────────────┘  │
│  ┌───────────────────────┐  │
│  │ Email                 │  │
│  └───────────────────────┘  │
│  ┌───────────────────────┐  │
│  │ Senha          [👁]   │  │
│  └───────────────────────┘  │
│                             │
│  Personalização (opcional): │
│  □ Pratico exercício físico │
│  □ Tenho criança em casa    │
│  □ Tenho problema respirat. │
│                             │
│  ┌───────────────────────┐  │
│  │    Criar conta        │  │
│  └───────────────────────┘  │
│                             │
└─────────────────────────────┘
```

**Estados específicos:**
- `email_conflict` (409) — mostrar "Email já cadastrado" abaixo do input email

---

### TELA 3 · HOME (`app/(tabs)/home.tsx`) — A TELA MAIS IMPORTANTE

**Wireframe:**
```
┌─────────────────────────────┐
│  Olá, Felipe!         [🔄]  │
│  📍 Zona Leste              │
├─────────────────────────────┤
│                             │
│      ┌─────────────┐        │
│      │             │        │
│      │     62      │        │
│      │  MODERADO   │        │
│      │             │        │
│      └─────────────┘        │
│                             │
├─────────────────────────────┤
│  Recomendação para você:    │
│  ┌───────────────────────┐  │
│  │ ⚠️  Qualidade do ar    │  │
│  │    moderada. Prefira  │  │
│  │    sair antes das     │  │
│  │    10h ou após 17h.   │  │
│  │    Evite corrida...   │  │
│  └───────────────────────┘  │
│                             │
├─────────────────────────────┤
│  Dados de origem:           │
│  • NO₂: 28.4 ppb (Sentinel) │
│  • Temp: 41.2°C (ECOSTRESS) │
│  • Atualizado: hoje 08:00   │
│                             │
└─────────────────────────────┘
```

**Componentes:** `ScoreGauge`, `RecomendacaoCard`, `Card`, `Header`

**Estados:**
- `loading_location` — pedindo permissão GPS
- `location_denied` — usuário negou GPS → mostrar "Para mostrar seu score, precisamos da sua localização. [Tentar de novo]"
- `loading_score` — fazendo fetch da API
- `error_404` — sem dado para a zona → "Nenhuma leitura para sua região hoje. Tente puxar para atualizar."
- `error_network` — `ErrorView` com botão "Tentar novamente"
- `success` — exibe gauge + recomendação

**Lógica:**
```typescript
function HomeScreen() {
  const { localizacao, pedirPermissao, erroLocalizacao } = useLocation();
  const { score, recomendacao, loading, erro, recarregar } = useScoreAtual(localizacao);

  if (erroLocalizacao === 'denied') return <PermissionDeniedView onRetry={pedirPermissao} />;
  if (!localizacao || loading) return <LoadingView mensagem="Buscando seu pulso urbano..." />;
  if (erro) return <ErrorView erro={erro} onRetry={recarregar} />;
  if (!score) return <EmptyView mensagem="Sem dados para sua região" />;

  return (
    <ScrollView refreshControl={<RefreshControl refreshing={loading} onRefresh={recarregar} />}>
      <Header titulo="Olá!" subtitulo={`📍 ${score.zonaNome}`} />
      <ScoreGauge valor={score.score} classificacao={score.classificacao} />
      <RecomendacaoCard recomendacao={recomendacao} />
      <Card>
        <DataSourceList score={score} />
      </Card>
    </ScrollView>
  );
}
```

---

### TELA 4 · MAPA (`app/(tabs)/mapa.tsx`)

**Wireframe:**
```
┌─────────────────────────────┐
│  Mapa de zonas        [📍]  │
├─────────────────────────────┤
│                             │
│                             │
│   ╔═══════════════════╗     │
│   ║                   ║     │
│   ║   [Mapa de SP]    ║     │
│   ║      🟢🟡🟠       ║     │
│   ║   🟠       🟡      ║     │
│   ║                   ║     │
│   ╚═══════════════════╝     │
│                             │
│  Tocar pin → modal:         │
│  ┌───────────────────┐      │
│  │ Zona Centro       │      │
│  │ Score: 55 (RUIM)  │      │
│  │ [Ver detalhes →]  │      │
│  └───────────────────┘      │
│                             │
└─────────────────────────────┘
```

**Comportamento:**
- Carrega `GET /score/zonas` ao montar
- Renderiza `MapView` centrado em São Paulo (-23.55, -46.63)
- Para cada zona, renderiza um `Marker` customizado (`ZonePin`) com cor pelo score
- Tocar pin → callout com info + botão "Ver detalhes" (push para `/(tabs)/zona/[id]`)
- Botão 📍 centraliza no usuário (se permissão concedida)

**Cuidados:**
- `MapView` no Expo Go: funciona em iOS, Android exige Google Maps API key em produção (no Development Build ou EAS Build com `expo-maps`)
- Para a entrega, **sem chave API**: usar `provider="default"` (Apple Maps no iOS, OpenStreetMap fallback no Android)
- Documentar isso no README

---

### TELA 5 · HISTÓRICO (`app/(tabs)/historico.tsx`)

**Wireframe:**
```
┌─────────────────────────────┐
│  Últimos 7 dias             │
├─────────────────────────────┤
│                             │
│  [Gráfico de barras horizontal ou linha]
│   ████████░░ 84  qua 25/05  │
│   ███████░░░ 72  qui 26/05  │
│   ██████░░░░ 62  sex 27/05  │
│   ...                       │
│                             │
├─────────────────────────────┤
│  Resumo                     │
│  Melhor dia: qua, 84 (BOM)  │
│  Pior dia: sex, 39 (CRÍT.)  │
│  Média da semana: 62.3      │
│                             │
└─────────────────────────────┘
```

**Componentes:** `FlatList` de `HistoricoItem`, ou `react-native-chart-kit` se quiser gráfico

**Estados:**
- Loading, erro, vazio, sucesso (padrão)

**Trade-off de tempo:**
- Versão mínima: lista de cards, cada um com barra horizontal proporcional ao score (5 min de UI)
- Versão completa: gráfico de linha real com `react-native-chart-kit` (1h)
- Decisão: **fazer versão mínima primeiro**, gráfico só se sobrar tempo

---

### TELA 6 · PERFIL (`app/(tabs)/perfil.tsx`) — implementa o CRUD

**Wireframe:**
```
┌─────────────────────────────┐
│  Perfil                     │
├─────────────────────────────┤
│                             │
│         [Avatar]            │
│       Felipe Ferrete         │
│   felipe@fiap.com.br        │
│                             │
├─────────────────────────────┤
│  Personalização             │
│  ☑ Pratico exercício físico │
│  ☐ Tenho criança em casa    │
│  ☐ Tenho problema respirat. │
│  [Salvar mudanças]          │
│                             │
├─────────────────────────────┤
│  [✏️ Editar nome/email]      │
│  [🚪 Sair da conta]          │
│  [🗑️ Excluir minha conta]    │
│                             │
└─────────────────────────────┘
```

**CRUD coberto aqui:**
- **READ** — carrega `GET /usuario/{id}` ao montar
- **UPDATE** — toque "Salvar mudanças" faz `PUT /usuario/{id}`
- **DELETE** — toque "Excluir conta" → confirm dialog → `DELETE /usuario/{id}` → logout
- **CREATE** — coberto pelo Register (TELA 2)

A **rubrica exige CRUD completo via API (30 pts)** — essa tela + Register cobre os 4 verbos.

---

### TELA 7 · DETALHES DA ZONA (`app/(tabs)/zona/[id].tsx`)

Tela de detalhe via deep link. Mostra:
- Nome da zona
- Score atual + classificação
- Gauge maior
- Recomendação completa
- Histórico dos últimos 3 dias daquela zona específica

---

## 23. Estados universais

**Todas as telas que fazem fetch DEVEM ter os 4 estados:**

```typescript
function MinhaScreen() {
  const { data, loading, error, refetch } = useFetchAlgo();

  if (loading) return <LoadingView />;
  if (error) return <ErrorView erro={error.message} onRetry={refetch} />;
  if (!data || data.length === 0) return <EmptyView />;
  return <ConteudoReal data={data} />;
}
```

### LoadingView padrão

```typescript
export function LoadingView({ mensagem = 'Carregando...' }: { mensagem?: string }) {
  return (
    <View style={styles.container}>
      <ActivityIndicator size="large" color={theme.colors.primary} />
      <Text style={styles.text}>{mensagem}</Text>
    </View>
  );
}
```

### ErrorView padrão

```typescript
export function ErrorView({ erro, onRetry }: { erro: string; onRetry?: () => void }) {
  return (
    <View style={styles.container}>
      <MaterialIcons name="error-outline" size={64} color={theme.colors.error} />
      <Text style={styles.title}>Ops!</Text>
      <Text style={styles.message}>{erro}</Text>
      {onRetry && <Button label="Tentar novamente" onPress={onRetry} />}
    </View>
  );
}
```

### EmptyView padrão

```typescript
export function EmptyView({ mensagem = 'Nada por aqui ainda', icone = 'inbox' }: Props) {
  return (
    <View style={styles.container}>
      <MaterialIcons name={icone} size={64} color={theme.colors.textMuted} />
      <Text style={styles.text}>{mensagem}</Text>
    </View>
  );
}
```

---

# PARTE 6 — FEATURES CRÍTICAS

## 24. Autenticação

### AuthContext

`src/contexts/AuthContext.tsx`:

```typescript
import React, { createContext, useEffect, useState, useCallback } from 'react';
import { getAuthToken, clearAuthToken } from '@/api/client';
import { router } from 'expo-router';

interface AuthContextValue {
  autenticado: boolean;
  carregando: boolean;
  usuarioId: number | null;
  logout: () => Promise<void>;
  refresh: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [autenticado, setAutenticado] = useState(false);
  const [carregando, setCarregando] = useState(true);
  const [usuarioId, setUsuarioId] = useState<number | null>(null);

  const refresh = useCallback(async () => {
    const token = await getAuthToken();
    setAutenticado(!!token);
    if (token) {
      // decode JWT para pegar usuarioId (sem validar — só extrair)
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        setUsuarioId(payload.usuarioId ?? null);
      } catch {
        setUsuarioId(null);
      }
    }
    setCarregando(false);
  }, []);

  const logout = useCallback(async () => {
    await clearAuthToken();
    setAutenticado(false);
    setUsuarioId(null);
    router.replace('/(auth)/login');
  }, []);

  useEffect(() => {
    refresh();
  }, [refresh]);

  return (
    <AuthContext.Provider value={{ autenticado, carregando, usuarioId, logout, refresh }}>
      {children}
    </AuthContext.Provider>
  );
}
```

`src/hooks/useAuth.ts`:

```typescript
import { useContext } from 'react';
import { AuthContext } from '@/contexts/AuthContext';

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth deve ser usado dentro de AuthProvider');
  return ctx;
}
```

---

## 25. Geolocalização

`src/contexts/LocationContext.tsx`:

```typescript
import React, { createContext, useState, useCallback } from 'react';
import * as Location from 'expo-location';

interface Localizacao {
  lat: number;
  lon: number;
}

interface LocationContextValue {
  localizacao: Localizacao | null;
  erroLocalizacao: 'denied' | 'unavailable' | null;
  carregando: boolean;
  pedirPermissao: () => Promise<void>;
}

export const LocationContext = createContext<LocationContextValue | null>(null);

export function LocationProvider({ children }: { children: React.ReactNode }) {
  const [localizacao, setLocalizacao] = useState<Localizacao | null>(null);
  const [erroLocalizacao, setErro] = useState<'denied' | 'unavailable' | null>(null);
  const [carregando, setCarregando] = useState(false);

  const pedirPermissao = useCallback(async () => {
    setCarregando(true);
    setErro(null);
    try {
      const { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== 'granted') {
        setErro('denied');
        return;
      }
      const pos = await Location.getCurrentPositionAsync({
        accuracy: Location.Accuracy.Balanced,
      });
      setLocalizacao({ lat: pos.coords.latitude, lon: pos.coords.longitude });
    } catch {
      setErro('unavailable');
      // Fallback: usar coordenadas do Centro de SP
      setLocalizacao({ lat: -23.5505, lon: -46.6333 });
    } finally {
      setCarregando(false);
    }
  }, []);

  return (
    <LocationContext.Provider value={{ localizacao, erroLocalizacao, carregando, pedirPermissao }}>
      {children}
    </LocationContext.Provider>
  );
}
```

### Permissão no `app.json`

```json
{
  "expo": {
    "plugins": [
      [
        "expo-location",
        {
          "locationAlwaysAndWhenInUsePermission": "Permita acesso à localização para mostrar o score da qualidade do ar na sua região."
        }
      ]
    ],
    "ios": {
      "infoPlist": {
        "NSLocationWhenInUseUsageDescription": "Permita acesso à localização para mostrar o score da qualidade do ar na sua região."
      }
    },
    "android": {
      "permissions": ["ACCESS_COARSE_LOCATION", "ACCESS_FINE_LOCATION"]
    }
  }
}
```

---

## 26. Mapa

### Setup no `app.json`

```json
{
  "expo": {
    "plugins": ["react-native-maps"]
  }
}
```

### Uso básico

```typescript
import MapView, { Marker, PROVIDER_DEFAULT } from 'react-native-maps';

<MapView
  provider={PROVIDER_DEFAULT}
  style={{ flex: 1 }}
  initialRegion={{
    latitude: -23.5505,
    longitude: -46.6333,
    latitudeDelta: 0.3,
    longitudeDelta: 0.3,
  }}
  showsUserLocation={true}
>
  {zonas.map((z) => (
    <Marker
      key={z.id}
      coordinate={{ latitude: z.lat, longitude: z.lon }}
      title={z.nome}
      description={`Score: ${z.score}`}
      pinColor={corDoScore(classificarScore(z.score))}
      onPress={() => router.push(`/(tabs)/zona/${z.id}`)}
    />
  ))}
</MapView>
```

⚠️ Em iOS dev (Expo Go), Apple Maps é usado. Em Android dev (Expo Go), OSM. Em produção (EAS Build) com `PROVIDER_GOOGLE`, exige API Key — fora do escopo da GS, manter `PROVIDER_DEFAULT`.

---

## 27. Forms (react-hook-form + zod)

`app/(auth)/login.tsx`:

```typescript
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { login } from '@/api/auth.api';
import { useAuth } from '@/hooks/useAuth';
import { useApiError } from '@/hooks/useApiError';

const schema = z.object({
  email: z.string().email('Email inválido'),
  senha: z.string().min(6, 'Senha deve ter no mínimo 6 caracteres'),
});

type FormData = z.infer<typeof schema>;

export default function LoginScreen() {
  const { refresh } = useAuth();
  const handleApiError = useApiError();
  const { control, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { email: '', senha: '' },
  });

  const onSubmit = async (data: FormData) => {
    try {
      await login(data);
      await refresh();
      router.replace('/(tabs)/home');
    } catch (e) {
      handleApiError(e, 'Falha no login');
    }
  };

  return (
    <View style={styles.container}>
      <Controller
        control={control}
        name="email"
        render={({ field: { onChange, value, onBlur } }) => (
          <Input
            label="Email"
            value={value}
            onChangeText={onChange}
            onBlur={onBlur}
            keyboardType="email-address"
            autoCapitalize="none"
            error={errors.email?.message}
            testID="login-email"
          />
        )}
      />
      <Controller
        control={control}
        name="senha"
        render={({ field: { onChange, value, onBlur } }) => (
          <Input
            label="Senha"
            value={value}
            onChangeText={onChange}
            onBlur={onBlur}
            secureTextEntry
            error={errors.senha?.message}
            testID="login-senha"
          />
        )}
      />
      <Button
        label="Entrar"
        onPress={handleSubmit(onSubmit)}
        loading={isSubmitting}
        testID="login-submit"
      />
    </View>
  );
}
```

---

## 28. Estado global

**Regra de thumb:**

- **Auth + Localização** → Context (precisa de toda a árvore)
- **Score atual / histórico** → useState local no componente que chama o hook
- **Form data** → react-hook-form
- **Cache de respostas** → não cachear no MVP (cada tela refaz fetch). Se sobrar tempo, AsyncStorage simples.

Não introduzir Redux, Zustand, TanStack Query, Recoil. Não há tempo nem necessidade.

---

# PARTE 7 — QUALIDADE

## 29. Performance

### Checklist obrigatório

- [ ] `FlatList` em vez de `map` para listas com 10+ itens (Histórico, Zonas)
- [ ] `keyExtractor` sempre presente
- [ ] `React.memo` em componentes de item de lista
- [ ] `useCallback` em funções passadas como props
- [ ] `useMemo` em cálculos pesados (transformar GeoJSON, agregar histórico)
- [ ] Imagens com tamanhos definidos (sem `width: '100%'` sem altura)
- [ ] `react-native-reanimated` para animações (não `Animated.timing` legado)
- [ ] Lazy load do mapa se ele estiver fora da tela inicial (geralmente já garantido por tab navigator)

---

## 30. Testes

### Setup

`jest.config.js`:
```javascript
module.exports = {
  preset: 'jest-expo',
  setupFilesAfterEach: ['@testing-library/jest-native/extend-expect'],
  transformIgnorePatterns: [
    'node_modules/(?!(jest-)?react-native|@react-native|expo(nent)?|@expo(nent)?/.*|@expo-google-fonts/.*|react-clone-referenced-element|@react-native-community|expo-router|expo-modules-core|@unimodules/.*|unimodules|sentry-expo|native-base|react-native-svg)'
  ],
};
```

### Cobertura mínima (não precisa ser alta — pontuação é em arquitetura)

- [ ] `Button` renderiza com loading
- [ ] `Input` mostra erro
- [ ] `useAuth` retorna `autenticado=false` quando não há token
- [ ] Login form valida email inválido
- [ ] Home renderiza loading antes de localização

### Exemplo de teste

```typescript
// __tests__/components/Button.test.tsx
import { render, fireEvent } from '@testing-library/react-native';
import { Button } from '@/components/Button';

describe('Button', () => {
  it('chama onPress quando pressionado', () => {
    const onPress = jest.fn();
    const { getByTestId } = render(<Button label="Click" onPress={onPress} testID="btn" />);
    fireEvent.press(getByTestId('btn'));
    expect(onPress).toHaveBeenCalledTimes(1);
  });

  it('não chama onPress quando disabled', () => {
    const onPress = jest.fn();
    const { getByTestId } = render(<Button label="Click" onPress={onPress} disabled testID="btn" />);
    fireEvent.press(getByTestId('btn'));
    expect(onPress).not.toHaveBeenCalled();
  });

  it('mostra spinner quando loading', () => {
    const { getByTestId, queryByText } = render(
      <Button label="Click" onPress={() => {}} loading testID="btn" />
    );
    expect(queryByText('Click')).toBeNull();
  });
});
```

---

## 31. Lint, format, type-check

### `.eslintrc.js`

```javascript
module.exports = {
  extends: ['expo', 'prettier'],
  plugins: ['prettier'],
  rules: {
    'prettier/prettier': 'error',
    '@typescript-eslint/no-explicit-any': 'error',
    '@typescript-eslint/no-unused-vars': ['warn', { argsIgnorePattern: '^_' }],
    'react-hooks/exhaustive-deps': 'warn',
  },
};
```

### `.prettierrc`

```json
{
  "semi": true,
  "singleQuote": true,
  "trailingComma": "es5",
  "tabWidth": 2,
  "printWidth": 100,
  "arrowParens": "always"
}
```

### Comandos no `package.json`

```json
"scripts": {
  "lint": "eslint . --ext .ts,.tsx",
  "lint:fix": "eslint . --ext .ts,.tsx --fix",
  "type-check": "tsc --noEmit",
  "format": "prettier --write \"**/*.{ts,tsx,json,md}\""
}
```

### Rodar antes de cada commit

```bash
npm run type-check && npm run lint && npm test -- --watchAll=false
```

---

# PARTE 8 — DEVOPS MOBILE

## 32. Setup pós-clone

### Comandos canônicos no README

```bash
# 1. Clonar
git clone https://github.com/<org>/pulso-mobile.git
cd pulso-mobile

# 2. Instalar dependências (versões travadas no package-lock.json)
npm ci

# 3. Configurar variáveis (não obrigatório para o mock)
cp .env.example .env
# editar .env e setar EXPO_PUBLIC_API_URL se for usar API real

# 4. Iniciar
npx expo start

# 5. Abrir no celular: escanear QR Code com app Expo Go (iOS App Store / Google Play)
# OU pressionar 'a' (Android emulator) ou 'i' (iOS simulator)
```

**Teste de sanidade — RODAR ANTES DE CADA PUSH:**

```bash
# Em pasta limpa, fora do git
mkdir /tmp/test-clone && cd /tmp/test-clone
git clone <url-do-repo> .
npm ci
npx expo start --no-dev --minify
# se aparecer QR Code, está OK
```

**Se isso falhar:** -50 pts. Não negociável.

### Problemas comuns e soluções

| Problema | Solução |
|---|---|
| "Cannot find module 'expo-router'" | Apagar `node_modules` e `package-lock.json`, `npm ci` |
| "expo-modules-core not built" | `npx expo prebuild --clean` |
| iOS Simulator não abre | `xcrun simctl boot "iPhone 15"` antes |
| Android Emulator não conecta | Verificar AVD criado, ADB rodando |
| MSW erro de polyfill | Adicionar `react-native-url-polyfill` em deps |
| "Failed to load Metro config" | `npm cache clean --force && npm ci` |

---

## 33. Build

### Opções e quando usar cada

| Tipo | Como rodar | Quando |
|---|---|---|
| **Expo Go** | `npx expo start` → escaneia QR | Desenvolvimento dia a dia |
| **Development Build** | `npx expo run:android` (precisa Android Studio) | Quando precisa de native modules além do que Expo Go suporta |
| **EAS Build (cloud)** | `eas build --profile preview --platform android` | Para gerar APK para distribuição/avaliação |

### EAS Build (recomendado para entrega final)

```bash
# 1. Login EAS (uma vez)
npm install -g eas-cli
eas login

# 2. Configurar (na raiz do projeto)
eas build:configure

# 3. Build de preview (APK para Android — mais simples para avaliador testar)
eas build --profile preview --platform android

# 4. Acompanhar build em https://expo.dev/accounts/<seu-user>/projects/pulso-mobile/builds

# 5. Quando terminar, baixar APK e anexar no README como link
```

`eas.json`:
```json
{
  "cli": { "version": ">= 5.0.0" },
  "build": {
    "preview": {
      "android": { "buildType": "apk" },
      "ios": { "simulator": true }
    },
    "production": {
      "android": { "buildType": "apk" }
    }
  }
}
```

⚠️ EAS Build tem cota free limitada. Faça no máximo 2–3 builds de preview na semana da entrega.

---

## 34. README

### Estrutura obrigatória (a rubrica avalia)

```markdown
# Pulso Urbano Mobile

App React Native para visualização de score de qualidade do ar
em São Paulo usando dados orbitais (Sentinel-5P + ECOSTRESS).

Projeto da Global Solution 2026/1 — FIAP ADS.

## 🎯 Solução proposta

[1-2 parágrafos explicando o problema e a solução, mesma narrativa do CONTEXT.md]

## 👥 Equipe

| Nome | RM | Turma |
|---|---|---|
| Felipe Ferrete | 562999 | 2TDSPF |
| Guilherme [Sobrenome] | [RM] | [Turma] |
| [outros] | | |

## 📺 Vídeo demonstração

[Link YouTube do vídeo de até 5 minutos]

## 🚀 Como executar

### Pré-requisitos
- Node.js 18+ (LTS recomendado)
- npm 9+ ou yarn 1.22+
- App Expo Go no celular (iOS App Store / Google Play)
- (Opcional) Android Studio ou Xcode para emulador

### Instalação

\`\`\`bash
git clone https://github.com/<org>/pulso-mobile.git
cd pulso-mobile
npm ci
npx expo start
\`\`\`

Escaneie o QR Code com o app Expo Go.

### Variáveis de ambiente

Copie `.env.example` para `.env` e configure:

\`\`\`env
EXPO_PUBLIC_API_URL=https://pulso-urbano-562999.fly.dev/api/v1
\`\`\`

Se deixar em branco, o app usa **mock API local** (recomendado para avaliação).

## 🏗️ Arquitetura

[Diagrama em ASCII ou imagem mostrando: Mobile → Java API → Oracle/Satellites]

### Stack
- Expo SDK 52
- TypeScript 5.3
- Expo Router 4 (navegação)
- Axios (HTTP)
- React Hook Form + Zod (forms)
- react-native-maps (mapa)
- expo-secure-store (JWT)

### Estrutura de pastas
[Cole o tree da seção 9 deste documento]

## 📱 Telas

| # | Tela | Descrição |
|---|---|---|
| 1 | Login | Autenticação JWT |
| 2 | Register | Cadastro com personalização (exercício, criança, asma) |
| 3 | Home | Score atual + recomendação personalizada |
| 4 | Mapa | Pins por zona com cor pelo score |
| 5 | Histórico | Últimos 7 dias com gráfico |
| 6 | Perfil | CRUD do usuário + logout |
| 7 | Detalhes da Zona | Informações específicas de uma zona |

## 🔌 Integração com Java API

[Link para o repositório da Java API]

Endpoints consumidos:
- POST /auth/login, /auth/register
- GET /score/current, /score/historico, /score/zonas
- GET /recomendacao
- CRUD /usuario/{id}

## 🧪 Testes

\`\`\`bash
npm test
\`\`\`

## 📋 Critérios da rubrica (FIAP)

- [x] 5+ telas com navegação (Expo Router)
- [x] CRUD via API (Axios)
- [x] Estilização personalizada (theme tokens)
- [x] Arquitetura organizada (src/api, src/components, src/hooks, src/contexts)
- [x] Vídeo demonstrativo
- [x] README completo
- [x] Histórico Git evolutivo

## 📄 Licença

MIT
```

---

# PARTE 9 — RUBRICA FIAP MAPEADA

## 35. Cobertura de cada critério

| Critério (peso) | Como o backlog cobre |
|---|---|
| **5+ telas com navegação (10 pts)** | 7 telas spec'd na seção 22; Expo Router file-based |
| **CRUD via API (30 pts)** | Register (CREATE), Perfil GET/PUT/DELETE; Axios padrão |
| **Estilização (5 pts)** | Theme tokens (cores, espaçamento), ícones MaterialIcons, fonte sistema |
| **Arquitetura (10 pts)** | Pastas separadas, TypeScript estrito, naming conventions, ESLint/Prettier |
| **Vídeo demo (15 pts)** | Roteiro pré-pronto (seção 37); ≤5min |

**Subtotal: 70 pts (entrega)** + 30 pts apresentação = **100 pts**

---

## 36. Penalidades

| Penalidade | Pontos perdidos | Como evitar |
|---|---|---|
| Não entregar via GitHub Classroom | **-20** | Configurar GitHub Classroom no DIA 1 |
| Ausência README.md | **-10** | Já especificado na seção 34 |
| Aplicativo fora do escopo | **-40** | Manter foco em qualidade do ar / satellite data |
| Histórico Git incoerente | **-40** | Conventional Commits + 1+ commit/dia |
| **App não executa após `git clone`** | **-50** | Testar fluxo da seção 32 ANTES de cada push |
| Sem 5 telas mínimas | **-10** | 7 telas planejadas, buffer de 2 |
| Sem estilização | **-20** | Theme tokens obrigatórios |

**Pior cenário acumulado se ignorar tudo: -190 pts. A nota mínima é 0.**

---

## 37. Apresentação presencial

A defesa em sala vale **30 pts**. Critérios:

1. **Apresentação da solução** (problema + público-alvo + clareza)
2. **Demonstração prática** (app rodando ao vivo, CRUD, navegação)
3. **Explicação técnica** (estrutura, componentes, integração)
4. **Domínio individual** (cada integrante responde sobre o código)

### Roteiro sugerido (5 min de demo + 5 min Q&A)

```
1. (1 min) Problema:
   "Em São Paulo, milhões de pessoas decidem se vão correr, levar
   o filho ao parque, sair de bike — sem saber a qualidade do ar.
   O dado existe, no Sentinel-5P. Falta um app que traduza isso."

2. (30s) Quem é o usuário:
   "Quem faz exercício, quem tem criança, quem tem asma."

3. (3 min) Demo viva:
   - Abre o app, faz login (Felipe@fiap.com / senha123)
   - Mostra home com score 62 MODERADO + recomendação personalizada
   - Abre mapa, mostra zonas coloridas, toca em uma, vê detalhes
   - Vai em histórico, mostra os 7 dias
   - Vai em perfil, edita flag de criança, salva, mostra que
     recomendação muda na home (CRUD funcionando)
   - Faz logout, mostra que volta para login

4. (1 min) Arquitetura:
   - "Mobile consome Java API via Axios com JWT no expo-secure-store"
   - "Mock MSW para desenvolvimento antes da API real"
   - "Estado via Context (Auth + Location), forms com Zod"
   - Mostra o tree de pastas
```

### Perguntas que vão fazer (prepare respostas)

- "Por que escolheram Expo em vez de React Native CLI?"
  → "Reduz risco de build, EAS Build pronto, todo o time consegue rodar sem Android Studio"

- "Como o app trata token expirado?"
  → "Interceptor axios captura 401, limpa secure-store, redireciona pra login. Mostro o código."

- "Como vocês testaram com a API que ainda não estava pronta?"
  → "MSW interceptando axios com os mesmos contratos do backend Java; no dia 04/06 plugamos a API real só mudando a base URL."

- "O que mudariam se pudessem refazer?"
  → "Adicionaríamos TanStack Query para cache automático e refetch em foco; faríamos onboarding mais educativo sobre o que é Sentinel-5P."

---

# PARTE 10 — PROMPTS PRONTOS

## 38. Template universal de task mobile

Cole este template antes de qualquer task específica:

```
CONTEXTO: [colar Parte 1 + Parte 2 + Parte 3 desta bíblia]

TASK: [descrever em 1 frase o que entregar]

CONTRATOS RELEVANTES:
[colar APENAS o endpoint da seção 14 que essa task consome]

ESTADO ATUAL:
- O que já foi commitado: [lista]
- O que ainda não foi: [lista]

ARQUIVO(S) A CRIAR/MODIFICAR:
- src/...

RESTRIÇÕES:
- Use o theme da seção 17
- Use componentes prontos (Button, Input, Card)
- Implemente os 4 estados: loading, error, empty, success
- TypeScript estrito (sem any)
- testID em elementos críticos

ENTREGUE: apenas o código completo dos arquivos. Sem explicação textual.
```

---

## 39. Prompts por feature

### Prompt: Implementar tela de Login

```
CONTEXTO: [cole Parte 1 + seção 13 + seção 17 + seção 27]

TASK: Implementar a tela de Login conforme especificação da TELA 1 (seção 22).

ARQUIVO A CRIAR: app/(auth)/login.tsx

DEPENDÊNCIAS QUE JÁ EXISTEM:
- src/api/auth.api.ts (com função `login()`)
- src/api/client.ts (com axios + interceptors)
- src/components/Button.tsx
- src/components/Input.tsx
- src/constants/theme.ts
- src/contexts/AuthContext.tsx (com hook `useAuth`)

REQUISITOS:
1. Form com email e senha (validação zod)
2. Botão "Entrar" com loading state
3. Link "Não tem conta? Cadastre-se" → router.push('/(auth)/register')
4. Em sucesso: refresh do AuthContext + router.replace('/(tabs)/home')
5. Em erro 401: toast "Credenciais inválidas"
6. Em erro 4xx/5xx/network: toast com mensagem apropriada
7. testID: 'login-email', 'login-senha', 'login-submit', 'login-register-link'

ENTREGUE: o arquivo completo, pronto pra copiar.
```

### Prompt: Implementar tela Home

```
CONTEXTO: [cole Parte 1 + seção 14 (endpoint /score/current) + seções 23 a 25]

TASK: Implementar TELA 3 — Home (seção 22).

ARQUIVO A CRIAR: app/(tabs)/home.tsx
SUBCOMPONENTES A CRIAR (se ainda não existem):
- src/components/ScoreGauge.tsx
- src/components/RecomendacaoCard.tsx

REQUISITOS:
1. Ao montar: pede permissão de localização via useLocation
2. Quando tiver lat/lon: chama GET /score/current
3. Estados: loading, error, empty, success — todos visuais
4. ScoreGauge: círculo grande com o número e cor por classificação
5. RecomendacaoCard: ícone (MaterialIcons mapeado) + texto
6. Pull-to-refresh: RefreshControl chamando recarregar
7. Mostrar fonte do dado em footer pequeno

ENTREGUE: arquivos completos.
```

### Prompt: Setup inicial do projeto

```
CONTEXTO: [cole seções 7 a 11 + 17 + 32]

TASK: Inicializar o projeto Expo SDK 52 com TypeScript do zero.

PASSOS A EXECUTAR (gere o script bash):
1. npx create-expo-app@latest pulso-mobile --template
2. Instalar todas as dependências da seção 8 com versões exatas
3. Criar estrutura de pastas da seção 9
4. Criar arquivos: theme.ts, config.ts, client.ts, AuthContext.tsx
5. Criar app.json com plugins (expo-location, react-native-maps)
6. Criar tsconfig.json com path aliases @/* → src/*
7. Criar .eslintrc.js e .prettierrc da seção 31
8. Criar README.md base
9. git init + primeiro commit "chore(scaffold): initialize Expo project"

ENTREGUE: o script bash completo + conteúdo de cada arquivo de config.
```

### Prompt: Mock API com MSW

```
CONTEXTO: [cole seções 14 + 16]

TASK: Implementar mock API completo com MSW cobrindo todos os endpoints.

ARQUIVOS A CRIAR:
- src/mocks/handlers.ts
- src/mocks/server.ts
- src/mocks/fixtures.ts (dados fake)

REQUISITOS:
1. Todos os endpoints da seção 14 mockados
2. Persistência em memória (POST de usuário aparece em GET)
3. Latência simulada de 300ms em cada response
4. Mensagens de erro idênticas ao backend (ErrorResponseDTO)
5. Pelo menos 5 zonas de SP com scores variados (BOM/MOD/RUIM/CRIT)

ENTREGUE: arquivos completos.
```

---

## ESTADO ATUAL DO PROJETO MOBILE

```
Decisão: TOMADA — Expo SDK 52 + TypeScript + Expo Router
Mock API: MSW configurado para uso até API Java estar deployada
Stack: react-hook-form + zod, axios, react-native-maps, expo-location, expo-secure-store

Status por arquivo:
  [ ] Scaffold do projeto
  [ ] package.json com versões travadas
  [ ] theme.ts + tokens visuais
  [ ] Componentes Button/Input/Card/LoadingView/ErrorView/EmptyView
  [ ] AuthContext + LocationContext
  [ ] Mock MSW
  [ ] Telas: Login, Register, Home, Mapa, Histórico, Perfil
  [ ] Tela de detalhes da zona
  [ ] README com tudo
  [ ] Build EAS preview
  [ ] Vídeo demo
```

---

## REGRAS FINAIS PARA AGENTES

Quando um agente de IA receber esta bíblia para implementar algo, ele deve:

1. **Confirmar leitura** com algo como "Li o contexto do Pulso Urbano Mobile. Vou implementar X seguindo a stack travada e os contratos da seção 14."
2. **Não questionar decisões já tomadas** (Expo vs RN CLI, Context vs Redux, etc.) — só executar
3. **Não inventar endpoints** — só usar os da seção 14
4. **Não inventar dependências** — só usar as da seção 8
5. **Não esquecer estados universais** (loading/error/empty/success)
6. **Não esquecer TypeScript estrito** (sem any)
7. **Não esquecer testID** em elementos críticos
8. **Não esquecer acessibilidade** (accessibilityLabel, accessibilityRole)
9. **Sempre usar tokens do theme** — nunca cor hardcoded
10. **Sempre tratar erros** — nunca deixar promise sem catch

---

*Bíblia gerada em 27/05/2026 · Versão 1.0 · Felipe Ferrete RM 562999*
*Manter atualizado conforme o projeto evolui — especialmente "Estado atual" e seção 38/39 de prompts*
