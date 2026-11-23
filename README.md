# Sistema Exportador de Planilhas para Precificação

**Consulta de vendas no Oracle Winthor e geração de planilhas para o processo de precificação.**

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor&logoColor=white)
![Oracle](https://img.shields.io/badge/Oracle-Database-F80000?logo=oracle&logoColor=white)
![Testes](https://img.shields.io/badge/Testes-NUnit-2563EB)
![Excel](https://img.shields.io/badge/Exporta%C3%A7%C3%A3o-XLS-217346)

Aplicação web desenvolvida como **Projeto Aplicado** da pós-graduação em **Arquitetura de Software e Inteligência Artificial**. O projeto evolui o Sistema Continental para permitir a consulta de vendas no ERP Oracle da matriz e a geração de planilhas Excel utilizadas no processo operacional de precificação.

O problema, os requisitos de negócio e as metas aqui descritas estão alinhados ao [relatório do Projeto Aplicado](Relat%C3%B3rio%20do%20Projeto%20Aplicado%20-%20Francisco%20Gleidson%20da%20Silva%20Freitas.pdf), incluído na raiz do repositório.

## Navegação

- [Contexto e objetivo](#contexto-e-objetivo)
- [Funcionalidades](#funcionalidades-implementadas)
- [Arquitetura e fluxo](#arquitetura)
- [Tecnologias e pré-requisitos](#tecnologias)
- [Configuração local](#configuração-local)
- [Compilar e executar](#compilar-e-executar)
- [Testes](#testes)
- [Medição de desempenho](#medição-de-desempenho-da-extração)
- [Segurança antes de implantar](#segurança-antes-de-implantar)
- [Documento do Projeto Aplicado](#documento-do-projeto-aplicado)

---

## Contexto e objetivo

O relatório do Projeto Aplicado descreve um fluxo no qual as vendas migraram para o ERP central, enquanto a extração e a preparação de planilhas dependiam de intervenção técnica. Essa dependência podia atrasar a atualização de preços e estoque das lojas parceiras e o processamento das informações pelo Financeiro.

A aplicação oferece um fluxo **self-service**: o usuário informa os números das vendas, consulta os dados no Oracle, revisa os itens encontrados e baixa uma planilha compatível com o processo existente. O objetivo é reduzir o trabalho manual e encurtar o tempo de disponibilização dos dados.

> [!IMPORTANT]
> As metas e evidências de desempenho e homologação dependem de validação no ambiente operacional; não são garantidas apenas pela execução local da aplicação.

## Funcionalidades implementadas

| Recurso | Descrição |
| --- | --- |
| Acesso autenticado | Tela de extração disponível em `/extracao-vendas`. |
| Seleção de destino | Loja Winthor, atualmente apresentada como opção fixa. |
| Identificação do arquivo | Data e complemento informados pelo usuário. |
| Consulta de vendas | Entrada de até 1.000 números distintos, separados por vírgula, ponto e vírgula, espaço ou quebra de linha. |
| Validação | Identificação de vendas inexistentes ou canceladas antes da extração completa. |
| Pré-visualização | Exibição dos itens encontrados e dos totais consolidados. |
| Exportação | Geração e download de `.xls` em memória, sem arquivo temporário no servidor. |

> [!NOTE]
> A data informada compõe o nome do arquivo; a consulta é filtrada pelos números de venda.

A planilha contém as colunas `PRO_CODIGO`, `COD_R3MAGAZINE`, `PRO_DESCRICAO`, `PRO_REFERENCIA`, `QUANTIDADE`, `VENDA`, `CUSTO`, `PRECO_LOJA` e `CUSTO_TOTAL`.

## Arquitetura

A solução é implementada em **Blazor Server com renderização interativa no servidor** e organiza responsabilidades em projetos separados, seguindo uma arquitetura em camadas inspirada nos princípios de DDD. A separação facilita a evolução e os testes das regras sem acoplar a interface diretamente ao acesso a dados.

| Projeto | Responsabilidade |
| --- | --- |
| `ContinentalSistemas.Domain` | Núcleo do domínio. O projeto está preparado para concentrar entidades, objetos de valor e regras de negócio independentes de infraestrutura. |
| `ContinentalSistemas.Application` | Casos de uso e contratos da aplicação. `ExtracaoVendasService` coordena a verificação de vendas, delega a consulta ao repositório e gera a planilha. |
| `ContinentalSistemas.Data` | Persistência e integração externa. `WinthorVendasRepository` acessa o Oracle usando `Oracle.ManagedDataAccess.Core` e comandos SQL parametrizados. O projeto também contém o contexto e as migrações do Identity. |
| `ContinentalSistemas.Ioc` | Registro das dependências e composição dos serviços e repositórios. |
| `ContinentalSistemas.App` | Aplicação web Blazor, autenticação, interface Bootstrap e integração do download com o navegador. |

O domínio está atualmente enxuto: a maior parte do comportamento específico da extração está no serviço de aplicação, e a comunicação Oracle fica na camada de dados. Portanto, o projeto adota a separação de camadas e conceitos de DDD como direção arquitetural, sem afirmar que há um modelo de domínio rico já implementado.

### Fluxo da extração

1. O usuário autenticado informa os parâmetros e os números das vendas na página Blazor.
2. A camada de aplicação valida os números existentes chamando `IWinthorVendasRepository`.
3. O repositório consulta o Oracle com parâmetros vinculados, evitando concatenar os valores informados pelo usuário ao SQL.
4. Se todos os números forem válidos, os itens são recuperados e apresentados na pré-visualização.
5. O serviço monta a planilha `.xls` com NPOI; a aplicação entrega o conteúdo ao navegador para download.

## Tecnologias

| Área | Tecnologia |
| --- | --- |
| Plataforma | C# e .NET 10 |
| Interface | ASP.NET Core Blazor Server / Razor Components e Bootstrap |
| Integração | Oracle Database e `Oracle.ManagedDataAccess.Core` |
| Planilhas | NPOI para geração de arquivos `.xls` |
| Identidade e persistência | ASP.NET Core Identity e Entity Framework Core |
| Testes | NUnit |

> [!NOTE]
> A solução não executa modelos de IA em tempo de execução. O contexto de Inteligência Artificial faz parte da pós-graduação e do processo de desenvolvimento assistido por ferramentas; a funcionalidade entregue neste repositório é a integração de dados e a geração de planilhas.

## Pré-requisitos

- SDK do .NET 10
- Visual Studio 2026 ou uma CLI compatível com .NET 10
- Acesso de rede ao Oracle Winthor para executar consultas reais
- SQL Server LocalDB para a configuração de desenvolvimento da autenticação, ou uma conexão SQL Server configurada no ambiente

## Configuração local

### 1. Configure o Oracle

A conexão Oracle e suas credenciais são lidas da configuração `WinthorOracle`. Configure os valores localmente com User Secrets, sem gravar segredos em arquivos rastreados pelo Git:

```powershell
dotnet user-secrets set "WinthorOracle:Host" "<host-oracle>" --project src\ContinentalSistemas.App\ContinentalSistemas.App.csproj
dotnet user-secrets set "WinthorOracle:UserId" "<usuario>" --project src\ContinentalSistemas.App\ContinentalSistemas.App.csproj
dotnet user-secrets set "WinthorOracle:Password" "<senha>" --project src\ContinentalSistemas.App\ContinentalSistemas.App.csproj
dotnet user-secrets set "WinthorOracle:SID" "<sid>" --project src\ContinentalSistemas.App\ContinentalSistemas.App.csproj
```

Substitua os valores entre `<...>` pelos dados fornecidos pelo administrador do ambiente. Em outros ambientes, use variáveis de ambiente com os nomes `WinthorOracle__Host`, `WinthorOracle__UserId`, `WinthorOracle__Password` e `WinthorOracle__SID`.

> [!WARNING]
> `appsettings.json` contém valores vazios para esses parâmetros. Não coloque credenciais reais nesse arquivo nem em arquivos rastreados pelo Git.

### 2. Configure a autenticação

A conexão `DefaultConnection` é usada pelo Identity e está configurada por padrão para SQL Server LocalDB em desenvolvimento. Para outros ambientes, forneça a conexão por configuração protegida, por exemplo com a variável de ambiente `ConnectionStrings__DefaultConnection`.

## Compilar e executar

Na raiz do repositório:

```powershell
dotnet restore ContinentalSistemas.slnx
dotnet build ContinentalSistemas.slnx
dotnet run --project src\ContinentalSistemas.App
```

Abra a URL local apresentada pela aplicação e acesse `/extracao-vendas` após autenticar. A consulta Oracle só funcionará quando rede, host, usuário, senha e SID estiverem configurados corretamente.

> [!IMPORTANT]
> Na inicialização, a aplicação aplica as migrações de identidade. O banco configurado precisa estar acessível e permitir as operações necessárias.

## Testes

Os testes unitários usam NUnit e podem ser executados separadamente:

```powershell
dotnet test tests\ContinentalSistemas.Application.tests\ContinentalSistemas.Application.tests.csproj
dotnet test tests\ContinentalSistemas.Data.tests\ContinentalSistemas.Data.tests.csproj
```

Os testes do serviço cobrem a determinação de vendas inexistentes e a estrutura e os valores do arquivo Excel. Os testes da camada de dados cobrem entradas vazias, validação da configuração Oracle e estrutura das consultas SQL.

> [!NOTE]
> Os testes unitários não substituem um teste de integração com uma instância Oracle acessível.

## Medição de desempenho da extração

Em homologação, configure o Oracle e a VPN como no ambiente operacional. Na tela `/extracao-vendas`, informe até 1.000 números de vendas distintos e execute a consulta e a exportação. Uma venda pode gerar vários itens; o resultado é agrupado por produto e preço, e sua contagem pode ser diferente da quantidade original de itens de venda.

### Métricas disponíveis

Os logs no nível `Information` incluem:

| Registro | O que é medido |
| --- | --- |
| `Desempenho Oracle` | Abertura de cada conexão e execução **mais leitura completa** de cada consulta, em milissegundos. `sucesso=False` identifica uma etapa interrompida por erro. A abertura pode reutilizar uma conexão do pool. |
| `Desempenho consulta` | Duração da validação e da extração, quantidades de vendas, itens e inexistentes, e resultado. Inclui as duas conexões e consultas, mas não a renderização da tabela ou a exportação. |
| `Desempenho exportacao` | Duração da geração do XLS, transferência via JavaScript, duração total, tamanho e resultado. A conclusão da chamada JavaScript não comprova que o arquivo foi salvo em disco; confira o download e o conteúdo. |
| `Memoria exportacao` | Amostras de memória privada, working set e memória gerenciada, em bytes, antes da exportação, após gerar o XLS e no fim. São valores do **processo inteiro**, não de um usuário, e não necessariamente representam o pico. Monitore o processo também externamente. |
| Eventos de circuito | Abertura, desconexão, reconexão e encerramento, com duração de vida e, quando aplicável, duração observada da desconexão até reconexão ou encerramento. Não medem diretamente o tempo de download. |

Em compilações **Debug**, ao terminar a consulta, a página também apresenta um painel com o resultado, os horários de início e fim, o tempo decorrido, o tempo de CPU e a CPU média normalizada pelos processadores lógicos, além da memória privada e residente no início e no fim. O painel também aparece se a consulta for interrompida por erro ou se houver vendas inexistentes. CPU e memória abrangem o processo servidor inteiro, inclusive outras atividades e usuários, e as amostras de memória não são necessariamente picos. A duração não inclui a renderização nem a geração ou o download da planilha. Em compilações Release o painel não é exibido; selecionar o ambiente Development, por si só, não o habilita.

### Coleta dos registros

Os registros da página incluem `extracaoId` e `circuitoId` aleatórios. Para correlacionar os registros Oracle com a extração no console, habilite a exibição de escopos do provedor de console antes de iniciar a aplicação, por exemplo no PowerShell:

```powershell
$env:Logging__Console__FormatterOptions__IncludeScopes = 'true'
dotnet run --project src\ContinentalSistemas.App\ContinentalSistemas.App.csproj
```

Configure a coleta e retenção dos logs no ambiente de homologação, sem registrar credenciais ou números de vendas. Escopos e outros logs da aplicação podem conter informações sensíveis: proteja o acesso aos arquivos e revise seu conteúdo antes de compartilhá-los.

### Avaliação dos resultados

1. Compare cada duração `ConsultaELeituraItens` com a meta de **menos de 5.000 ms**; registre também a verificação de vendas e o tempo total.
2. Repita os cenários e documente os critérios de aprovação, resultados, erros, versão do ambiente e concorrência.
3. Para medir carga simultânea ou comprovar a VPN, execute testes com múltiplos navegadores ou uma ferramenta de carga em ambiente autorizado. A instrumentação, por si só, não executa esses testes.
4. Se a meta não for atendida, analise o plano real e os índices existentes com o DBA antes de qualquer ajuste.

## Segurança antes de implantar

- Não versione credenciais Oracle, connection strings de produção nem segredos de autenticação. Use User Secrets localmente e um cofre de segredos ou variáveis protegidas no ambiente implantado.
- Revise `src/ContinentalSistemas.App/Program.cs`: a configuração atual cria o usuário inicial `Master` com uma senha definida no código. Remova essa credencial fixa e implemente um provisionamento seguro antes de implantar; se a senha já foi usada, altere-a.
- Conceda ao usuário Oracle somente as permissões estritamente necessárias para as consultas de leitura.
- Use TLS, controles de acesso e políticas de retenção adequados ao ambiente e aos dados corporativos.

## Documento do Projeto Aplicado

O arquivo [Relatório do Projeto Aplicado - Francisco Gleidson da Silva Freitas.pdf](Relat%C3%B3rio%20do%20Projeto%20Aplicado%20-%20Francisco%20Gleidson%20da%20Silva%20Freitas.pdf) reúne o contexto do negócio, as personas, o levantamento de requisitos, o planejamento em sprints e as evidências descritas no relatório. Ele complementa este README; resultados de homologação, metas e desempenho devem ser interpretados conforme as evidências e o ambiente documentados nele.
