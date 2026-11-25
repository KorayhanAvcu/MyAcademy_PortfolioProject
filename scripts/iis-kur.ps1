<#
.SYNOPSIS
    Siteyi Windows servisinden IIS'e tasir.

.DESCRIPTION
    YONETICI olarak calistirilir. Sirasiyla:
      1. IIS Windows ozelliklerini acar
      2. .NET 8 Hosting Bundle'i kurar (winget, Microsoft kaynagi)
      3. Calisan Windows servisini durdurur ve devre disi birakir
      4. Uygulama havuzu (app pool) ve siteyi olusturur
      5. App pool kimligine PortfolioDb uzerinde db_owner verir
      6. Siteyi baslatir ve dogrular

    Port 5080 korunur, boylece Tailscale Funnel ayari degismez.

    Windows servisi SILINMEZ, yalnizca devre disi birakilir -- bir sorun
    cikarsa geri donmek icin: Set-Service PortfolioSite -StartupType Automatic
    ve Start-Service PortfolioSite (once IIS sitesini durdur).
#>

[CmdletBinding()]
param(
    [string]$AppPath     = "C:\PortfolioApp",
    [string]$SiteName    = "PortfolioSite",
    [string]$PoolName    = "PortfolioSite",
    [string]$ServiceName = "PortfolioSite",
    [int]$Port           = 5080,
    [string]$SqlServer   = "KORAYHAN\SQLEXPRESS",
    [string]$Database    = "PortfolioDb"
)

$ErrorActionPreference = "Stop"

$identity  = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Bu script YONETICI olarak calistirilmali." -ForegroundColor Red
    exit 1
}

# --- 1) IIS ozellikleri --------------------------------------------------
Write-Host "`n[1/6] IIS ozellikleri aciliyor..." -ForegroundColor Cyan
$ozellikler = @(
    "IIS-WebServerRole", "IIS-WebServer", "IIS-CommonHttpFeatures",
    "IIS-StaticContent", "IIS-DefaultDocument", "IIS-HttpErrors",
    "IIS-HttpLogging", "IIS-RequestFiltering", "IIS-Security",
    "IIS-WebServerManagementTools", "IIS-ManagementConsole"
)
$restartGerekli = $false
foreach ($o in $ozellikler) {
    $f = Get-WindowsOptionalFeature -Online -FeatureName $o -ErrorAction SilentlyContinue
    if ($null -eq $f) { Write-Host "  $o bulunamadi, atlandi" -ForegroundColor Yellow; continue }
    if ($f.State -ne "Enabled") {
        $r = Enable-WindowsOptionalFeature -Online -FeatureName $o -NoRestart -ErrorAction Stop
        if ($r.RestartNeeded) { $restartGerekli = $true }
        Write-Host "  $o acildi"
    }
}
Write-Host "  IIS ozellikleri hazir" -ForegroundColor Green

