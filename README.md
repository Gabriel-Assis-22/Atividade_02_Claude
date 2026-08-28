# Catálogo de Filmes — Tom Hanks (Arquitetura de Microsserviços)

> ISW055 · Infraestrutura e Aplicações em Cloud · Professor [@siriani](https://github.com/siriani)  
> Aluno: Gabriel Assis

Aplicação web desenvolvida com arquitetura de microsserviços desacoplados, backend em **.NET 10 (C#) com Domain-Driven Design (DDD)**, frontend em **Angular 19 (SPA)** e banco **MariaDB**.

---

## 🏗️ Arquitetura de Microsserviços

O sistema foi modularizado em microsserviços independentes conectados por uma rede interna de bridge no Docker:

```
                                  REDE EXTERNA (HOST)
                                          │
                                          ▼ Porta 8208
                                ┌───────────────────┐
                                │     frontend      │
                                │ (Angular + Nginx) │
                                └─────────┬─────────┘
                                          │
                  REDE INTERNA DOCKER (app-network) — SEM PORTAS EXTERNAS
                  ───────────────────────────────────────────────────────
                                          │ /api/
                                          ▼
                                ┌───────────────────┐
                                │      backend      │
                                │ (Catálogo de      │
                                │  Filmes - .NET)   │
                                └─────────┬─────────┘
                                          │ HTTP Interno
                                          ▼ (http://auth-service:8081)
                                ┌───────────────────┐
                                │   auth-service    │ ──▶ Mailtrap (SMTP/API)
                                │  (Autenticação,   │
                                │   Roles & Reset)  │
                                └───────────────────┘
```

### 1. `frontend` (Porta pública: `8208`)
- Interface moderna em **Angular 19** com componentes standalone e lazy-loading.
- Servido via **Nginx** em container alpine.
- Configurado com proxy reverso interno para `/api/` direcionando ao backend.

### 2. `backend` (Catálogo — Porta interna: `8080`)
- ASP.NET Core 10 Web API estruturado em DDD (`Domain`, `Application`, `Infrastructure`, `Api`).
- Responsável pelo catálogo TMDB (proxy seguro), favoritos e comentários.
- **Delega 100% das operações de autenticação** ao `auth-service` via requisições HTTP internas (`http://auth-service:8081`).

### 3. `auth-service` (Autenticação — Porta interna: `8081` — **SEM PORTA NO HOST**)
- Microsserviço dedicado de identidade e autenticação em .NET 10 DDD.
- **Controle de Acesso Baseado em Papéis (*Roles*)**: Suporta papéis como `usuario` e `admin`. A role é injetada nos claims do JWT e exposta via endpoint `/internal/validate`.
- **Recuperação de Senha com Expiração de 30 Minutos**:
  - Geração de token único criptográfico de 32 bytes (UUID hex).
  - Tabela `reset_tokens` com `criado_em`, `expira_em` (30 minutos) e flag `usado`.
  - Validação rigorosa: rejeita tokens inexistentes, expirados ou reutilizados.
- **Envio Real de E-mails via Mailtrap**:
  - Disparo de e-mails transacionais com link de redefinição de senha para ambiente de desenvolvimento/inspeção segura.

---

## 🔒 Comprovação de Isolamento de Rede (Requisito 2)

O serviço `auth-service` **NÃO** expõe portas para a máquina host (`ports:` omitido), comunicando-se exclusivamente pela rede interna `app-network`:

```yaml
version: "3.8"

networks:
  app-network:
    driver: bridge

services:
  frontend:
    build: ./frontend
    ports:
      - "8208:80"        # ÚNICO ponto de entrada público
    depends_on:
      - backend
    networks:
      - app-network

  backend:
    build: ./backend
    expose:
      - "8080"           # Apenas rede interna
    depends_on:
      - auth-service
    env_file: .env
    networks:
      - app-network

  auth-service:
    build: ./auth-service
    expose:
      - "8081"           # ISOLADO: Sem mapeamento 'ports:' no host
    env_file: .env
    networks:
      - app-network
```

---

## 🔄 Fluxo de Recuperação de Senha ("Esqueci Minha Senha")

1. **Solicitação**: O usuário acessa `/auth/forgot-password` e informa o e-mail cadastrado.
2. **Geração**: O `auth-service` gera um token único associado ao usuário com validade estrita de **30 minutos**.
3. **Disparo**: O serviço envia um e-mail com layout escuro responsivo via API do **Mailtrap**, contendo o link de redefinição:  
   `https://gabriel-assis-isw055.lapps.studio/auth/reset-password?token=<TOKEN_UNICO>`
4. **Inspeção no Mailtrap**: O e-mail é capturado na caixa de entrada virtual do Mailtrap para verificação.
5. **Redefinição**: Ao clicar no link, o usuário é direcionado para a tela de redefinição, onde informa a nova senha.
6. **Validação e Invalidação**: O `auth-service` valida o token (existência, tempo de expiração e se já foi utilizado), atualiza o hash BCrypt do usuário e marca o token como `usado = true` para impedir reutilização.

---

## 🚀 Como Executar Localmente

### Pré-requisitos
- .NET 10 SDK
- Node.js 20+ & Angular CLI 19
- Docker e Docker Compose

### Execução via Docker Compose:
```bash
docker compose up --build
```
Acesse a aplicação no navegador em `http://localhost:8208`.

---

## 🐳 Deploy no Portainer

1. Acesse [portainer.lapps.studio](https://portainer.lapps.studio)
2. Atualize a Stack apontando para o repositório no branch `main`.
3. Configure as variáveis no campo **Env**:
   - `TMDB_API_KEY`
   - `MAILTRAP_API_TOKEN`
   - `DB_HOST`, `DB_PORT`, `DB_USER`, `DB_PASSWORD`, `DB_NAME`
   - `JWT_SECRET`
   - `AUTH_SERVICE_URL=http://auth-service:8081`
   - `FRONTEND_URL=https://gabriel-assis-isw055.lapps.studio`
4. Clique em **Update the stack**.
