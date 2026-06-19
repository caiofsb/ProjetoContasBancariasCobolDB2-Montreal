           05  WKR-FIM-CLI            PIC X(01) VALUE 'N'.
           05  WKR-FIM-TRX            PIC X(01) VALUE 'N'.
           05  WKR-FIM-RESP           PIC X(01) VALUE 'N'.

           05  WKR-CLI-ID             PIC 9(05) VALUE ZEROS.
           05  WKR-CLI-NOME           PIC X(30) VALUE SPACES.
           05  WKR-CLI-SALDO          PIC 9(09) VALUE ZEROS.

           05  WKR-TRX-CLI-ID         PIC 9(05) VALUE ZEROS.
           05  WKR-TRX-ID             PIC 9(05) VALUE ZEROS.
           05  WKR-TRX-TIPO           PIC X(01) VALUE SPACES.
           05  WKR-TRX-VALOR          PIC 9(09) VALUE ZEROS.

           05  WKR-SALDO-ATUAL        PIC 9(09) VALUE ZEROS.
           05  WKR-SALDO-ANT          PIC 9(09) VALUE ZEROS.
           05  WKR-NOVO-SALDO         PIC 9(09) VALUE ZEROS.

           05  WKR-CLI-LIDOS          PIC 9(06) VALUE ZEROS.
           05  WKR-CLI-PROCESSADOS    PIC 9(06) VALUE ZEROS.
           05  WKR-TRX-LIDOS          PIC 9(06) VALUE ZEROS.
           05  WKR-TRX-PROCESSADAS    PIC 9(06) VALUE ZEROS.
           05  WKR-CREDITOS           PIC 9(06) VALUE ZEROS.
           05  WKR-DEBITOS            PIC 9(06) VALUE ZEROS.
           05  WKR-ERROS              PIC 9(06) VALUE ZEROS.
           05  WKR-COMMITS            PIC 9(06) VALUE ZEROS.

           05  WKR-CMD                PIC X(300) VALUE SPACES.
           05  WKR-DB2-REQ            PIC X(500) VALUE SPACES.
           05  WKR-RETORNO-DB2        PIC X(500) VALUE SPACES.

           05  WKR-DESC-ERRO          PIC X(100) VALUE SPACES.
           05  WKR-OPERACAO           PIC X(30) VALUE SPACES.
           05  WKR-STATUS-LINHA       PIC X(100) VALUE SPACES.
           05  WKR-LINHA              PIC X(132) VALUE SPACES.

           05  WKR-IDX                PIC 9(04) VALUE ZEROS.
           05  WKR-CLI-QTD            PIC 9(04) VALUE ZEROS.
           05  WKR-ACHOU              PIC X(01) VALUE 'N'.
           05  WKR-CLI-STATUS         PIC X(20) VALUE SPACES.

           05  WKR-TABELA-CLIENTES OCCURS 1000 TIMES.
               10  WKR-TAB-CLI-ID     PIC 9(05) VALUE ZEROS.
               10  WKR-TAB-CLI-NOME   PIC X(30) VALUE SPACES.
               10  WKR-TAB-CLI-SALDO  PIC 9(09) VALUE ZEROS.