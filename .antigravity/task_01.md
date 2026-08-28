# Desacoplando o Login — Microsserviço de Autenticação

O catálogo de filmes fazia login, cadastro e controle de acesso no mesmo backend que serve o catálogo. Hoje isso vira um serviço à parte — e ganha uma funcionalidade nova: **recuperação de senha por e-mail, com link que expira em 30 minutos**.

Antes, tudo rodava em um único container: catálogo, login, cadastro e controle de permissões (quem pode favoritar/comentar). Apesar de funcional, esse modelo apresenta um problema de design arquitetural — qualquer alteração na lógica de autenticação exige modificar o mesmo código-fonte e realizar o deploy de todo o catálogo.

O objetivo é trabalhar com **serviços desacoplados**: extrair toda a responsabilidade de autenticação (login, papéis de usuário, recuperação de senha) para um container dedicado e isolado, comunicando-se com o catálogo estritamente via rede interna do Docker.

---

## 📋 Requisitos do Microsserviço

### 1. Novo Container Dedicado
- Criar um segundo serviço no `docker-compose.yml`.
- A stack tecnológica pode ser a mesma do catálogo ou diferente.
- O serviço centraliza: **login**, **cadastro**, **gestão de roles** e **recuperação de senha**.
- O backend do catálogo deixa de conter essas lógicas e passa a delegar as requisições ao serviço de autenticação.

### 2. Comunicação Exclusiva via Rede Interna Docker
- O serviço de autenticação **não** deve mapear portas para o host (`ports:` expostas externamente).
- O acesso deve ocorrer apenas pela rede Docker interna através do service name (ex.: `http://auth-service:3000/login`).
- Apenas o catálogo permanece como ponto de entrada público.

### 3. Controle de Acesso Baseado em Papéis (*Roles*)
- Implementar pelo menos dois níveis de acesso (ex.: `usuario` e `admin`).
- O serviço de autenticação deve ser capaz de informar o papel do usuário quando consultado pelo catálogo.

### 4. Recuperação de Senha com Expiração
- Geração de token único com validade de **30 minutos**.
- Estrutura sugerida para a tabela `reset_tokens`:

| Campo | Tipo / Descrição |
| :--- | :--- |
| `token` | `VARCHAR/UUID` (Único e aleatório, ex.: UUID ou hash 32+ bytes) |
| `usuario_id` | Identificador do usuário associado |
| `criado_em` | `TIMESTAMP` de geração |
| `expira_em` | `TIMESTAMP` (`criado_em` + 30 minutos) |
| `usado` | `BOOLEAN` (Evita reutilização do token) |

### 5. Envio Real de E-mail
- O link de redefinição deve ser disparado via SMTP real/sandbox:
  - **Desenvolvimento:** [Mailtrap](https://mailtrap.io/) (sandbox para captura e inspeção segura).
  - **Produção:** [Brevo](https://www.brevo.com/) (envio transacional para caixa de entrada real).
- *Para a atividade, o uso do Mailtrap em ambiente de desenvolvimento é suficiente.*

### 6. Validação do Token de Redefinição
Ao receber a nova senha, o serviço deve validar rigorosamente:
1. O token existe?
2. O token ainda não expirou (`now() <= expira_em`)?
3. O token ainda não foi utilizado (`usado == false`)?

Caso qualquer validação falhe, a operação deve ser rejeitada.

---

## 🚀 Entregáveis

- [ ] Manter o mesmo repositório público do GitHub da atividade anterior.
- [ ] Atualizar o `README.md` documentando as mudanças arquiteturais.
- [ ] Mencionar o professor no README: [@siriani](https://github.com/siriani).
- [ ] Apresentar o `docker-compose.yml` configurado com os dois serviços e a rede compartilhada.
- [ ] Comprovar que o microsserviço de autenticação não possui portas expostas ao host.
- [ ] Demonstrar o fluxo completo de "Esqueci minha senha":
  - Solicitação de redefinição
  - E-mail capturado no Mailtrap (print)
  - Uso do link e redefinição com sucesso


  ## Conteudo email trap:
  using Mailtrap;
    using Mailtrap.Emails.Requests;
    using Mailtrap.Emails.Responses;

    try
    {
        var apiToken = "<MAILTRAP_API_TOKEN>";
        using var mailtrapClientFactory = new MailtrapClientFactory(apiToken);
        IMailtrapClient mailtrapClient = mailtrapClientFactory.CreateClient();
        SendEmailRequest request = SendEmailRequest
            .Create()
            .From("hello@demomailtrap.co", "Mailtrap Test")
            .To("gabriel143486@gmail.com")
            .Subject("You are awesome!")
            .Category("Integration Test")
            .Text("Congrats for sending test email with Mailtrap!");
        SendEmailResponse? response = await mailtrapClient
            .Email()
            .Send(request);
    }
    catch (Exception ex)
    {
        Console.WriteLine("An error occurred while sending email: {0}", ex);
    }