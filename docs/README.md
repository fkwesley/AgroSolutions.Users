﻿# 👤 Common.Users API - Hackaton FIAP

> API RESTful para gestão de usuários e autenticação JWT, com persistência em SQL Server, observabilidade completa e deploy automatizado em AKS.

> [Vídeo de Apresentação](https://youtu.be/gQLOlJ2EWxc)

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C# 12](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![REST Level 3](https://img.shields.io/badge/REST-Level%203%20(HATEOAS)-success)](https://martinfowler.com/articles/richardsonMaturityModel.html)

---

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Arquitetura](#-arquitetura)
- [Funcionalidades](#-funcionalidades)
- [API RESTful - Nível 3](#-api-restful---nível-3)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Princípios SOLID](#-princípios-solid)
- [Tecnologias](#-tecnologias)
- [CI/CD](#-cicd)
- [Setup Rápido](#-setup-rápido)
- [Testes](#-testes)

---

## 🎯 Visão Geral

Microserviço responsável pela gestão de usuários e autenticação JWT, com persistência em SQL Server (Entity Framework Core), logging multi-destino via Serilog e health checks dinâmicos.

### 🌟 Destaques

- ✅ **Clean Architecture** (Onion Architecture)
- ✅ **REST Level 3** (HATEOAS completo)
- ✅ **SOLID Principles** aplicados rigorosamente
- ✅ **Domain-Driven Design** (DDD)
- ✅ **SQL Server** com Entity Framework Core 8 (Code-First + Migrations)
- ✅ **JWT Authentication** com roles (Admin/User) e contas técnicas
- ✅ **Password Hashing** com validação de senha forte
- ✅ **Health Checks** dinâmicos com auto-discovery
- ✅ **Observabilidade completa** (Logs estruturados, Correlation IDs, Elastic APM)
- ✅ **Testes em 4 camadas** (Unit, Integration, Architecture, Load)

---

### 📌 Requisitos Hackaton FIAP
  - **Arquitetura baseada em microserviços**
    - Microserviço para gestão de usuários e autenticação JWT (banco SQL) ← **Este microserviço**
    - Microserviço para gestão de fazendas, talhões e safras (banco SQL)
    - Microserviço para ingestão dos dados dos sensores (banco NoSQL - Requisito Opcional)
    - Funções Serverless para coleta de dados dos sensores (integração api de previsão do tempo - Requisito Opcional)
  - **Adoção das melhores práticas de arquitetura e dev**
    - Clean Architecture (Onion Architecture)
    - API RESTful Level 3 (HATEOAS completo)
    - SOLID Principles aplicados rigorosamente
    - Patterns como Repository, Unit of Work, Factory, Strategy
    - Domain-Driven Design (DDD)
    - Health Checks dinâmicos
    - Observabilidade completa (Logs estruturados, Correlation IDs)
    - Testes em 4 camadas (Unit, Integration, Architecture, Load)
  - **Orquestração com Kubernetes**
    - Imagens Docker otimizadas para .NET 8 (Alpine)
    - Armazenamento das imagens no Azure Container Registry (ACR)
    - Microserviços hospedados em Azure Kubernetes Services (AKS)
    - Manifestos Kubernetes para deploy, service, hpa, configMap e secrets
  - **Mensageria**
    - ServiceBus para comunicação assíncrona entre microserviços
  - **CI/CD Automatizado**
    - Github Actions para build, testes, build de imagem Docker e deploy no AKS
  - **Observabilidade**
    - Elastic APM para monitoramento de performance e rastreamento distribuído
    - Elasticsearch para armazenamento e análise de logs estruturados
    - Kibana para dashboards

---

## 🏗️ Arquitetura

### Clean Architecture (Onion)

```
┌─────────────────────────────────────────────┐
│              API (Presentation)             │  ← Controllers, Middlewares
├─────────────────────────────────────────────┤
│           Application (Use Cases)           │  ← Services, DTOs, Mappings
├─────────────────────────────────────────────┤
│              Domain (Core)                  │  ← Entities, Enums, Exceptions
├─────────────────────────────────────────────┤
│          Infrastructure (External)          │  ← SQL Server, Serilog, Health Checks
└─────────────────────────────────────────────┘
```

**Dependency Rule:** Domain ← Application ← Infrastructure ← API


### Fluxo de Processamento

```
         Cliente → API (POST /v1/auth/login)
           ↓
         AuthController → UserService (validação de credenciais)
           ↓
         AuthService (geração de JWT token)
           ↓
         Token JWT retornado ao cliente
           ↓
         Cliente → API (GET/POST/PUT/DELETE /v1/users) [Authorize(Roles = "Admin")]
           ↓
         UsersController → UserService (orquestração)
           ↓
         UserRepository → SQL Server (persistência via EF Core)
           ↓
         Response com HATEOAS links
```

### Padrões Utilizados

- **Clean Architecture**: Separação clara de responsabilidades
- **SOLID Principles**: Código manutenível e testável
- **Repository Pattern**: Abstração de acesso a dados (IUserRepository, IPasswordHasherRepository)
- **Mapping Extensions**: Conversão Entity ↔ DTO via extension methods
- **Strategy Pattern**: Logging multi-destino (Database/Elastic/NewRelic) via configuração

**Benefícios:**
- ✅ Domain independente de infraestrutura
- ✅ Fácil substituição de frameworks/bancos
- ✅ Testável sem dependências externas
- ✅ Escalável e manutenível

---

## 👤 Funcionalidades

- **Gestão de Usuários**: CRUD completo (criar, listar, buscar por ID, atualizar, deletar)
- **Autenticação JWT**: Login com geração de token JWT (roles Admin/User, contas técnicas com token sem expiração)
- **Validação de Senha Forte**: Mínimo 8 caracteres, letras, números e caracteres especiais
- **Validação de E-mail**: Formato de e-mail validado na entidade de domínio
- **Password Hashing**: Hashing seguro de senhas via IPasswordHasherRepository
- **Armazenamento SQL Server**: Persistência via Entity Framework Core 8 (Code-First + Migrations)
- **Health Checks**: Database, Elasticsearch, System (auto-discovery)
- **Logging Multi-destino**: Database, Elasticsearch, New Relic (via Serilog)
- **Correlation IDs**: Rastreamento distribuído entre requisições
- **Security Headers**: HSTS, CSP, X-Frame-Options e outros headers de segurança

---

## 🌐 API RESTful - Nível 3 (HATEOAS)

### Richardson Maturity Model
```
Nível 3: HATEOAS     ← ✅ Esta API
Nível 2: HTTP Verbs  ← ✅
Nível 1: Resources   ← ✅
Nível 0: POX
```

### Endpoints

| Método | Rota | Descrição | Auth |
|--------|------|-----------|------|
| `POST` | `/v1/auth/login` | Autenticação e geração de JWT | Público |
| `GET` | `/v1/users` | Listar todos os usuários | Admin |
| `GET` | `/v1/users/{userId}` | Buscar usuário por ID | Admin |
| `POST` | `/v1/users` | Criar novo usuário | Admin |
| `PUT` | `/v1/users/{userId}` | Atualizar usuário | Admin |
| `DELETE` | `/v1/users/{userId}` | Deletar usuário | Admin |
| `GET` | `/v1/health` | Health check com detalhes dos componentes | Público |

### Requisitos REST Implementados

| Requisito | Descrição | Status | Padrão |
|-----------|-----------|--------|--------|
| **URIs substantivos** | Recursos com substantivos no plural | ✅ | `/users` |
| **HTTP Verbs** | GET, POST, PUT, DELETE corretos | ✅ | Semântica HTTP |
| **Idempotência** | GET, PUT, DELETE idempotentes | ✅ | RFC 7231 |
| **Status Codes** | 2xx, 4xx, 5xx apropriados | ✅ | HTTP Standards |
| **HATEOAS** | Links de navegação em respostas | ✅ | Richardson Level 3 |
| **Versionamento** | URL versioning | ✅ | `/v1/` |
| **Content Negotiation** | Accept/Content-Type headers | ✅ | `application/json` |
| **Error Handling** | Respostas padronizadas de erro | ✅ | ErrorResponse |
| **Stateless** | Sem estado no servidor | ✅ | JWT tokens |
| **CORS** | Cross-Origin Resource Sharing | ✅ | Configurável |
| **Correlation IDs** | Rastreamento distribuído | ✅ | `X-Correlation-ID` |

### REST Constraints (Roy Fielding)

| Constraint | Status |
|-----------|--------|
| Client-Server | ✅ Separação de responsabilidades |
| Stateless | ✅ Sem sessão, requisições auto-contidas |
| Cacheable | ✅ Headers de cache (NoCacheMiddleware) |
| Layered System | ✅ Load Balancer → Gateway → API → SQL Server |
| Uniform Interface | ✅ URIs padronizadas, HATEOAS |
| Code on Demand | ⚠️ Opcional (não implementado) |

---

## 📁 Estrutura do Projeto

```
Common.Users/
│
├── 📂 API/                          # Presentation Layer
│   ├── Controllers/v1/                 # UsersController, AuthController, HealthController
│   ├── Middlewares/                    # ErrorHandling, RequestLogging, SecurityHeaders, NoCache, ApiVersion
│   ├── Configurations/                 # DI, Swagger, CORS, Auth, Versioning, Validation, Logging
│   ├── Helpers/                        # HateoasHelper
│   ├── Models/                         # ErrorResponse
│   ├── Program.cs                      # Entry point (Serilog bootstrap)
│   ├── appsettings.json                # Configurações de produção
│   └── appsettings.Development.json    # Configurações de desenvolvimento
│
├── 📂 Application/                  # Use Cases
│   ├── Services/                       # UserService, AuthService, HealthCheckService
│   ├── Interfaces/                     # Contratos (IUserService, IAuthService, IHealthCheckService, IHealthCheck, ILoggerService)
│   ├── Mappings/                       # UserMappingExtensions (User ↔ DTO)
│   ├── DTO/                            # Request/Response DTOs
│   │   ├── User/                       # AddUserRequest, UpdateUserRequest, UserResponse
│   │   ├── Auth/                       # LoginRequest, LoginResponse
│   │   ├── Health/                     # HealthResponse
│   │   └── Common/                     # Link, IHateoasResource
│   ├── Exceptions/                     # ValidationException
│   ├── Helpers/                        # DateTimeHelper
│   └── Settings/                       # LoggerSettings, ElasticLoggerSettings, NewRelicLoggerSettings, ExternalLoggerSettings
│
├── 📂 Domain/                       # Core Business
│   ├── Entities/                       # User, RequestLog
│   ├── Enums/                          # LogLevel
│   ├── Exceptions/                     # BusinessException
│   └── Repositories/                   # IUserRepository, IPasswordHasherRepository
│
├── 📂 Infrastructure/               # External Concerns
│   ├── Context/                        # UsersDbContext (EF Core), CorrelationContext
│   ├── Configurations/                 # UserConfiguration, RequestLogConfiguration (EF Core Fluent API)
│   ├── Migrations/                     # EF Core Migrations (Code-First)
│   ├── Repositories/                   # UserRepository, PasswordHasherRepository, DatabaseLoggerRepository
│   ├── Interfaces/                     # IDatabaseLoggerRepository
│   └── Services/
│       ├── Logging/                    # DatabaseLoggerService, ElasticLoggerService, NewRelicLoggerService
│       └── HealthCheck/                # DatabaseHealthCheck, ElasticsearchHealthCheck, SystemHealthCheck
│
├── 📂 Tests/                        # Tests Layer
│   ├── UnitTests/
│   │   ├── Domain/Entities/            # UserTests
│   │   ├── Application/Services/       # UserServiceTests
│   │   ├── Application/Mappings/       # UserMappingExtensionsTests
│   │   └── Application/Helpers/        # DateTimeHelperTests
│   ├── ArchitectureTests/              # LayerDependency, SolidPrinciples, NamingConvention, SecurityAndQuality, ApiDesign, Performance, Testability
│   ├── IntegrationTests/
│   │   ├── Common/                     # CustomWebApplicationFactory, DatabaseSeeder
│   │   └── Infrastructure/Repositories/# UserRepositoryTests
│   └── LoadTests/                      # k6 load testing (load-test.js)
│
├── 📂 docs/                         # Documentation
│   ├── SOLID_Summary.md                # Análise SOLID detalhada
│   ├── Architecture.drawio             # Diagramas de arquitetura
│   └── README.md                       # Este arquivo
│
├── 📂 Kubernetes/                   # Kubernetes manifests
│   ├── deployment.yaml                 # Deployment do microserviço
│   ├── service.yaml                    # Service (ClusterIP/LoadBalancer)
│   ├── hpa.yaml                        # Horizontal Pod Autoscaler
│   ├── configmap.yaml                  # ConfigMap
│   ├── secret.yaml                     # Secrets
│   ├── namespace.yaml                  # Namespace
│   └── AzureAKS_script.txt            # Scripts auxiliares AKS
│
├── 📂 .github/                      # GitHub workflows
│   └── workflows/ci-cd-aks.yml        # CI/CD para AKS
│
├── .gitignore                       # Arquivos ignorados pelo Git
├── Dockerfile                       # Imagem Docker da API (multi-stage, Alpine)
└── Common.Users.sln                 # Solution .NET
```

---

## ✨ Funcionalidades Principais

### 🔹 Gestão de Usuários (Users)
- CRUD completo de usuários (GET, POST, PUT, DELETE)
- Validações de negócio (e-mail, senha forte, campos obrigatórios)
- Persistência em SQL Server via Entity Framework Core 8 (Code-First)
- Links HATEOAS para navegação entre recursos
- Roles: Admin e User

### 🔹 Autenticação JWT (Auth)
- Login com validação de credenciais (userId + password)
- Geração de JWT Token com claims (sub, user_id, user_email, name, role)
- Tokens com expiração de 1 hora (usuários normais) ou sem expiração (contas técnicas)
- Autorização baseada em roles (`[Authorize(Roles = "Admin")]`)

### 🔹 Segurança
- **JWT Bearer Authentication** com chave simétrica (HMAC-SHA256)
- **Password Hashing** seguro via IPasswordHasherRepository
- **Validação de Senha Forte**: letras + números + caracteres especiais + mínimo 8 caracteres
- **Security Headers**: HSTS, CSP, X-Frame-Options (SecurityHeadersMiddleware)
- **CORS** configurável

### 🔹 Observabilidade
- **Logging multi-destino:** Database, Elasticsearch, New Relic (via Serilog)
- **Correlation IDs:** Rastreamento distribuído (CorrelationContext)
- **Elastic APM:** Monitoramento de performance e tracing
- **Request Logging:** Middleware de logging de requisições (RequestLoggingMiddleware)
- **Structured Logs:** Serilog com enrichers (Environment, Thread)

### 🔹 Health Checks Dinâmicos
- **Auto-discovery** via `IEnumerable<IHealthCheck>`
- **Checks:** Database (SQL Server), Elasticsearch, System
- **Extensível:** Adicione novo check sem modificar código existente

---

## 🎯 Princípios SOLID

### Resumo

| Princípio | Aplicação |
|-----------|-----------|
| **S** - Single Responsibility | Cada classe tem 1 responsabilidade (UserService orquestra, AuthService gera tokens, Repository persiste) |
| **O** - Open/Closed | Extensível sem modificar (novo Health Check → registra no DI; novo Logger → adiciona implementação) |
| **L** - Liskov Substitution | ILoggerService → Database/Elastic/NewRelic substituíveis; IUserRepository → qualquer implementação |
| **I** - Interface Segregation | Interfaces coesas (IUserRepository, IPasswordHasherRepository, IAuthService, IUserService, IHealthCheck) |
| **D** - Dependency Inversion | Depende de abstrações, não implementações (Repository no Domain, implementação na Infrastructure) |

### Exemplos Práticos

**Adicionar novo Health Check:**
```csharp
// 1. Implementar interface
public class RedisHealthCheck : IHealthCheck
{
    public string ComponentName => "Redis";
    public bool IsCritical => false;
    public Task<ComponentHealth> CheckHealthAsync() { ... }
}

// 2. Registrar no DI
builder.Services.AddScoped<IHealthCheck, RedisHealthCheck>();

// ✅ HealthCheckService descobre automaticamente!
```

**Trocar Logger:**
```csharp
// Apenas alterar configuração
"LoggerSettings": { "Provider": "Elastic" }  // ou "Database", "NewRelic"

// Código cliente não muda! (Dependency Inversion)
```

📚 **Documentação completa:** `docs/SOLID_Summary.md`

---

## 🛠️ Tecnologias

**Core:** .NET 8, C# 12, ASP.NET Core 8
**Persistência:** SQL Server (Entity Framework Core 8, Code-First, Migrations)
**Auth:** JWT Bearer Authentication (HMAC-SHA256)
**Logging:** Serilog (Elasticsearch, SQL Server, New Relic)
**APM:** Elastic APM
**Testes:** xUnit, Moq, FluentAssertions, NetArchTest, k6
**Infra:** Docker (Alpine), Kubernetes (AKS), GitHub Actions
**Documentação:** Swagger/OpenAPI 3.0

---

## 🚀 CI/CD

### Pipeline Automatizado (GitHub Actions → AKS)

A aplicação possui pipeline CI/CD via GitHub Actions (`.github/workflows/ci-cd-aks.yml`) com:
- Build e restauração de dependências
- Execução de testes (Architecture, Unit, Integration) + Code Coverage
- Build e push de imagem Docker no Azure Container Registry (ACR)
- Deploy no Azure Kubernetes Service (AKS)

---

## 🚀 Setup Rápido

```bash
# 1. Clonar
git clone https://github.com/fkwesley/Common.Users.git
cd Common.Users

# 2. Restaurar dependências
dotnet restore

# 3. Configurar conexões (appsettings.Development.json)
"ConnectionStrings": {
  "UsersDbConnection": "Server=localhost\\SQLEXPRESS;Database=UsersDb;Trusted_Connection=True;TrustServerCertificate=True"
}
"Jwt": {
  "Key": "<sua-chave-secreta>",
  "Issuer": "<seu-issuer>"
}

# 4. Aplicar migrations
cd Infrastructure
dotnet ef database update --startup-project ../API

# 5. Executar
cd ../API
dotnet run

# ✅ API disponível em: https://localhost:5001/swagger
```

---

## 🧪 Testes

### Pirâmide de Testes
```
      /\
     /E2E\        ← 5% (críticos)
    /------\
   / Integr \     ← 20% (Repositories, API)
  /----------\
 /Unit Tests  \   ← 70% (Entities, Services, Mappings, Helpers)
/______________\
  + Architecture  ← 5% (Layers, SOLID, Naming, Security, API Design, Performance, Testability)
```

### Executar
```bash
dotnet test                                                          # Todos
dotnet test --filter "FullyQualifiedName~UnitTests"                 # Unitários
dotnet test --filter "FullyQualifiedName~IntegrationTests"          # Integração
dotnet test --filter "FullyQualifiedName~ArchitectureTests"         # Arquitetura
k6 run Tests/LoadTests/load-test.js                                  # Carga
```

### Arquitetura (NetArchTest)
```csharp
// Valida Clean Architecture
Types.InAssembly(domainAssembly)
    .ShouldNot().HaveDependencyOn("Infrastructure")
    .GetResult().IsSuccessful.Should().BeTrue();
```

---

## 👨‍💻 Autor

**Frank Vieira** - [GitHub](https://github.com/fkwesley)