# --- 2) Hosting Bundle ---------------------------------------------------
Write-Host "`n[2/6] .NET 8 Hosting Bundle..." -ForegroundColor Cyan
$ancm = "C:\Program Files\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll"
if (Test-Path $ancm) {
    Write-Host "  Zaten kurulu" -ForegroundColor Green
} else {
    Write-Host "  winget ile kuruluyor (Microsoft kaynagi, birkac dakika surebilir)..."
    & winget install --id Microsoft.DotNet.HostingBundle.8 --exact --silent `
        --accept-source-agreements --accept-package-agreements
    if (-not (Test-Path $ancm)) {
        Write-Host "  Hosting Bundle kurulamadi." -ForegroundColor Red
        Write-Host "  Elle kur: https://dotnet.microsoft.com/download/dotnet/8.0 -> 'Hosting Bundle'"
        exit 1
    }
    Write-Host "  Kuruldu" -ForegroundColor Green
    & net stop was /y 2>&1 | Out-Null
    & net start w3svc 2>&1 | Out-Null
}

Import-Module WebAdministration -ErrorAction Stop

# --- 3) Windows servisini durdur ----------------------------------------
Write-Host "`n[3/6] Windows servisi devre disi birakiliyor..." -ForegroundColor Cyan
$svc = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($null -ne $svc) {
    if ($svc.Status -ne "Stopped") { Stop-Service -Name $ServiceName -Force; Start-Sleep 3 }
    Set-Service -Name $ServiceName -StartupType Disabled
    Write-Host "  Durduruldu ve devre disi birakildi (silinmedi, geri donulebilir)" -ForegroundColor Green
} else {
    Write-Host "  Servis bulunamadi, atlandi" -ForegroundColor Yellow
}

# Port 80'deki varsayilan siteyi kapat: disariya acik gereksiz bir dinleyici kalmasin
if (Test-Path "IIS:\Sites\Default Web Site") {
    Stop-Website -Name "Default Web Site" -ErrorAction SilentlyContinue
    Set-ItemProperty "IIS:\Sites\Default Web Site" -Name serverAutoStart -Value $false
    Write-Host "  'Default Web Site' durduruldu" -ForegroundColor Green
}

# --- 4) App pool ve site -------------------------------------------------
Write-Host "`n[4/6] Uygulama havuzu ve site olusturuluyor..." -ForegroundColor Cyan

if (Test-Path "IIS:\AppPools\$PoolName") { Remove-WebAppPool -Name $PoolName }
New-WebAppPool -Name $PoolName | Out-Null
# ASP.NET Core CLR hattini kullanmaz: "No Managed Code"
Set-ItemProperty "IIS:\AppPools\$PoolName" -Name managedRuntimeVersion -Value ""
Set-ItemProperty "IIS:\AppPools\$PoolName" -Name startMode -Value "AlwaysRunning"
Set-ItemProperty "IIS:\AppPools\$PoolName" -Name processModel.idleTimeout -Value "00:00:00"
Set-ItemProperty "IIS:\AppPools\$PoolName" -Name recycling.periodicRestart.time -Value "00:00:00"
Write-Host "  App pool: $PoolName (No Managed Code, hep calisir)"

if (Test-Path "IIS:\Sites\$SiteName") { Remove-Website -Name $SiteName }
New-Website -Name $SiteName -PhysicalPath $AppPath -ApplicationPool $PoolName `
            -Port $Port -HostHeader "" | Out-Null
Set-ItemProperty "IIS:\Sites\$SiteName" -Name serverAutoStart -Value $true
Write-Host "  Site: $SiteName -> $AppPath, port $Port" -ForegroundColor Green

# App pool kimliginin yayin klasorunu okuyabilmesi gerekiyor
$kimlik = "IIS AppPool\$PoolName"
& icacls $AppPath /grant "${kimlik}:(OI)(CI)RX" /T /Q 2>&1 | Out-Null
# uploads klasorune YAZMA da gerekiyor: gorseller buraya yukleniyor
$uploads = Join-Path $AppPath "wwwroot\uploads"
New-Item -ItemType Directory -Force -Path $uploads | Out-Null
& icacls $uploads /grant "${kimlik}:(OI)(CI)M" /T /Q 2>&1 | Out-Null
# loglar da yazilabilir olmali
$logs = Join-Path $AppPath "logs"
New-Item -ItemType Directory -Force -Path $logs | Out-Null
& icacls $logs /grant "${kimlik}:(OI)(CI)M" /T /Q 2>&1 | Out-Null
Write-Host "  Klasor izinleri verildi (uploads ve logs yazilabilir)" -ForegroundColor Green

# --- 5) SQL izni ---------------------------------------------------------
Write-Host "`n[5/6] SQL izni veriliyor..." -ForegroundColor Cyan
$sql = @"
SET QUOTED_IDENTIFIER ON;
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$kimlik')
    CREATE LOGIN [$kimlik] FROM WINDOWS;
USE [$Database];
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$kimlik')
    CREATE USER [$kimlik] FOR LOGIN [$kimlik];
ALTER ROLE db_owner ADD MEMBER [$kimlik];
"@
$sql | & sqlcmd -S $SqlServer -E -b
if ($LASTEXITCODE -ne 0) { Write-Host "  SQL izni verilemedi." -ForegroundColor Red; exit 1 }
Write-Host "  $kimlik -> $Database uzerinde db_owner" -ForegroundColor Green

# --- 6) Baslat ve dogrula ------------------------------------------------
Write-Host "`n[6/6] Site baslatiliyor..." -ForegroundColor Cyan
Start-WebAppPool -Name $PoolName -ErrorAction SilentlyContinue
Start-Website -Name $SiteName

$deadline = (Get-Date).AddSeconds(60)
$ok = $false
while ((Get-Date) -lt $deadline) {
    try {
        $r = Invoke-WebRequest -Uri "http://localhost:$Port/" -UseBasicParsing -TimeoutSec 5
        if ($r.StatusCode -eq 200) { $ok = $true; break }
    } catch { Start-Sleep -Seconds 3 }
}

Write-Host ""
if ($ok) {
    Write-Host "IIS uzerinden yayinda: http://localhost:$Port/" -ForegroundColor Green
    if ($restartGerekli) {
        Write-Host "NOT: Windows bazi IIS ozellikleri icin yeniden baslatma istiyor." -ForegroundColor Yellow
    }
    Write-Host "Simdi kontrolore haber ver: disaridan guvenlik testi yapilacak."
} else {
    Write-Host "Site 60 saniyede yanit vermedi." -ForegroundColor Red
    Write-Host "Bak: $AppPath\logs\  ve  Get-EventLog -LogName Application -Newest 20"
    Write-Host "Geri donmek icin: Stop-Website $SiteName; Set-Service $ServiceName -StartupType Automatic; Start-Service $ServiceName"
    exit 1
}
