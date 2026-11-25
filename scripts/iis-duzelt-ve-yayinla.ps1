<#
.SYNOPSIS
    IIS baglantisini yalnizca loopback'e kisitlar ve yeni kodu yayinlar.

.DESCRIPTION
    YONETICI olarak calistirilir.

    NEDEN: IIS varsayilan olarak 0.0.0.0:5080 dinliyor, yani yerel agdaki
    herkes porta ulasabiliyor. Windows servisi yalnizca 127.0.0.1 dinliyordu.
    Bu fark gercek bir acik yaratti: ayni agdaki biri sahte bir
    "Host: localhost" ve "X-Forwarded-For: 127.0.0.1" basligiyla LocalOnly
    kisitini asip admin giris sayfasina ulasabiliyordu.

    Iki katmanli duzeltme:
      1. IIS baglantisi 127.0.0.1'e kisitlanir (bu script)
      2. Uygulama yalnizca loopback'ten gelen BAGLANTILARIN
         X-Forwarded-For basligina guvenir (kod degisikligi, burada yayinlanir)

    Tunel istemcisi ayni makinede calistigi icin site disaridan erisilebilir
    kalir; degisen tek sey yerel agdan dogrudan erisimin kapanmasi.
#>

[CmdletBinding()]
param(
    [string]$RepoPath = "C:\Users\marve\source\repos\MyAcademy_PortfolioProject",
    [string]$AppPath  = "C:\PortfolioApp",
    [string]$SiteName = "PortfolioSite",
    [int]$Port        = 5080
)

$ErrorActionPreference = "Stop"

$identity  = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Bu script YONETICI olarak calistirilmali." -ForegroundColor Red
    exit 1
}

Import-Module WebAdministration -ErrorAction Stop

# --- 1) Siteyi durdur ----------------------------------------------------
Write-Host "`n[1/5] Site durduruluyor..." -ForegroundColor Cyan
Stop-Website -Name $SiteName -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
# App pool'u da durdur: dosyalar aksi halde kilitli kalir
$pool = (Get-Item "IIS:\Sites\$SiteName").applicationPool
Stop-WebAppPool -Name $pool -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

# --- 2) Baglantiyi loopback'e kisitla ------------------------------------
Write-Host "[2/5] Baglanti 127.0.0.1'e kisitlaniyor..." -ForegroundColor Cyan
Get-WebBinding -Name $SiteName | ForEach-Object {
    Remove-WebBinding -Name $SiteName -BindingInformation $_.bindingInformation -ErrorAction SilentlyContinue
}
New-WebBinding -Name $SiteName -IPAddress "127.0.0.1" -Port $Port -Protocol "http"
Get-WebBinding -Name $SiteName | ForEach-Object {
    Write-Host "  baglanti: $($_.bindingInformation)" -ForegroundColor Green
}

# --- 3) Gorselleri yedekle ve yayinla ------------------------------------
Write-Host "[3/5] Yayinlaniyor..." -ForegroundColor Cyan
$uploads = Join-Path $AppPath "wwwroot\uploads"
$yedek   = Join-Path $env:TEMP ("uploads-yedek-" + (Get-Date -Format "yyyyMMdd-HHmmss"))
if (Test-Path $uploads) {
    Copy-Item -Path $uploads -Destination $yedek -Recurse -Force
    Write-Host "  gorseller yedeklendi: $yedek"
}

Push-Location $RepoPath
try {
    & dotnet publish Portfolio -c Release -o $AppPath
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish basarisiz ($LASTEXITCODE)" }
}
finally { Pop-Location }

if (Test-Path $yedek) {
    New-Item -ItemType Directory -Force -Path $uploads | Out-Null
    Copy-Item -Path (Join-Path $yedek "*") -Destination $uploads -Recurse -Force
    Write-Host "  gorseller yerine kondu" -ForegroundColor Green
}

# publish izinleri sifirlamis olabilir
$kimlik = "IIS AppPool\$pool"
& icacls $AppPath /grant "${kimlik}:(OI)(CI)RX" /T /Q 2>&1 | Out-Null
& icacls $uploads /grant "${kimlik}:(OI)(CI)M" /T /Q 2>&1 | Out-Null
$logs = Join-Path $AppPath "logs"
New-Item -ItemType Directory -Force -Path $logs | Out-Null
& icacls $logs /grant "${kimlik}:(OI)(CI)M" /T /Q 2>&1 | Out-Null

# --- 4) Baslat -----------------------------------------------------------
Write-Host "[4/5] Site baslatiliyor..." -ForegroundColor Cyan
Start-WebAppPool -Name $pool -ErrorAction SilentlyContinue
Start-Website -Name $SiteName

# --- 5) Dogrula ----------------------------------------------------------
Write-Host "[5/5] Dogrulaniyor..." -ForegroundColor Cyan
$deadline = (Get-Date).AddSeconds(60)
$ok = $false
while ((Get-Date) -lt $deadline) {
    try {
        $r = Invoke-WebRequest -Uri "http://127.0.0.1:$Port/" -UseBasicParsing -TimeoutSec 5
        if ($r.StatusCode -eq 200) { $ok = $true; break }
    } catch { Start-Sleep -Seconds 3 }
}

Write-Host ""
if ($ok) {
    Write-Host "Tamam. Site localhost'tan yanit veriyor." -ForegroundColor Green
    Write-Host "Dinlenen adres (artik yalnizca loopback olmali):"
    netstat -ano | Select-String ":$Port\s" | Select-String "LISTENING"
    Write-Host "`nSimdi kontrolore haber ver: yerel agdan ve internetten test edilecek."
} else {
    Write-Host "Site 60 saniyede yanit vermedi. Loglar: $AppPath\logs\" -ForegroundColor Red
    exit 1
}
