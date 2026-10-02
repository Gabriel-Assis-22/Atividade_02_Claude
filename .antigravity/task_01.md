# ISW055 · P1 · Avaliação bimestral · individual

## P1 — Relatório bimestral de atividades

> A primeira avaliação não é uma prova: é um relatório, em PDF, do que você entregou neste bimestre — com introdução, metodologia, um quadro com a data planejada e a data realizada de cada atividade, e a evidência de cada entrega. O que vale é o que você consegue provar.

* **Entrega:** quarta-feira, 07/10/2026
* **Formato:** avaliação individual · PDF único · mesma data pra todas as turmas

Ao longo do bimestre você construiu, semana a semana, o catálogo de filmes que virou um sistema de vários serviços. As entregas estão espalhadas — em commits, artigos, mensagens. A P1 junta tudo num documento só, do jeito que um profissional presta contas de um período de trabalho: o que era pra ser feito, quando, o que foi feito, quando, e a prova.

> Entrega individual, sem exceção. Cada aluno escreve o próprio relatório, sobre as próprias entregas. Plano Premium (09/10) e SaaS/PaaS/IaaS (16/10) ficam fora desta P1 — entram na próxima avaliação.

---

## Conceito

### Por que um relatório, se as entregas já estão públicas

Porque saber fazer é metade; saber documentar e comprovar o que fez é a outra metade — e é a que decide entrevistas, auditorias e promoções. Um relatório de atividades tem três propriedades que uma pilha de links não tem: **rastreabilidade** (cada item aponta pra uma evidência com data e hora), **contexto** (por que aquilo foi feito, com que método, com que dificuldade) e **honestidade estruturada** (o que não foi entregue aparece na tabela do mesmo jeito que o que foi — o relatório é um registro, não uma propaganda).

A coluna **data planejada × data realizada** é o coração do documento. Ela mostra, de uma vez, o seu ritmo no bimestre — e é exatamente o tipo de tabela que um gestor de projeto olha antes de qualquer outra coisa.

---

## Estrutura

### O que o PDF precisa conter, nesta ordem

`P1_CODIGO_Nome_Sobrenome.pdf`

* **Capa · Sumário**
* **1 Introdução**
* **2 Metodologia**
* **3 Quadro de entregas**

  | nº | atividade / descrição | planejada | realizada |
  | :---: | :--- | :---: | :---: |
  | 1 | … | 20/08 | 19/08 22:41 |

* **4 Atividades — uma ficha por atividade**
* **5 Considerações finais**
* **Declaração de autoria**

*(o quadro da seção 3 é gerado pelas fichas da seção 4)*

---

#### Ficha — Atividade n

* **Descrição:** o que a atividade pedia
* **Data planejada:** prazo publicado no site
* **Data realizada:** DD/MM/AAAA HH:MM — de onde veio a hora
* **Situação:** entregue · com atraso · não entregue
* **Evidência:** GitHub · LinkedIn · WhatsApp
* **Link:** URL direta, que abre
* **print da entrega:** (link + data e hora visíveis)
* **print do resultado:** (sistema / mapa / artigo)
* **o que foi feito · dificuldades e como foram resolvidas**
* *(atividade não entregue também ganha ficha — com o que faltou e por quê)*

O template já traz esse esqueleto pronto: as fichas da seção 4 alimentam automaticamente o quadro da seção 3, então você preenche cada atividade uma vez só.

---

### Detalhamento das Seções

* **Capa e sumário:** Fatec, disciplina, turma, seu nome, semestre. O sumário é gerado pelo template.
* **1 Introdução:** O que é a disciplina, qual foi o projeto do bimestre, o que o relatório contém — escrito pra quem não assistiu às aulas.
* **2 Metodologia:** Como as atividades foram feitas (ferramentas, ambiente, fontes) e de onde saíram as datas e horas das evidências.
* **3 Quadro de entregas:** Tabela: nº · atividade · descrição · data planejada · data realizada · situação. Gerada a partir das fichas.
* **4 Atividades realizadas:** Uma ficha por atividade: descrição, datas, situação, evidência, link, o que foi feito, prints, dificuldades.
* **5 Considerações finais:** O que aprendeu, o que foi mais difícil, o que faria diferente. Sem repetir a introdução.
* **Declaração de autoria:** Que o relatório é seu, individual, e que as evidências são verificáveis nos links.

---

## Escopo

### Atividades que entram nesta P1

Todas as atividades da disciplina com prazo até 07/10 — as datas planejadas abaixo são as publicadas na lista de atividades. A data realizada é você quem informa, com prova.

