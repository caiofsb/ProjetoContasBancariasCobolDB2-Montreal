       7000-DB-EXEC-BATCH.
           MOVE "helper\Db2Helper.exe exec-batch" TO WKR-CMD.
           CALL "SYSTEM" USING WKR-CMD.

       7100-DB-GRAVAR-REQ.
           WRITE REG-DB2-REQ FROM WKR-DB2-REQ.

       7200-DB-REQ-LIMPAR.
           MOVE "LIMPAR_TABELAS" TO WKR-DB2-REQ.
           PERFORM 7100-DB-GRAVAR-REQ.

       7300-DB-REQ-UPSERT-CLIENTE.
           MOVE SPACES TO WKR-DB2-REQ.
           STRING "UPSERT_CLIENTE|"
                  WKR-CLI-ID "|"
                  WKR-CLI-NOME "|"
                  WKR-CLI-SALDO
                  DELIMITED BY SIZE
                  INTO WKR-DB2-REQ.
           PERFORM 7100-DB-GRAVAR-REQ.

       7400-DB-REQ-TRANSACAO.
           MOVE SPACES TO WKR-DB2-REQ.
           STRING "INSERIR_TRANSACAO|"
                  WKR-TRX-ID "|"
                  WKR-TRX-CLI-ID "|"
                  WKR-TRX-TIPO "|"
                  WKR-TRX-VALOR "|"
                  WKR-SALDO-ANT "|"
                  WKR-NOVO-SALDO "|"
                  "PROCESSADA|OK"
                  DELIMITED BY SIZE
                  INTO WKR-DB2-REQ.
           PERFORM 7100-DB-GRAVAR-REQ.

       7500-DB-REQ-ATUALIZAR-SALDO.
           MOVE SPACES TO WKR-DB2-REQ.
           STRING "ATUALIZAR_SALDO|"
                  WKR-TRX-CLI-ID "|"
                  WKR-NOVO-SALDO
                  DELIMITED BY SIZE
                  INTO WKR-DB2-REQ.
           PERFORM 7100-DB-GRAVAR-REQ.

       7600-DB-REQ-ERRO.
           MOVE SPACES TO WKR-DB2-REQ.
           STRING "INSERIR_ERRO|"
                  WKR-CLI-ID "|"
                  WKR-DESC-ERRO "|"
                  WKR-TRX-ID "|"
                  WKR-TRX-TIPO "|"
                  WKR-TRX-VALOR "|"
                  WKR-SALDO-ANT "|"
                  WKR-NOVO-SALDO
                  DELIMITED BY SIZE
                  INTO WKR-DB2-REQ.
           PERFORM 7100-DB-GRAVAR-REQ.

       7700-DB-REQ-RELATORIO.
           MOVE "GERAR_RELATORIO_DB2" TO WKR-DB2-REQ.
           PERFORM 7100-DB-GRAVAR-REQ.