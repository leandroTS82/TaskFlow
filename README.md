# TaskFlow

Orquestrador de jobs distribuído construído com .NET 8 — alta disponibilidade, entrega garantida, processamento ordenado por prioridade.

---

## Visão Geral

O TaskFlow recebe tarefas via API e garante que nenhuma seja perdida mesmo em caso de falha de infraestrutura. Os jobs são persistidos atomicamente junto com uma mensagem de outbox, publicados no RabbitMQ por um processor independente e consumidos por um worker que atualiza o status final.

```
Cliente → API → MongoDB (Job + Outbox)
                    ↓
              Processor → RabbitMQ → Worker → Completed
```

Arquitetura completa e decisões de design: [ARCHITECTURE.md](ARCHITECTURE.md)

---

## Stack

- **.NET 8 LTS** — runtime
- **MongoDB** — NoSQL com Replica Set para transações
- **RabbitMQ + MassTransit** — mensageria
- **Mediator (MIT)** — CQRS
- **FluentValidation** — validação de entrada
- **Polly** — retry + circuit breaker
- **Serilog** — logs estruturados
- **Docker Compose** — infraestrutura local

---

## Como executar

### Requisitos

- Docker Desktop instalado e rodando
- .NET 8 SDK

### Subir a infraestrutura

```bash
docker-compose up -d
```

| Serviço | Endereço |
|---|---|
| MongoDB | localhost:27017 |
| RabbitMQ management | localhost:15672 (guest/guest) |
| API | localhost:8080/swagger |

### Rodar os projetos

Configure Multiple Startup Projects no Visual Studio ou rode cada um em um terminal separado:

```bash
# Terminal 1
cd src/TaskFlow.API && dotnet run

# Terminal 2
cd src/TaskFlow.Processor && dotnet run

# Terminal 3
cd src/TaskFlow.Worker && dotnet run
```

---

## Endpoints da API

Todos os endpoints requerem o header `X-Api-Key`.

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/jobs` | Cria um job — suporta header `Idempotency-Key` |
| `GET` | `/api/jobs/{id}` | Retorna o status do job pelo ID |
| `GET` | `/api/jobs` | Lista todos os jobs |
| `POST` | `/api/jobs/{id}/cancel` | Cancela um job em Pending ou Running |
| `GET` | `/health` | Health check de MongoDB e RabbitMQ |

### Exemplo de requisição

```bash
curl -X POST http://localhost:8080/api/jobs \
  -H "X-Api-Key: dev-api-key-example" \
  -H "Idempotency-Key: chave-unica-cliente-001" \
  -H "Content-Type: application/json" \
  -d '{"jobType": "SendEmail", "priority": 1, "payload": "{\"to\": \"usuario@email.com\"}"}'
```

---

## Docker

### Iniciar

```bash
docker-compose up -d
```

### Parar e remover

```bash
docker-compose down -v
```

### Problema conhecido — Replica Set não inicializado

Em alguns ambientes o container `taskflow-mongo-init` pode falhar ao inicializar o Replica Set automaticamente. Sintoma: a API retorna `MongoNotPrimaryException`.

Para corrigir manualmente:

```bash
docker exec -it taskflow-mongo mongosh -u root -p password --authenticationDatabase admin --eval "rs.initiate({ _id: 'rs0', members: [{ _id: 0, host: 'localhost:27017' }] })"
```

Para confirmar que está funcionando:

```bash
docker exec -it taskflow-mongo mongosh -u root -p password --authenticationDatabase admin --eval "rs.status().myState"
```

Retorno esperado: `1` (Primary).

### Logs

```bash
# MongoDB
docker logs taskflow-mongo --follow

# API
docker logs taskflow-api --follow

# Mongo Init
docker logs taskflow-mongo-init --follow
```

---

## Estrutura do Projeto

```
TaskFlow/
├── src/
│   ├── TaskFlow.API            # Web API — ponto de entrada
│   ├── TaskFlow.Application    # Commands, Queries, Handlers
│   ├── TaskFlow.Domain         # Entidades, Interfaces, Exceções
│   ├── TaskFlow.Infrastructure # Repositories, MongoDB, MassTransit
│   ├── TaskFlow.Processor      # Outbox → RabbitMQ
│   └── TaskFlow.Worker         # RabbitMQ → processa job
├── tests/
│   └── TaskFlow.Tests          # Testes unitários
├── docker-compose.yml
├── ARCHITECTURE.md
└── README.md
```

---

## Arquitetura

O sistema segue Clean Architecture + DDD + CQRS. Documentação completa com ADRs e diagramas C4: [ARCHITECTURE.md](ARCHITECTURE.md)

---

## Testes

```bash
cd tests/TaskFlow.Tests
dotnet test
```

Testes unitários cobrem entidades do Domain e handlers da Application usando xUnit, FluentAssertions e NSubstitute.

---

## Planejamento Inicial
Rascunho inicial — plano de ação e adaptação baseada em Kanban
| | |
|---|---|
| <img width="400" src="https://github.com/user-attachments/assets/6918c74c-29ea-4c5b-94d5-4f5e09c5b537" /> | <img width="400" src="https://github.com/user-attachments/assets/9c30bf90-d72f-4092-aca9-cf6f782be884" /> |