| Nº | Atividade | Data planejada | Evidência esperada |
| :---: | :--- | :---: | :--- |
| **1** | Agenda telefônica em Flask | 07/08/2026 | realizada em sala — print do sistema rodando |
| **2** | Catálogo de filmes — Tom Hanks | 20/08/2026 | GitHub — commit, README, print do catálogo |
| **3** | Desacoplando o login — microsserviço de autenticação | 28/08/2026 | GitHub — commit, docker-compose.yml, print do login |
| **4** | Controle de acesso por papel — RBAC | 04/09/2026 | GitHub — commit, print do 403 e da ação de admin |
| **5** | Logs e auditoria | 25/09/2026 | GitHub — commit, print da consulta de logs pelo admin |
| **6** | Upload e perfil de usuário | 02/10/2026 | GitHub — commit, print do perfil com foto |

> As três atividades extras (Swagger, CI/CD, observabilidade) não são obrigatórias — mas, se você fez alguma, documente na seção própria do template: conta a favor.

---

## Evidência

### Como provar a data e a hora de cada entrega

Evidência é link que abre + print com data e hora visíveis. Um sem o outro não vale.

#### GitHub
* **Onde está a data e a hora:**  
  Abra a página do commit da entrega no GitHub (aba Commits → clique no commit): ela mostra data e hora exatas.
* **No terminal:**  
  ```bash
  git log -1 --date=format:"%d/%m/%Y %H:%M" --format="%h %ad"
  ```
* O print precisa mostrar o hash, a data/hora e o nome do repositório.

#### Regra geral
* **O print precisa ser legível:**  
  Tela inteira, sem zoom que corte a data. Se o print não deixa claro de qual entrega se trata, ele não é evidência — é uma imagem.

---

## Template

### Baixe o modelo — já vem com as suas atividades

O template está em Typst, uma linguagem de composição de documentos parecida com Markdown e muito mais simples que LaTeX. Ele já traz a capa, o sumário, as fichas de cada atividade desta disciplina com as datas planejadas preenchidas, e o quadro de entregas que se monta sozinho.

* **⬇ P1_ISW055_template.typ** — fonte editável
* **⬇ P1_ISW055_template.pdf** — como fica compilado

---

### Formas de Edição e Compilação

1. **Compilar no navegador — sem instalar nada**  
   Entre em `typst.app` (conta gratuita), crie um projeto vazio, envie o `.typ` e uma pasta `prints/` com as suas imagens. O PDF aparece ao lado enquanto você edita; baixe pelo botão de exportar.

2. **Ou no terminal**  
   Instale o Typst (`brew install typst`, `winget install Typst.Typst` ou o binário em `github.com/typst/typst`) e rode `typst compile P1_ISW055_template.typ` na pasta onde estão o arquivo e os prints.

3. **Prefere Word ou Google Docs?**  
   Pode — desde que o PDF final tenha exatamente a mesma estrutura do template (seções, quadro de entregas com as mesmas colunas, ficha por atividade, declaração). Use o PDF do template como referência visual.

---

## Requisitos

### O que será conferido

1. **Quadro de entregas completo**  
   Todas as atividades do escopo na tabela, com data planejada, data realizada (dia e hora) e situação — inclusive as não entregues, marcadas como tal.

2. **Uma ficha por atividade, com evidência que abre**  
   Link direto (não a home do seu perfil), print da entrega com data/hora visíveis e print do resultado. Links que não abrem ou prints sem data invalidam a evidência daquela atividade.

3. **Introdução e metodologia de verdade**  
   Texto seu, no mínimo dois parágrafos cada, sem copiar o enunciado das atividades nem esta página. A metodologia diz de onde saíram as datas e horas.

4. **Organização e legibilidade**  
   Sumário funcionando, seções na ordem, prints legíveis com legenda, sem blocos de orientação do template esquecidos no meio do texto.

5. **Honestidade**  
   Atividade não entregue ou entregue com atraso aparece assim no quadro, com uma linha sobre o motivo. Uma ficha honesta de atividade não feita vale mais que uma evidência forjada — e evidência forjada zera a P1.

---

## Entrega

### Como entregar

* **PDF único, nomeado:** `P1_ISW055_Nome_Sobrenome.pdf`
* **Commitado no mesmo repositório das atividades, na pasta `docs/`:**  
  — a data e hora do commit é a data da entrega
* **Link do PDF adicionado ao README**, que continua mencionando `github.com/siriani`
* **Entrega individual** — um relatório por aluno, sobre o seu próprio repositório
* **Prazo:** quarta-feira, 07/10/2026  
  — comece agora: as primeiras atividades já podem ser documentadas hoje, e o quadro de entregas se completa sozinho a cada ficha preenchida