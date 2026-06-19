# Projeto Contas Bancárias COBOL DB2

Projeto desenvolvido como continuação do [Projeto de transações bancárias em COBOL](https://github.com/caiofsb/Projeto-Transacoes-Bancarias-Cobol--Montreal), agora com integração ao banco de dados DB2.

O objetivo do sistema é processar arquivos de clientes e transações bancárias, aplicar regras de negócio, atualizar saldos no DB2, registrar erros e gerar relatórios de processamento.

## Tecnologias utilizadas

* COBOL
* GnuCOBOL
* DB2 Community
* C#
* ODBC

## Estrutura do projeto

```text
COBOL/
  PROJETO6.cbl          -> Programa principal em COBOL. Controla o fluxo geral do processamento.

COPYS/
  REGCLI6.cpy           -> Layout do arquivo de clientes.
  REGTRX6.cpy           -> Layout do arquivo de transações.
  REGOUT6.cpy           -> Layout dos arquivos de saída.
  WRKP6.cpy             -> Variáveis de trabalho do sistema.
  DBP6.cpy              -> Rotinas que montam as requisições para o DB2.
  ROTP6.cpy             -> Regras principais do processamento.

DB2HELPER/
  Program.cs            -> Helper em C# que executa comandos SQL no DB2 via ODBC.
  Db2Helper.csproj      -> Arquivo de configuração do projeto C#.

DB/
  DDL_P6.sql            -> Script para criar as tabelas e índices no DB2.
  QUERIES_P6.sql        -> Consultas SQL usadas como referência no projeto.

INPUT/
  CLIENTES.txt          -> Arquivo de entrada com os dados dos clientes.
  TRANSACOES.txt        -> Arquivo de entrada com as transações bancárias.

OUTPUT/
  CLIENTES_ORD.txt      -> Arquivo de clientes ordenado.
  TRANSACOES_ORD.txt    -> Arquivo de transações ordenado.
  RELATORIO.txt         -> Relatório geral do processamento.
  DETALHADO.txt         -> Relatório detalhado por cliente/operação.
  ERROS.txt             -> Arquivo com os erros encontrados.
  LOG.txt               -> Log do processamento.
  DB2_REQUEST.txt       -> Arquivo com as requisições que o COBOL envia para o helper.
  DB2_RESPONSE.txt      -> Arquivo com as respostas retornadas pelo helper.
  RELATORIO_DB2.txt     -> Relatório final gerado consultando diretamente o DB2.
JCL/
  JCLP6.jcl -> JCL de prática.

RUN_LOCAL.ps1           -> Script que ordena arquivos, compila e executa o projeto.

```

## Fluxo do sistema

```text
Arquivo de clientes
Arquivo de transações
        |
        v
Ordenação dos arquivos
        |
        v
Programa COBOL
        |
        v
Regras de negócio
        |
        v
DB2
        |
        v
Relatórios, logs e erros
```

## Em Funcionamento no VSCODE

<img width="1100" height="619" alt="2026-06-18 22-34-32 (1)" src="https://github.com/user-attachments/assets/176ee7a2-6540-49cd-9651-1661cdbd98f0" />

## O projeto utiliza as seguintes tabelas no DB2:
<img width="827" height="224" alt="image" src="https://github.com/user-attachments/assets/6fef2958-1d94-4d47-8fd6-2e21238fca2e" />

### CLIENTES                -> Guarda os dados dos clientes e seus saldos.
<img width="936" height="249" alt="image" src="https://github.com/user-attachments/assets/d47855cf-75f0-413b-93d8-365850892d73" />

### TRANSACOES              -> Guarda as transações válidas processadas.
<img width="1016" height="262" alt="image" src="https://github.com/user-attachments/assets/61b18c7d-1448-4ee3-92d2-f55cfc4feacd" />

### ERROS_PROCESSAMENTO     -> Guarda os erros encontrados durante o processamento.
<img width="950" height="230" alt="image" src="https://github.com/user-attachments/assets/51006a63-5280-43b0-8d1e-3b6b65f2f27f" />

### HISTORICO_TRANSACOES    -> Guarda o histórico das transações processadas.
<img width="952" height="360" alt="image" src="https://github.com/user-attachments/assets/e1812c51-6993-4e11-8b75-06fdf5491da1" />


## Relatórios gerados

O sistema gera os seguintes arquivos na pasta `OUTPUT`:

```text
RELATORIO.txt           -> Mostra totais de clientes, transações e erros.
DETALHADO.txt           -> Mostra cliente, operação e status do processamento.
ERROS.txt               -> Mostra os erros de regra de negócio encontrados.
LOG.txt                 -> Mostra mensagens gerais e respostas do DB2.
DB2_RESPONSE.txt        -> Mostra o retorno  do helper .
RELATORIO_DB2.txt       -> Mostra dados finais consultados diretamente do DB2.
```


## Observação sobre o ambiente

Em um ambiente mainframe real, a execução seria feita via JCL e DB2 no z/OS. Mas nesee projeto, a execução prática foi adaptada para ambiente local usando GnuCOBOL, DB2 Community e um helper em C# para comunicação com o banco de dados. O COBOL continua sendo responsável pela leitura dos arquivos, regras de negócio, geração dos logs e relatórios. O helperé usado apenas como ponte para conseguir executar os comandos SQL no DB2.

## Objetivo final

Este projeto demonstra a evolução do [Projeto 5](https://github.com/caiofsb/Projeto-Transacoes-Bancarias-Cobol--Montreal) mantendo a organização com COBOL, mas adicionando integração com banco de dados DB2, controle de erros, histórico de transações e geração de relatórios a partir do banco.
