using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Globalization;
using System.IO;

class Program
{
    static string ConnStr =
        "DRIVER={IBM DB2 ODBC DRIVER};" +
        "DATABASE=PROJ6DB;" +
        "HOSTNAME=127.0.0.1;" +
        "PORT=50000;" +
        "PROTOCOL=TCPIP;" +
        "UID=db2inst1;" +
        "PWD= Mudar quando for rodar;";

    static string BaseDir = Directory.GetCurrentDirectory();
    static string OutDir = Path.Combine(BaseDir, "OUTPUT");
    static string RequestFile = Path.Combine(OutDir, "DB2_REQUEST.txt");
    static string ResponseFile = Path.Combine(OutDir, "DB2_RESPONSE.txt");
    static string RelatorioDb2File = Path.Combine(OutDir, "RELATORIO_DB2.txt");

    static List<string> Respostas = new List<string>();

    static void AddResp(string texto)
    {
        Respostas.Add(texto);
    }

    static void SaveResp()
    {
        Directory.CreateDirectory(OutDir);
        File.WriteAllLines(ResponseFile, Respostas);
    }

    static OdbcConnection OpenConnection()
    {
        var conn = new OdbcConnection(ConnStr);
        conn.Open();
        return conn;
    }

    static int ToInt(string value)
    {
        return int.Parse(value.Trim(), CultureInfo.InvariantCulture);
    }

    static string Id5(int value)
    {
        return value.ToString("00000");
    }

    static string Money(int value)
    {
        return value.ToString("000000000");
    }

    static OdbcCommand Cmd(OdbcConnection conn, OdbcTransaction tx, string sql)
    {
        var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        return cmd;
    }

    static int Testar()
    {
        try
        {
            using var conn = OpenConnection();
            Respostas.Clear();
            AddResp("OK|CONEXAO COM DB2 FUNCIONANDO");
            SaveResp();
            return 0;
        }
        catch (Exception ex)
        {
            Respostas.Clear();
            AddResp("ERRO|" + ex.Message);
            SaveResp();
            return 8;
        }
    }

    static void LimparTabelas(OdbcConnection conn, OdbcTransaction tx)
    {
        using (var cmd = Cmd(conn, tx, "DELETE FROM HISTORICO_TRANSACOES"))
        {
            cmd.ExecuteNonQuery();
        }

        using (var cmd = Cmd(conn, tx, "DELETE FROM ERROS_PROCESSAMENTO"))
        {
            cmd.ExecuteNonQuery();
        }

        using (var cmd = Cmd(conn, tx, "DELETE FROM TRANSACOES"))
        {
            cmd.ExecuteNonQuery();
        }

        using (var cmd = Cmd(conn, tx, "DELETE FROM CLIENTES"))
        {
            cmd.ExecuteNonQuery();
        }

        AddResp("OK|TABELAS LIMPAS");
    }

    static void UpsertCliente(OdbcConnection conn, OdbcTransaction tx, string[] p)
    {
        int cliId = ToInt(p[1]);
        string nome = p[2].Trim();
        int saldo = ToInt(p[3]);

        using var check = Cmd(conn, tx, "SELECT COUNT(*) FROM CLIENTES WHERE CLI_ID = ?");
        check.Parameters.Add("CLI_ID", OdbcType.Int).Value = cliId;

        int existe = Convert.ToInt32(check.ExecuteScalar());

        if (existe > 0)
        {
            using var update = Cmd(conn, tx,
                "UPDATE CLIENTES SET CLI_NOME = ?, CLI_SALDO = ?, DT_ATUALIZACAO = CURRENT DATE WHERE CLI_ID = ?");

            update.Parameters.Add("CLI_NOME", OdbcType.VarChar, 30).Value = nome;
            update.Parameters.Add("CLI_SALDO", OdbcType.Decimal).Value = saldo;
            update.Parameters.Add("CLI_ID", OdbcType.Int).Value = cliId;
            update.ExecuteNonQuery();

            AddResp($"OK|CLIENTE ATUALIZADO|{Id5(cliId)}");
        }
        else
        {
            using var insert = Cmd(conn, tx,
                "INSERT INTO CLIENTES (CLI_ID, CLI_NOME, CLI_SALDO, DT_ATUALIZACAO) VALUES (?, ?, ?, CURRENT DATE)");

            insert.Parameters.Add("CLI_ID", OdbcType.Int).Value = cliId;
            insert.Parameters.Add("CLI_NOME", OdbcType.VarChar, 30).Value = nome;
            insert.Parameters.Add("CLI_SALDO", OdbcType.Decimal).Value = saldo;
            insert.ExecuteNonQuery();

            AddResp($"OK|CLIENTE INSERIDO|{Id5(cliId)}");
        }
    }

