# Catálogo de Filmes — Tom Hanks

> ISW055 · Atividade Prática 02 · @siriani

Aplicação web que busca filmes com **Tom Hanks** na API do TMDB e permite que cada usuário favorite e comente — com isolamento completo de dados entre usuários.

---

## ✨ Funcionalidades

- **Autenticação** — cadastro e login com senha criptografada (bcrypt)
- **Catálogo ao vivo** — filmes buscados em tempo real da API TMDB
- **Favoritos** — salve seus filmes preferidos por conta
- **Comentários** — escreva comentários por filme, por conta
- **Isolamento total** — nenhum dado de um usuário é visível para outro

---

## 🛠️ Tecnologias

| Camada | Tecnologia |
|--------|-----------|
| Backend | Node.js + Express |
| Templates | EJS (server-side rendering) |
| Banco de dados | MariaDB via mysql2 |
| Autenticação | express-session + bcryptjs |
| API externa | TMDB (The Movie Database) |
| Containerização | Docker + docker-compose |

---

## 🚀 Como Executar Localmente

### Pré-requisitos

- Node.js 20+
- Acesso ao banco MariaDB da disciplina

### 1. Clone e instale as dependências

```bash
git clone https://github.com/Gabriel-Assis-22/Atividade_02_Claude.git
cd Atividade_02_Claude
npm install
```

### 2. Configure as variáveis de ambiente

```bash
cp .env.example .env
# Edite o .env com suas credenciais reais (nunca commite este arquivo)
```

### 3. Crie as tabelas no banco (uma única vez)

Execute o arquivo `init.sql` no banco `IAC_2026_02_gabriel_assis` via DBeaver ou linha de comando.

### 4. Inicie o servidor

```bash
npm start
# Acesse http://localhost:3000
```

---

## 🐳 Deploy via Portainer

1. Acesse [portainer.lapps.studio](https://portainer.lapps.studio)
2. Vá em **Stacks → + Add stack → Repository**
3. Informe a URL deste repositório
4. Configure as variáveis de ambiente no campo **Env** do Portainer (nunca no repositório)
5. **Deploy the stack**

A aplicação ficará disponível em: `https://gabriel-assis-isw055.lapps.studio`

---

## 🔒 Segurança

- Chave da TMDB e credenciais do banco **nunca expostas** no código cliente
- Todas as chamadas à TMDB são feitas pelo backend (proxy)
- Senhas armazenadas com hash bcrypt (salt rounds = 10)
- Isolamento de dados: todas as queries usam `WHERE usuario_id = ?` com o ID da sessão server-side
- `.env` no `.gitignore` — apenas `.env.example` versionado

---

## 📋 Variáveis de Ambiente

Veja o arquivo [`.env.example`](.env.example) para a lista completa de variáveis necessárias.
