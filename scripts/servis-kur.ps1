<#
.SYNOPSIS
    Portfolio sitesini Windows servisi olarak kurar ve baslatir.

.DESCRIPTION
    Bir kez, YONETICI olarak calistirilir. Yaptiklari:
      1. Makine duzeyinde environment variable'lari tanimlar
      2. Admin sifresini sorar (ekranda gorunmez) ve ADMIN_SEED_PASSWORD olarak yazar
      3. Servisi olusturur, cokunce otomatik yeniden baslatmayi ayarlar
      4. Servisi baslatir ve calistigini dogrular

    Sifre makine duzeyinde bir environment variable olarak saklanir. Bunu
    okuyabilecek olan, makinede yonetici olan herkestir -- yani zaten
    veritabanina da erisebilecek olan kisi.

.NOTES
    Servisi kaldirmak icin:  sc.exe delete PortfolioSite
    Loglar:                  C:\PortfolioApp\logs\
#>

[CmdletBinding()]
param(
    [string]$AppPath    = "C:\PortfolioApp",
    [string]$ServiceName = "PortfolioSite",
    [string]$PublicHost = "korayhan.tail476c72.ts.net",
    [int]$Port          = 5080,
    [string]$SqlServer  = "KORAYHAN\SQLEXPRESS",
    [string]$Database   = "PortfolioDb"
)

$ErrorActionPreference = "Stop"

# --- Yonetici kontrolu ---------------------------------------------------
$identity  = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Bu script YONETICI olarak calistirilmali." -ForegroundColor Red
    Write-Host "PowerShell'i saga tiklayip 'Yonetici olarak calistir' secip tekrar dene."
    exit 1
}

# --- Yayin ciktisi yerinde mi -------------------------------------------
$exe = Join-Path $AppPath "Portfolio.exe"
if (-not (Test-Path $exe)) {
    Write-Host "Yayin ciktisi bulunamadi: $exe" -ForegroundColor Red
    Write-Host "Once su komutu calistir: dotnet publish Portfolio -c Release -o $AppPath"
    exit 1
}

Write-Host "Yayin ciktisi bulundu: $exe" -ForegroundColor Green

# --- Environment variable'lar -------------------------------------------
$connectionString = "Server=$SqlServer;Database=$Database;Integrated Security=true;TrustServerCertificate=true"

# AllowedHosts'a localhost MUTLAKA dahil olmali: admin paneli tasarim geregi
# yalnizca loopback'ten erisiliyor, yani localhost reddedilirse panele hic
# girilemez (her istek 400 "does not match an allowed host" doner).
$allowedHosts = "$PublicHost;localhost;127.0.0.1"

$vars = @{
    "ASPNETCORE_ENVIRONMENT"                 = "Production"
    "ConnectionStrings__DefaultConnection"   = $connectionString
    "AllowedHosts"                           = $allowedHosts
    "Site__BaseUrl"                          = "https://$PublicHost"
}

foreach ($name in $vars.Keys) {
    [Environment]::SetEnvironmentVariable($name, $vars[$name], "Machine")
    Write-Host "  $name ayarlandi"
}

# --- Admin sifresi -------------------------------------------------------
# Veritabaninda hic kullanici yoksa uygulama bu degisken olmadan BASLAMAYI
# REDDEDER. Ilk baslatmada admin bu sifreyle olusturulur.
Write-Host ""
Write-Host "Admin sifresini gir (ekranda gorunmeyecek)." -ForegroundColor Cyan
Write-Host "Kural: en az 10 karakter, rakam + buyuk harf + kucuk harf zorunlu."
$secure = Read-Host -Prompt "ADMIN_SEED_PASSWORD" -AsSecureString

$bstr  = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
try {
    $plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)

    if ($plain.Length -lt 10) {
        Write-Host "Sifre 10 karakterden kisa. Uygulama bunu reddeder." -ForegroundColor Red
        exit 1
    }

    [Environment]::SetEnvironmentVariable("ADMIN_SEED_PASSWORD", $plain, "Machine")
    Write-Host "  ADMIN_SEED_PASSWORD ayarlandi" -ForegroundColor Green
}
finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
    Remove-Variable plain -ErrorAction SilentlyContinue
}

# --- Varsa eski servisi kaldir ------------------------------------------
$existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($null -ne $existing) {
    Write-Host ""
    Write-Host "Ayni isimde servis var, kaldiriliyor..." -ForegroundColor Yellow
    if ($existing.Status -ne "Stopped") {
        Stop-Service -Name $ServiceName -Force
    }
    & sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 2
}

# --- Servisi olustur ----------------------------------------------------
Write-Host ""
Write-Host "Servis olusturuluyor..." -ForegroundColor Cyan

# LocalSystem olarak calisir; bu hesaba PortfolioDb uzerinde db_owner verildi
# (yalnizca o veritabani, sunucu rolu yok).
& sc.exe create $ServiceName binPath= "`"$exe`"" start= auto obj= LocalSystem DisplayName= "Portfolio Sitesi" | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Servis olusturulamadi (sc.exe cikis kodu $LASTEXITCODE)." -ForegroundColor Red
    exit 1
}

& sc.exe description $ServiceName "Kisisel portfolyo sitesi (ASP.NET Core 8)" | Out-Null

# Cokunce otomatik yeniden baslat: 5sn, 10sn, 30sn; sayac gunde bir sifirlanir.
& sc.exe failure $ServiceName reset= 86400 actions= restart/5000/restart/10000/restart/30000 | Out-Null

Write-Host "  Servis olusturuldu, otomatik yeniden baslatma ayarlandi" -ForegroundColor Green

# --- Baslat ve dogrula --------------------------------------------------
Write-Host ""
Write-Host "Servis baslatiliyor..." -ForegroundColor Cyan
Start-Service -Name $ServiceName

$deadline = (Get-Date).AddSeconds(45)
$ok = $false
while ((Get-Date) -lt $deadline) {
    try {
        $r = Invoke-WebRequest -Uri "http://localhost:$Port/" -UseBasicParsing -TimeoutSec 5
        if ($r.StatusCode -eq 200) { $ok = $true; break }
    } catch {
        Start-Sleep -Seconds 2
    }
}

$svc = Get-Service -Name $ServiceName
Write-Host ""
Write-Host "Servis durumu : $($svc.Status)"

if ($ok) {
    Write-Host "Site yanit veriyor: http://localhost:$Port/ -> 200" -ForegroundColor Green
    Write-Host ""
    Write-Host "Sonraki adim: Tailscale Funnel'i ac, sonra" -ForegroundColor Cyan
    Write-Host "  https://$PublicHost" -ForegroundColor Cyan
    Write-Host "adresini disaridan dene."
} else {
    Write-Host "Site 45 saniyede yanit vermedi." -ForegroundColor Red
    Write-Host "Loglara bak: $AppPath\logs\"
    Write-Host "Windows olay gunlugu: Get-EventLog -LogName Application -Newest 20 | Where-Object Source -like '*Portfolio*'"
    exit 1
}
