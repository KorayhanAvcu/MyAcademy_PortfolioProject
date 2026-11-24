# Portfolyo Sitesi (ASP.NET Core 8 MVC)

Kisisel portfolyo sitesi ve yalnizca bu makineden erisilen yonetim paneli.

## Yerel calistirma

Gerekli ortam degiskenleri:

| Degisken | Anlami |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` ya da `Production` |
| `ConnectionStrings__DefaultConnection` | `PortfolioDb` baglanti cumlesi |
| `ADMIN_SEED_PASSWORD` | Ilk yonetici kullanicisinin sifresi (dosyaya yazmayin) |
| `AllowedHosts` | Kabul edilen alan adlari (ornek: `alanadim.com;localhost`) |
| `Site__BaseUrl` | Sitenin public adresi (ornek: `https://alanadim.com`) |

```powershell
dotnet run --project Portfolio
```

Veritabaninda hic kullanici yokken `ADMIN_SEED_PASSWORD` tanimli degilse uygulama **bilerek baslamaz**.

## Ilk yonetici

Ilk acilista hic kullanici yoksa uygulama `korayhanavcu@gmail.com` kullanicisini `ADMIN_SEED_PASSWORD` sifresiyle olusturur ve `Admin` rolune ekler. Kayit sayfasi bilerek yoktur.

Canli veritabaninda su an **sifir** kullanici var ve `Admin` rolu mevcut; `ADMIN_SEED_PASSWORD` tanimliyken ilk acilis yoneticiyi olusturur.

## Sifre kurtarma

Sifre sifirlama e-postasi bilerek yoktur. Prosedur:

1. `ADMIN_SEED_PASSWORD` degiskenini yeni sifreyle ayarlayin.
2. Yonetici kaydini silin (once `AspNetUserRoles`, sonra `AspNetUsers`):

```powershell
sqlcmd -S KORAYHAN\SQLEXPRESS -E -d PortfolioDb -Q "SET QUOTED_IDENTIFIER ON; DELETE FROM AspNetUserRoles; DELETE FROM AspNetUsers;"
```

   `SET QUOTED_IDENTIFIER ON;` sart: Identity tablolarinda filtreli indeksler var, onsuz SQL Server Msg 1934 ile reddeder.
3. Uygulamayi yeniden baslatin; yonetici yeni sifreyle yeniden olusur.

Icerik etkilenmez; hicbir tablo yonetici kullanicisina referans vermez.

## Yonetim paneli yalnizca localhost'tan erisilir

Panel ve giris sayfasi bilerek yalnizca loopback isteklerine cevap verir; baska her yerden **404** doner, boylece panelin varligi belli olmaz. Tunel yalnizca public siteyi yayinlamalidir.

**Sonuc: panele yalnizca makinenin kendisinden erisilir** (`http://localhost:5080/Auth/Login`).

## Yayin

1. `dotnet publish -c Release`
2. Ciktiyi `LocalSystem` hesabiyla calisan bir Windows servisi olarak kaydedin.
3. `NT AUTHORITY\SYSTEM` hesabina **yalnizca** `PortfolioDb` uzerinde `db_owner` verin. instance yalnizca Windows kimlik dogrulamasi kabul eder; `NT AUTHORITY\SYSTEM` icin bir login vardir ama sysadmin degildir ve bu veritabaninda kullanicisi yoktur. Sunucu rolu verilmez.
4. Kestrel `http://localhost:5080` adresinde dinler.
5. Ortam degiskenlerini makine duzeyinde tanimlayin (yukaridaki tablo).
6. Tuneli de servis olarak calistirin ve yalnizca public yollari yayinlayin (`/Auth/*` ve yonetim yollari yayinlanmaz).

### KRITIK: ara katman sirasi

`Program.cs` icinde `UseForwardedHeaders`, `UseLocalOnlyEndpoints()` cagrisindan **once** kalmak zorundadir. Sira degisirse tunelden gelen isteklerin gercek IP'si hic okunmaz, her istek uzaktan geliyormis gibi gorunur ve **panel localhost'tan bile 404 doner**; kendinizi disarida birakirsiniz.

