# Guia de Acessos e Deploy — Infraestrutura da Disciplina

---

## 🗄️ MySQL — Banco de Dados por Aluno

### Credenciais

| Aluno | Usuário / Banco | Senha |
| :--- | :--- | :--- |
| GABRIEL ROBERTO RODELLA DE ASSIS | `IAC_2026_02_gabriel_assis` | `MbhQ01ueA99jdx` |

### Como Conectar

* **Host:** `35.226.64.52`
* **Porta:** `3306`
* **Regra:** O usuário e o nome do banco são idênticos (`IAC_2026_02_gabriel_assis`).
* **Clients compatíveis:** DBeaver, MySQL Workbench, linha de comando, etc.

### Trocar a Senha

Depois de logar pela primeira vez, execute:

```sql
ALTER USER CURRENT_USER() IDENTIFIED BY 'sua_nova_senha';
```

> **Nota:** A conexão é TCP sem TLS — não reutilize essa senha em outro serviço.

---

## 🐳 Acessos ao Portainer — Deploy do Container

### Tabela de Acessos

| Aluno | Usuário | Senha | Porta | Subdomínio (Prefixo) |
| :--- | :--- | :--- | :--- | :--- |
| GABRIEL ROBERTO RODELLA DE ASSIS | `IAC_2026_02_gabriel_assis` | `kUimYMaDeueezY` | `8208` | `gabriel-assis` |

### Como Usar

1. Acesse [https://portainer.lapps.studio](https://portainer.lapps.studio) e faça login com seu usuário e senha.
2. Acesse **Containers → + Add container** (ou **Stacks → + Add stack** para compor serviços).
3. No campo de portas (**Port mapping / Publish a new network port**), mapeie a porta do host (`8208`) para a porta interna que a aplicação escuta no container.

### ⚠️ Sua Porta Define seu Endereço

Sua aplicação só responde em:

```
https://gabriel-assis-isw055.lapps.studio
```

> A aplicação só aparece publicamente se for publicada **exatamente na porta do host `8208`**. Nenhuma outra porta funcionará para o seu subdomínio. Cada aluno só enxerga os próprios containers no painel.

---

## 🚀 Como Colocar seu Site no Ar

### Opção A — Imagem Pronta (Docker Hub)

Fluxo padrão de mercado: você builda a imagem localmente e envia para o registry.

1. Crie uma conta gratuita em [hub.docker.com](https://hub.docker.com).
2. No seu computador, na pasta do projeto com o `Dockerfile`, execute:

```bash
docker build -t seu-usuario/nome-do-app .
```

3. Realize o login e envie a imagem:

```bash
docker login
docker push seu-usuario/nome-do-app
```

4. No Portainer, vá em **Containers → + Add container**.
   * Em **Image**, informe `seu-usuario/nome-do-app:latest`.
   * Em **Port mapping**:
     * Host: `8208`
     * Container: Porta interna da aplicação (ex: `80`, `3000`, `8080`).

---

### Opção B — Build Direto do GitHub

Mais simples para testar sem Docker Hub: o próprio servidor compila a imagem.

1. Mantenha um `Dockerfile` na raiz do seu repositório no GitHub.
2. No Portainer, vá em **Stacks → + Add stack → aba Repository**.
3. Insira a URL do repositório (ex: `https://github.com/seu-usuario/seu-projeto`).
4. Declare a stack no editor do Portainer apontando o build e o mapeamento de portas:

```yaml
version: '3.8'
services:
  app:
    build: .
    ports:
      - "8208:sua-porta-do-app"
```

5. Clique em **Deploy the stack** para o Portainer clonar e buildar a imagem automaticamente.