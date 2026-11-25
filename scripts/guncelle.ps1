<#
.SYNOPSIS
    Yeni kodu derleyip calisan servise yayinlar.

.DESCRIPTION
    YONETICI olarak calistirilir. Sirasiyla:
      1. Servisi durdurur (dosyalar kilitli oldugu icin sart)
      2. Yuklenen gorselleri gecici bir yere kopyalar
      3. dotnet publish calistirir
      4. Gorselleri geri koyar (publish ciktiyi temizlerse kaybolmasinlar)
      5. Servisi baslatir ve sitenin yanit verdigini dogrular

    Gorsel yedegi onemli: wwwroot/uploads icerigi kaynak kodda degil, yalnizca
    yayin klasorunde ve veritabanindaki kayitlar oraya isaret ediyor.
#>

[CmdletBinding()]
param(
    [string]$RepoPath    = "C:\Users\marve\source\repos\MyAcademy_PortfolioProject",
    [string]$AppPath     = "C:\PortfolioApp",
    [string]$ServiceName = "PortfolioSite",
    [int]$Port           = 5080
)

$ErrorActionPreference = "Stop"

$identity  = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Bu script YONETICI olarak calistirilmali." -ForegroundColor Red
    exit 1
}

$uploads = Join-Path $AppPath "wwwroot\uploads"
$yedek   = Join-Path $env:TEMP ("uploads-yedek-" + (Get-Date -Format "yyyyMMdd-HHmmss"))

Write-Host "Servis durduruluyor..." -ForegroundColor Cyan
$svc = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($null -ne $svc -and $svc.Status -ne "Stopped") {
    Stop-Service -Name $ServiceName -Force
    Start-Sleep -Seconds 3
}

if (Test-Path $uploads) {
    Copy-Item -Path $uploads -Destination $yedek -Recurse -Force
    $sayi = (Get-ChildItem $yedek -Recurse -File).Count
    Write-Host "Gorseller yedeklendi ($sayi dosya): $yedek" -ForegroundColor Green
} else {
    Write-Host "uploads klasoru yok, yedeklenecek gorsel bulunamadi." -ForegroundColor Yellow
}

Write-Host "Derleniyor ve yayinlaniyor..." -ForegroundColor Cyan
Push-Location $RepoPath
try {
    & dotnet publish Portfolio -c Release -o $AppPath
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish basarisiz (cikis kodu $LASTEXITCODE)" }
}
finally { Pop-Location }

# Publish ciktiyi temizlemis olabilir: eksik gorselleri geri koy.
if (Test-Path $yedek) {
    New-Item -ItemType Directory -Force -Path $uploads | Out-Null
    Copy-Item -Path (Join-Path $yedek "*") -Destination $uploads -Recurse -Force
    $sayi = (Get-ChildItem $uploads -Recurse -File).Count
    Write-Host "Gorseller yerine kondu ($sayi dosya)" -ForegroundColor Green
}

Write-Host "Servis baslatiliyor..." -ForegroundColor Cyan
Start-Service -Name $ServiceName

$deadline = (Get-Date).AddSeconds(45)
$ok = $false
while ((Get-Date) -lt $deadline) {
    try {
        $r = Invoke-WebRequest -Uri "http://localhost:$Port/" -UseBasicParsing -TimeoutSec 5
        if ($r.StatusCode -eq 200) { $ok = $true; break }
    } catch { Start-Sleep -Seconds 2 }
}

if ($ok) {
    Write-Host ""
    Write-Host "Yayinlandi. Site yanit veriyor: http://localhost:$Port/" -ForegroundColor Green
    Write-Host "Gorsel yedegi $yedek klasorunde duruyor; her sey yolundaysa silebilirsin."
} else {
    Write-Host "Site 45 saniyede yanit vermedi. Loglar: $AppPath\logs\" -ForegroundColor Red
    Write-Host "Gorsel yedegi: $yedek" -ForegroundColor Yellow
    exit 1
}