## Yedekleme

```powershell
.\scripts\backup-db.ps1                        # varsayilan SQL Server Backup klasoru
.\scripts\backup-db.ps1 -Destination D:\Yedek  # ikinci disk
```

Script hem `PortfolioDb` hem `Portfolio/wwwroot/uploads/` klasorunu yedekler (yuklenen gorseller diskte durur, veritabaninda degil). SQL Server Express `WITH COMPRESSION` desteklemedigi icin sikistirma kullanilmaz. Her yedekten sonra `RESTORE VERIFYONLY` calisir; dogrulanamazsa script hata ile durur. `-Destination` klasorune SQL Server hizmet hesabi yazabilmelidir.

Sahibin yapacagi adimlar (scriptin disinda):

- Gunluk calismasi icin Windows Task Scheduler gorevi olusturun.
- Yedegi bir kez ayri bir adla (ornek `PortfolioDb_RestoreTest`) geri yukleyip tablo sayilarini karsilastirin, sonra dusurun. Canli veritabanina geri yukleme denemesi yapmayin. Denenmemis yedek yedek degildir.

## Testler

Testler bu makinede SQL Server Express gerektirir; baglanti cumlesi `KORAYHAN\SQLEXPRESS` adresine sabit yazilidir. `PortfolioDb_Test` veritabanini olusturur ve siler, `PortfolioDb`'ye asla dokunmaz.

```powershell
dotnet test
```

Her test sinifi `[Collection(PortfolioAppCollection.Name)]` tasimalidir; yoksa fabrika ikinci kez olusturulur ve sabit bir hata firlatir.

## Bilinen eksikler (kapsam disi birakildi, unutulmadi)

- Tailwind hala `cdn.tailwindcss.com` uzerinden yukleniyor; bu uretim icin onerilmez: konsolda uyari ve daha yavas ilk cizim.
- CI/CD yok; yayin elle yapilir. `.github/workflows` bos.
- Test kapsami bilerek ince: dort test (giris, kilitlenme, localhost kisitlamasi, antiforgery). Gorsel yukleme, iletisim formunun hiz siniri ve honeypot'u, seeder ve bos veri dayanikliligi elle dogrulandi, testle degil.
- Yonetici avatari (`AppUser.ImageUrl`) icin yukleme arayuzu yok; yonetici profil sayfasi yok.

## Yayindan once sahibin tamamlamasi gerekenler

- Banner, iki proje gorseli ve profil fotografi panelden yuklenmeli. Iki proje gorseli su an Google Resimler kucuk resimlerine isaret ediyor; bunlar hem gecici hem **baskasinin eseri**, mutlaka degistirilmeli.
- Site basligi, tek cumlelik aciklama ve Open Graph gorseli girilmeli.
- `ContactInfos.Address` yayinlansin mi karari verilmeli; su an public iletisim bolumunde gorunuyor.
- Sekiz sablon referans (uydurma isimlerle uydurma tavsiyeler) yayindan once silinmeli.
- Alan adi rotasi secilmeli: Cloudflare uzerinde ucuz bir alan adi ya da ucretsiz Tailscale Funnel adresi.

## Elle dogrulananlar (tekrar yapmaya gerek yok)

- Ana sayfa 200 donuyor ve icerik var.
- Bilinmeyen adresler ve olmayan statik dosyalar giris sayfasina yonlenmek yerine 404 donuyor.
- `/Dashboard` ve `/Auth/Login` loopback olmayan IP'den 404, localhost'tan calisiyor.
- Gercek formdan giris `/Dashboard`'a yonlendiriyor.
- Yonetim silme dugmelerinin POST zinciri bastan sona calisiyor (token sayfada, dogru uc nokta, satir siliniyor); tokensiz POST 400, silme uc noktasina GET 404 donuyor.
- Identity oncesi dogrulanmis yedek var: `C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\Backup\PortfolioDb_pre_identity_20261124.bak`.
