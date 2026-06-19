cd C:\Dev\Projeto6

if (!(Test-Path OUTPUT)) {
    New-Item -ItemType Directory OUTPUT | Out-Null
}

$env:PATH = "C:\IBM\clidriver\bin;$env:PATH"

Get-Content .\INPUT\CLIENTES.txt |
    Where-Object { $_.Trim().Length -gt 0 } |
    Sort-Object { $_.Substring(0,5) } |
    Set-Content -Encoding ASCII .\OUTPUT\CLIENTES_ORD.txt

Get-Content .\INPUT\TRANSACOES.txt |
    Where-Object { $_.Trim().Length -gt 0 } |
    Sort-Object { $_.Substring(0,5) + $_.Substring(5,5) } |
    Set-Content -Encoding ASCII .\OUTPUT\TRANSACOES_ORD.txt

if (!(Test-Path .\helper\Db2Helper.exe)) {
    dotnet publish .\DB2HELPER\Db2Helper.csproj -c Release -r win-x64 --self-contained false -o .\helper
}

cobc -x -free .\COBOL\PROJETO6.cbl -I .\COPYS -o PROJETO6.exe

.\PROJETO6.exe