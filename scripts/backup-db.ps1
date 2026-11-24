<#
.SYNOPSIS
  PortfolioDb veritabanini ve wwwroot/uploads klasorunu yedekler.

.DESCRIPTION
  1) BACKUP DATABASE (sikistirmasiz: SQL Server Express WITH COMPRESSION desteklemez, Msg 1844)
  2) RESTORE VERIFYONLY ile yedegi dogrular; dogrulanamazsa script hata ile durur
  3) Portfolio/wwwroot/uploads klasorunu zaman damgali bir klasore kopyalar
     (yuklenen gorseller veritabaninda degil diskte durur; yalnizca veritabani
     yedegi gorselleri sessizce kaybettirir)

  Zamanlama (Task Scheduler) ve geri yukleme testi sahibin yapacagi adimlardir; bu script yapmaz.

.PARAMETER Destination
  Yedeklerin yazilacagi klasor. Ikinci bir diski gostermeniz onerilir (ornek: D:\Yedek\Portfolio).
  SQL Server hizmet hesabi bu klasore yazabilmelidir.

.PARAMETER Server
  SQL Server ornegi. Windows kimlik dogrulamasi kullanilir.

.PARAMETER Database
  Yedeklenecek veritabani.

.PARAMETER UploadsPath
  Kopyalanacak uploads klasoru. Varsayilan: scriptin bir ust klasorundeki Portfolio\wwwroot\uploads.

.PARAMETER KeepDays
  Bu gunden eski yedekler silinir (yalnizca bu scriptin urettigi adli dosya/klasorler). 0 = silme.
#>
[CmdletBinding()]
param(
    [string]$Destination = 'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup\',
    [string]$Server = 'KORAYHAN\SQLEXPRESS',
    [string]$Database = 'PortfolioDb',
    [string]$UploadsPath = '',
    [int]$KeepDays = 0
)

$ErrorActionPreference = 'Stop'

if (-not $UploadsPath) {
    $UploadsPath = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) '..\Portfolio\wwwroot\uploads'
}

function Invoke-Sql([string]$Query) {
    & sqlcmd -S $Server -E -b -h -1 -W -Q $Query
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd hata kodu $LASTEXITCODE ile bitti: $Query" }
}

if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw "sqlcmd bulunamadi. SQL Server komut satiri araclarini kurun ya da PATH'e ekleyin."
}
if (-not (Test-Path -LiteralPath $UploadsPath)) {
    throw "Uploads klasoru bulunamadi: $UploadsPath"
}
if (-not (Test-Path -LiteralPath $Destination)) {
    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
}

$stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$bakFile = Join-Path $Destination "${Database}_$stamp.bak"
$uploadsCopy = Join-Path $Destination "${Database}_uploads_$stamp"

# 1) Veritabani yedegi. COMPRESSION bilincli olarak YOK (Express desteklemez).
Write-Host "Veritabani yedekleniyor: $bakFile"
Invoke-Sql "BACKUP DATABASE [$Database] TO DISK = N'$bakFile' WITH INIT, CHECKSUM, STATS = 10"

# 2) Dogrulama. Basarisizsa Invoke-Sql firlatir ve script durur.
Write-Host 'Yedek dogrulaniyor (RESTORE VERIFYONLY)...'
Invoke-Sql "RESTORE VERIFYONLY FROM DISK = N'$bakFile' WITH CHECKSUM"
if (-not (Test-Path -LiteralPath $bakFile)) { throw "Yedek dosyasi olusmadi: $bakFile" }

# 3) Yuklenen gorseller
Write-Host "Uploads kopyalaniyor: $uploadsCopy"
Copy-Item -LiteralPath $UploadsPath -Destination $uploadsCopy -Recurse -Force
$srcCount = (Get-ChildItem -LiteralPath $UploadsPath -Recurse -File).Count
$dstCount = (Get-ChildItem -LiteralPath $uploadsCopy -Recurse -File).Count
if ($srcCount -ne $dstCount) {
    throw "Uploads kopyasi eksik: kaynak $srcCount dosya, kopya $dstCount dosya."
}

# 4) Eski yedekleri temizle (yalnizca bu scriptin adlandirmasina uyanlar)
if ($KeepDays -gt 0) {
    $limit = (Get-Date).AddDays(-$KeepDays)
    Get-ChildItem -LiteralPath $Destination -Filter "${Database}_2*.bak" -File |
        Where-Object LastWriteTime -lt $limit | Remove-Item -Force
    Get-ChildItem -LiteralPath $Destination -Filter "${Database}_uploads_*" -Directory |
        Where-Object LastWriteTime -lt $limit | Remove-Item -Recurse -Force
}

Write-Host "TAMAM. Veritabani: $bakFile ($srcCount dosya uploads kopyalandi)"

