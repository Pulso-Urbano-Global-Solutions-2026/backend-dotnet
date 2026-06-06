---
name: Bug Report — QA
about: Reporte um bug encontrado durante os testes de QA da API .NET
title: "[BUG] <descrição curta>"
labels: bug, qa
assignees: ''
---

## Passos para reproduzir

1. 
2. 
3. 

## Resultado esperado

<!-- O que deveria acontecer -->

## Resultado atual

<!-- O que aconteceu de fato -->

## Endpoint + Status Code

| Campo | Valor |
|---|---|
| Método | `GET / POST / PUT / DELETE` |
| Rota | `/api/alertas/...` |
| Status recebido | `4xx / 5xx` |
| Status esperado | `2xx / 4xx` |

```bash
# Comando curl usado para reproduzir (remova tokens reais):
curl -s -X METHOD http://localhost:5000/api/...
```

## Resposta da API

```json
{
  "status": 0,
  "erro": "",
  "mensagem": ""
}
```

## Screenshot / Log

<!-- Cole aqui o screenshot ou trecho de log relevante -->

## Ambiente

- [ ] Local (dotnet run)
- [ ] Docker (docker run)
- [ ] Compose completo (Oracle + Java + .NET)

**Versão / commit:** 