    static void AtualizarSaldo(OdbcConnection conn, OdbcTransaction tx, string[] p)
    {
        int cliId = ToInt(p[1]);
        int saldo = ToInt(p[2]);

        using var cmd = Cmd(conn, tx,
            "UPDATE CLIENTES SET CLI_SALDO = ?, DT_ATUALIZACAO = CURRENT DATE WHERE CLI_ID = ?");

        cmd.Parameters.Add("CLI_SALDO", OdbcType.Decimal).Value = saldo;
        cmd.Parameters.Add("CLI_ID", OdbcType.Int).Value = cliId;

        int rows = cmd.ExecuteNonQuery();

        if (rows == 0)
        {
            throw new Exception("CLIENTE NAO ENCONTRADO NO UPDATE");
        }

        AddResp($"OK|SALDO ATUALIZADO|{Id5(cliId)}|{Money(saldo)}");
    }

    static void InserirTransacao(OdbcConnection conn, OdbcTransaction tx, string[] p)
    {
        int trxId = ToInt(p[1]);
        int cliId = ToInt(p[2]);
        string tipo = p[3].Trim();
        int valor = ToInt(p[4]);
        int saldoAnt = ToInt(p[5]);
        int saldoNovo = ToInt(p[6]);
        string status = p[7].Trim();
        string desc = p[8].Trim();

        using var cmd = Cmd(conn, tx,
            "INSERT INTO TRANSACOES (TRX_ID, CLI_ID, TRX_TIPO, TRX_VALOR, DT_PROCESSAMENTO) VALUES (?, ?, ?, ?, CURRENT DATE)");

        cmd.Parameters.Add("TRX_ID", OdbcType.Int).Value = trxId;
        cmd.Parameters.Add("CLI_ID", OdbcType.Int).Value = cliId;
        cmd.Parameters.Add("TRX_TIPO", OdbcType.Char, 1).Value = tipo;
        cmd.Parameters.Add("TRX_VALOR", OdbcType.Decimal).Value = valor;
        cmd.ExecuteNonQuery();

        InserirHistorico(conn, tx, trxId, cliId, tipo, valor, saldoAnt, saldoNovo, status, desc);

        AddResp($"OK|TRANSACAO INSERIDA|{trxId}");
    }

    static void InserirErro(OdbcConnection conn, OdbcTransaction tx, string[] p)
    {
        int cliId = ToInt(p[1]);
        string descricao = p[2].Trim();

        int trxId = 0;
        string tipo = "";
        int valor = 0;
        int saldoAnt = 0;
        int saldoNovo = 0;

        if (p.Length > 3)
        {
            trxId = ToInt(p[3]);
            tipo = p[4].Trim();
            valor = ToInt(p[5]);
            saldoAnt = ToInt(p[6]);
            saldoNovo = ToInt(p[7]);
        }

        using var cmd = Cmd(conn, tx,
            "INSERT INTO ERROS_PROCESSAMENTO (CLI_ID, DESCRICAO_ERRO, DT_OCORRENCIA) VALUES (?, ?, CURRENT TIMESTAMP)");

        cmd.Parameters.Add("CLI_ID", OdbcType.Int).Value = cliId;
        cmd.Parameters.Add("DESCRICAO_ERRO", OdbcType.VarChar, 100).Value = descricao;
        cmd.ExecuteNonQuery();

        if (trxId > 0)
        {
            InserirHistorico(conn, tx, trxId, cliId, tipo, valor, saldoAnt, saldoNovo, "ERRO", descricao);
        }

        AddResp($"OK|ERRO INSERIDO|{Id5(cliId)}|{descricao}");
    }

    static void InserirHistorico(
        OdbcConnection conn,
        OdbcTransaction tx,
        int trxId,
        int cliId,
        string tipo,
        int valor,
        int saldoAnt,
        int saldoNovo,
        string status,
        string descricao)
    {
        using var cmd = Cmd(conn, tx,
            "INSERT INTO HISTORICO_TRANSACOES " +
            "(TRX_ID, CLI_ID, TRX_TIPO, TRX_VALOR, SALDO_ANTERIOR, SALDO_NOVO, STATUS_PROCESSAMENTO, DESCRICAO, DT_HISTORICO) " +
            "VALUES (?, ?, ?, ?, ?, ?, ?, ?, CURRENT TIMESTAMP)");

        cmd.Parameters.Add("TRX_ID", OdbcType.Int).Value = trxId;
        cmd.Parameters.Add("CLI_ID", OdbcType.Int).Value = cliId;
        cmd.Parameters.Add("TRX_TIPO", OdbcType.Char, 1).Value = tipo;
        cmd.Parameters.Add("TRX_VALOR", OdbcType.Decimal).Value = valor;
        cmd.Parameters.Add("SALDO_ANTERIOR", OdbcType.Decimal).Value = saldoAnt;
        cmd.Parameters.Add("SALDO_NOVO", OdbcType.Decimal).Value = saldoNovo;
        cmd.Parameters.Add("STATUS_PROCESSAMENTO", OdbcType.VarChar, 30).Value = status;
        cmd.Parameters.Add("DESCRICAO", OdbcType.VarChar, 100).Value = descricao;
        cmd.ExecuteNonQuery();
    }

