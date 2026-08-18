Aqui está o conteúdo organizado e formatado em Markdown (.md):

Markdown
# ISW055 · Atividade Prática
## Catálogo de Filmes — Tom Hanks

Construa uma aplicação que busca filmes com **Tom Hanks** em uma API externa e permite que cada usuário favorite e comente — garantindo o isolamento completo dos dados entre diferentes usuários.

---

### 📅 Entrega
* **Prazo:** Quinta-feira, **20/08/2026**
* *(Começa como exercício de fim de semana, com prazo final até quinta-feira)*

> **Objetivo:** Esta atividade fecha o ciclo do que a disciplina vem construindo: você já é "inquilino" desta infraestrutura — tem seu próprio banco, isolado dos colegas. Agora é a sua vez de implementar essa mesma ideia dentro da sua aplicação: usuários diferentes do seu catálogo de filmes não podem ver os favoritos nem os comentários uns dos outros.

---

## 🛠️ Requisitos da Aplicação (3 Camadas)

### 1. Consumo de API — TMDB
Busque os filmes com Tom Hanks na API do TMDB (gratuita, com chave de desenvolvedor). Pôster, título e sinopse vêm sempre ao vivo da API — sua aplicação **nunca guarda esses dados nem baixa a imagem**, apenas utiliza a URL fornecida pela TMDB.

* **Buscar ID do ator:**
  ```http
  GET /search/person?query=Tom+Hanks
(Obtém o person_id de Tom Hanks)

Listar filmes:

HTTP
GET /person/{person_id}/movie_credits
(Retorna a lista de filmes com o poster_path de cada um)

URL final do pôster:

HTML
[https://image.tmdb.org/t/p/w500](https://image.tmdb.org/t/p/w500){poster_path}
(Pronta para renderizar em tags <img>)

2. Persistência — MariaDB
Favoritos e comentários são gravados no seu banco individual da disciplina. O catálogo em si nunca é salvo — apenas o que o usuário decide registrar sobre um filme.

Esquema de Banco Sugerido:
SQL
CREATE TABLE usuarios (
  id INT AUTO_INCREMENT PRIMARY KEY,
  nome VARCHAR(100) NOT NULL,
  email VARCHAR(150) UNIQUE NOT NULL,
  senha_hash VARCHAR(255) NOT NULL,
  criado_em TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE favoritos (
  id INT AUTO_INCREMENT PRIMARY KEY,
  usuario_id INT NOT NULL,
  tmdb_movie_id INT NOT NULL,
  titulo VARCHAR(255) NOT NULL,
  poster_path VARCHAR(255),
  criado_em TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (usuario_id) REFERENCES usuarios(id),
  UNIQUE (usuario_id, tmdb_movie_id) -- Impede favoritar o mesmo filme duas vezes
);

CREATE TABLE comentarios (
  id INT AUTO_INCREMENT PRIMARY KEY,
  usuario_id INT NOT NULL,
  tmdb_movie_id INT NOT NULL,
  texto TEXT NOT NULL,
  criado_em TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);
3. Segregação de Usuário (Multitenancy / Isolamento)
A aplicação deve possuir sistema próprio de cadastro e login (independentes do acesso ao Portainer/MySQL).

Toda consulta a favoritos ou comentários precisa ser estritamente filtrada pelo usuário logado:

SQL
SELECT * FROM favoritos WHERE usuario_id = ?;
Regra crítica: Nunca retorne dados de outra conta, mesmo que o ID de um favorito alheio seja informado manualmente.

🏗️ Arquiteturas de Referência
Fluxo de Dados e Requisições
[ Usuário A ] ──┐
                ├──> [ Aplicação Backend ] (Sessão / Token identifica usuario_id)
[ Usuário B ] ──┘          │                       │
                           │                       │
                 (Buscar catálogo)       (Favoritos / Comentários)
                           │                       │
                           ▼                       ▼
                     [ API TMDB ]           [ MariaDB Individual ]
                  (Externa / Sem estado)   - usuario_id = A
                                           - usuario_id = B
                                           (Queries com WHERE usuario_id = ?)
Nota: Buscar um filme nunca grava nada — é sempre uma chamada direta à TMDB. Favoritar ou comentar grava no MariaDB, sempre vinculado ao usuario_id da sessão ativa.

Pipeline de Deploy (Repositório ao Subdomínio)
[ Repositório GitHub ] (Público, com @siriani no README e Dockerfile)
         │
         ▼ (Clona e builda)
[ Portainer do Aluno ]
         │
         ▼ (Deploy the Stack)
[ Container na Porta Reservada ] ───> Subdomínio Pessoal no Ar
O container só ficará acessível publicamente se estiver mapeado exatamente na porta reservada do aluno (conforme PDF de acessos).

🧪 Cenário de Testes (Critérios de Aceite)
Acesso inicial: Abrir o subdomínio e visualizar a tela de login/cadastro (e não o catálogo diretamente).

Cadastro & Catálogo: Criar uma conta, autenticar e visualizar a listagem de filmes do Tom Hanks com pôster, título e sinopse vindos da TMDB.

Persistência: Favoritar um filme (ex: Forrest Gump), escrever um comentário e recarregar a página — os dados devem permanecer visíveis.

Isolamento de dados: Fazer logout, criar uma segunda conta e garantir que nenhum favorito ou comentário da primeira conta esteja visível.

Auditoria de código: Repositório público com Dockerfile, menção a @siriani no README.md e nenhuma credencial exposta.

⚠️ Segurança: Credenciais Apenas no Servidor
Atenção: Como o repositório é público, qualquer chave ou senha commitada fica exposta publicamente.

Proibido no Frontend/Cliente: Nunca exponha chaves da TMDB ou senhas do MariaDB no código client-side (JS de navegador), no HTML ou no Dockerfile.

Backend como Proxy: Todas as chamadas para a TMDB e queries ao MariaDB devem partir exclusivamente do backend (servidor).

Variáveis de Ambiente:

Configure as credenciais no Portainer via campo Env ou arquivo .env referenciado no docker-compose.yml.

No repositório, versione apenas um arquivo .env.example com os nomes das variáveis (sem valores sensíveis).

Adicione o arquivo .env ao .gitignore.

📋 Checklist de Avaliação e Entrega
[ ] Autenticação: Cadastro e login funcionais.

[ ] Integração TMDB: Busca e renderização dinâmica dos filmes e pôsteres.

[ ] Persistência MariaDB: Gravação correta de favoritos e comentários.

[ ] Segregação: Isolamento total de dados entre usuários testado com 2 contas.

[ ] Infraestrutura: Container publicado na porta reservada com subdomínio ativo.

[ ] GitHub: Repositório público contendo Dockerfile e menção a @siriani no README.md.

[ ] Segurança: Zero credenciais ou segredos expostos no código ou frontend.