    static void GerarRelatorioDb2(OdbcConnection conn, OdbcTransaction tx)
    {
        var linhas = new List<string>();

        linhas.Add("RELATORIO FINAL CONSULTANDO DIRETAMENTE O DB2");
        linhas.Add("---------------------------------------------");

        linhas.Add("TOTAL CLIENTES............: " + Count(conn, tx, "CLIENTES"));
        linhas.Add("TOTAL TRANSACOES..........: " + Count(conn, tx, "TRANSACOES"));
        linhas.Add("TOTAL ERROS...............: " + Count(conn, tx, "ERROS_PROCESSAMENTO"));
        linhas.Add("TOTAL HISTORICO...........: " + Count(conn, tx, "HISTORICO_TRANSACOES"));
        linhas.Add("");

        linhas.Add("CLIENTES");
        linhas.Add("-------");

        using (var cmd = Cmd(conn, tx,
            "SELECT CLI_ID, CLI_NOME, CLI_SALDO FROM CLIENTES ORDER BY CLI_ID"))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                int cliId = reader.GetInt32(0);
                string nome = reader.GetString(1).Trim();
                int saldo = Convert.ToInt32(reader.GetDecimal(2));

                linhas.Add($"{Id5(cliId)} {nome,-30} {Money(saldo)}");
            }
        }

        linhas.Add("");
        linhas.Add("HISTORICO");
        linhas.Add("---------");

        using (var cmd = Cmd(conn, tx,
            "SELECT CLI_ID, TRX_ID, TRX_TIPO, TRX_VALOR, SALDO_ANTERIOR, SALDO_NOVO, STATUS_PROCESSAMENTO " +
            "FROM HISTORICO_TRANSACOES ORDER BY CLI_ID, TRX_ID"))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                int cliId = reader.GetInt32(0);
                int trxId = reader.GetInt32(1);
                string tipo = reader.GetString(2).Trim();
                int valor = Convert.ToInt32(reader.GetDecimal(3));
                int saldoAnt = Convert.ToInt32(reader.GetDecimal(4));
                int saldoNovo = Convert.ToInt32(reader.GetDecimal(5));
                string status = reader.GetString(6).Trim();

                linhas.Add($"{Id5(cliId)} TRX {trxId:00000} {tipo} {Money(valor)} {Money(saldoAnt)}->{Money(saldoNovo)} {status}");
            }
        }

        File.WriteAllLines(RelatorioDb2File, linhas);
        AddResp("OK|RELATORIO DB2 GERADO");
    }

    static int Count(OdbcConnection conn, OdbcTransaction tx, string tabela)
    {
        using var cmd = Cmd(conn, tx, $"SELECT COUNT(*) FROM {tabela}");
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    static int ExecBatch()
    {
        Respostas.Clear();

        try
        {
            if (!File.Exists(RequestFile))
            {
                AddResp("ERRO|ARQUIVO DB2_REQUEST NAO ENCONTRADO");
                SaveResp();
                return 8;
            }

            using var conn = OpenConnection();
            var tx = conn.BeginTransaction();

            int contador = 0;

            foreach (string raw in File.ReadAllLines(RequestFile))
            {
                string line = raw.Trim();

                if (line.Length == 0)
                {
                    continue;
                }

                string[] p = line.Split('|');
                string comando = p[0].Trim().ToUpperInvariant();

                try
                {
                    if (comando == "LIMPAR_TABELAS")
                    {
                        LimparTabelas(conn, tx);
                    }
                    else if (comando == "UPSERT_CLIENTE")
                    {
                        UpsertCliente(conn, tx, p);
                        contador++;
                    }
                    else if (comando == "INSERIR_TRANSACAO")
                    {
                        InserirTransacao(conn, tx, p);
                        contador++;
                    }
                    else if (comando == "ATUALIZAR_SALDO")
                    {
                        AtualizarSaldo(conn, tx, p);
                    }
                    else if (comando == "INSERIR_ERRO")
                    {
                        InserirErro(conn, tx, p);
                        contador++;
                    }
                    else if (comando == "GERAR_RELATORIO_DB2")
                    {
                        GerarRelatorioDb2(conn, tx);
                    }
                    else
                    {
                        AddResp("ERRO|COMANDO INVALIDO|" + comando);
                    }

                    if (contador > 0 && contador % 100 == 0)
                    {
                        tx.Commit();
                        AddResp("OK|COMMIT A CADA 100 REGISTROS");
                        tx = conn.BeginTransaction();
                    }
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    AddResp("ROLLBACK|ERRO SQL|" + ex.Message);
                    tx = conn.BeginTransaction();
                }
            }

            tx.Commit();
            AddResp("OK|COMMIT FINAL EXECUTADO");
            SaveResp();
            return 0;
        }
        catch (Exception ex)
        {
            AddResp("ERRO_GERAL|" + ex.Message);
            SaveResp();
            return 8;
        }
    }

    static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Respostas.Clear();
            AddResp("ERRO|COMANDO NAO INFORMADO");
            SaveResp();
            return 8;
        }

        string comando = args[0].Trim().ToLowerInvariant();

        if (comando == "testar")
        {
            return Testar();
        }

        if (comando == "exec-batch")
        {
            return ExecBatch();
        }

        Respostas.Clear();
        AddResp("ERRO|COMANDO INVALIDO");
        SaveResp();
        return 8;
    }
}