# Identity Geçişi, Güvenlik Düzeltmeleri ve Sıfır Maliyetli Canlı Yayın

Tarih: 2026-11-24
Proje: MyAcademy_PortfolioProject (.NET 8 MVC, EF Core, SQL Server Express)

## 1. Niyet ve neden

Proje bir kişisel portfolyo sitesi: public tarafta tek sayfalık tanıtım
(banner, hakkında, deneyim, eğitim, hizmetler, projeler, referanslar,
iletişim formu), arka tarafta bu içeriği yöneten bir admin paneli.
İskelet çalışıyor ama proje hiç canlıya çıkmamış durumda ve canlıya
çıkışı engelleyen somut eksikleri var.

Amaç iki şey: (a) projeyi gerçekten internette erişilebilir hale
getirmek, (b) bunu yaparken onu "kurs projesi" seviyesinden
"gösterilebilir iş" seviyesine çıkarmak. İkincisi önemli, çünkü sitenin
kendisi bir portfolyo — kodunun kalitesi de gösterilen şeyin parçası.

Maliyet sıfır olacak ve veritabanı kullanıcının kendi cihazında kalacak.
Bu iki kısıt kullanıcı tarafından açıkça koyuldu.

## 2. Hedefler

1. Admin kimlik doğrulaması ASP.NET Core Identity'ye (`UserManager` /
   `SignInManager`) taşınacak; şifreler hash'lenmiş saklanacak.
2. Sistemde **tek bir** admin olacak: `korayhanavcu@gmail.com`. Bu
   kullanıcı uygulama ilk ayağa kalktığında otomatik oluşturulacak;
   başka kullanıcı oluşturma yolu (kayıt sayfası, davet, API) hiç
   bulunmayacak.
3. Canlıya çıkışı engelleyen dört kritik sorun kapatılacak (bkz. 6.2).
4. Projenin bir test paketi olacak; kritik akışlar (giriş, yetki, 
   iletişim formu) otomatik doğrulanacak.
5. Site, kullanıcının kendi makinesinden, sıfır aylık maliyetle, HTTPS
   üzerinden internete açılacak.
6. Admin panelinden görsel dosyası yüklenebilecek; site dışarıdaki
   geçici URL'lere bağımlı olmaktan çıkacak. (Kapsama sonradan eklendi
   — bkz. 7.10.)
7. Yayına çıkmadan önce içerik kontrolünden geçilecek: şablondan kalan
   uydurma referanslar kaldırılacak, görseller projeye taşınacak,
   başlık/meta metinleri gerçek değerlerle doldurulacak (bkz. 10).

## 3. Hedef olmayanlar

Bu iş kapsamında **yapılmayacak** olanlar — ihtiyaç duyulursa sonraki
işler:

- Çok kullanıcılı yönetim, rol yönetimi arayüzü, kullanıcı listesi.
- E-posta gönderimi: şifre sıfırlama maili, iletişim formu bildirimi,
  e-posta doğrulama. (Sıfır maliyet kısıtı ve tek kullanıcı olması
  nedeniyle gerek yok; gelen mesajlar admin panelindeki
  `UserMessage` listesinden okunacak.)
- İki faktörlü doğrulama (2FA), harici giriş (Google/GitHub ile giriş).
- ~~Görsel dosya yükleme.~~ **Bu madde kapsama alındı** — mevcut
  görsellerin tamamı ölmekte olan üçüncü taraf URL'lerine bağlı olduğu
  ortaya çıktı ve kullanıcı kalıcı çözümü seçti. Bkz. 7.10.
- Tasarım/UX yenilemesi, yeni sayfa veya yeni içerik tipi.
- Çoklu dil desteği.
- Yüksek erişilebilirlik / yük dengeleme. Tek örnek (single instance)
  çalışacak.

## 4. Anti-hedefler

Teknik olarak "çalışsa" bile bu işi başarısız sayacağımız durumlar:

1. **Şifrenin repoda bulunması.** Hiçbir şifre, connection string ya da
   gizli değer git'e commit edilmeyecek; `appsettings.json` dahil.
2. **Admin panelinin bir an için anonim erişilebilir olması.** Yetki
   varsayılan olarak kapalı olacak; bir controller'ın yetki ekini
   yazmayı unutmak paneli açmamalı.
3. **Mevcut verinin kaybı.** Veritabanındaki portfolyo içeriği
   (projeler, deneyimler, referanslar, gelen mesajlar) geçiş sırasında
   korunacak. Migration eklemeli (additive) olacak; `Admins` tablosu
   dışında tablo düşürülmeyecek.
4. **Public tarafın görsel olarak bozulması.** Site şu an nasıl
   görünüyorsa, iş bittiğinde de öyle görünecek.

## 5. Kısıtlar

- **Aylık maliyet: 0.** Ücretli barındırma, ücretli veritabanı, ücretli
  e-posta servisi yok.
- **Veritabanı kullanıcının kendi cihazında.** Mevcut SQL Server Express
  (`KORAYHAN\SQLEXPRESS`) kalıyor. Bu, EF sağlayıcısının SQL Server
  olarak kalması anlamına da geliyor — PostgreSQL'e geçiş ve 7
  migration'ı yeniden üretme işi gündemden çıktı.
- **Uygulama da aynı cihazda.** Yani site, cihaz açık ve tünel çalışır
  durumdayken ayakta. Bu bilinen ve kabul edilmiş bir bedel.
- Hedef framework `net8.0` (makinede .NET 10 SDK kurulu, `net8.0`
  hedefi sorunsuz derleniyor). Framework yükseltmesi bu işin kapsamında
  değil.
- Platform Windows.

## 6. Yaklaşımlar

### 6.1 Barındırma: değerlendirilen üç yol

**A. Azure App Service + Azure SQL.** Veri katmanına hiç dokunmadan
çıkılır, mevcut migration'lar çalışır, GitHub Actions ile gerçek bir
CI/CD hattı kurulur. Bedeli: ücretsiz F1 planında uygulama uykuya dalar,
kendi alan adını SSL ile bağlamak için ~$13/ay Basic plan gerekir,
kredi kartı doğrulaması ister. **Elendi:** sıfır maliyet kısıtını ve
"veritabanı kendi cihazımda" kısıtını ihlal ediyor.

**B. VPS + Docker Compose (PostgreSQL).** ~5€/ay sabit maliyet, uyku yok,
tam kontrol. Bedeli: EF sağlayıcısını Npgsql'e çevirmek, 7 migration'ı
sıfırdan üretmek, sunucu bakımını üstlenmek. **Elendi:** maliyet sıfır
değil ve veritabanı kullanıcının cihazından çıkıyor.

**C. Türk paylaşımlı hosting (Plesk, .NET 8 + MSSQL).** Ayda ~100-150 TL,
kod değişmez, en az uğraş. Bedeli: CI/CD yok, deploy elle, teşhis
kısıtlı. **Elendi:** maliyet sıfır değil, veritabanı kullanıcının
cihazından çıkıyor.

### 6.2 Seçilen yol: kendi makinesinde barındırma + dışa açan tünel

Uygulama ve SQL Server Express kullanıcının makinesinde kalıyor.
Makineden dışarıya doğru kurulan bir tünel (Cloudflare Tunnel /
`cloudflared`) gelen ziyaretçileri `localhost`'taki Kestrel'e taşıyor.

Bu yolun seçilme nedeni, iki kısıtı birlikte karşılayan tek yol
olması. Yan faydaları:

- **Router'da port açılmıyor.** Bağlantı içten dışa kurulduğu için ev
  ağında dinleyen bir port oluşmuyor.
- **SQL Server internete hiç açılmıyor.** Tünel yalnızca uygulamanın
  HTTP portunu yayınlıyor; veritabanı `localhost`'ta kalıyor.
- **TLS sertifikası tünel sağlayıcısı tarafından yönetiliyor**, ücretsiz.

Reddedilen bir dördüncü varyant: uygulamayı ücretsiz bir bulut katmanına
koyup veritabanını evde bırakmak. Bu, bulutun SQL Server'a ulaşabilmesi
için veritabanını internete açmayı gerektirir — hem güvenlik açısından
kötü hem her sorguda ev-bulut gidiş dönüşü eklenir. Her şeyin aynı
makinede olmasından daha kötü, karşılığında hiçbir kazanç yok.

**Kalan alt karar — kalıcı adres.** Araştırma şunu gösterdi: Cloudflare'in
kalıcı isimli tüneli, DNS'i kullanıcının kendi Cloudflare hesabında olan
bir alan adı gerektiriyor; `cfargotunnel.com` adresleri yalnızca aynı
hesaptaki DNS kayıtlarıyla çalışıyor. Bu, `is-a.dev` gibi ücretsiz
subdomain servislerinden CNAME çekme kısayolunu kapatıyor. Hesapsız
kullanılan hızlı tünel (`trycloudflare.com`) ücretsiz ama adresi her
yeniden başlatmada değişiyor ve uptime garantisi yok — CV'ye yazılacak
bir adres değil. İki rota kalıyor:

- ~1-3 USD/yıl bir alan adı alıp Cloudflare'e bağlamak (teknik olarak en
  temiz, ama tam olarak "sıfır" değil),
- Tailscale Funnel gibi alan adı istemeyen, sabit `*.ts.net` adresi veren
  ücretsiz bir alternatif.

Bu karar **kodu etkilemiyor** (bkz. 7.6) ve deploy adımına kadar
bekletilebilir. O adıma gelindiğinde ikinci rotanın güncel durumu
doğrulanıp kullanıcıya kesin bilgiyle sunulacak.

### 6.3 Kimlik doğrulama: değerlendirilen iki yol

**A. Mevcut özel çözümü koruyup sadece şifreyi hash'lemek.** `Admin`
tablosu ve `AuthController` kalır, `Password` alanı
`PasswordHasher<T>` ile hash'lenir. En az iş, en az risk. Bedeli: hesap
kilitleme, şifre değiştirme, token üretimi gibi her şey elle yazılır;
yazılmazsa eksik kalır.

**B. ASP.NET Core Identity'ye geçmek.** `UserManager` / `SignInManager`,
hash'leme, şifre politikası, hesap kilitleme hazır gelir.

**Seçilen: B.** Kullanıcı açıkça `UserManager` istedi. Ek olarak, bu
proje bir portfolyo olduğu için ekosistemin standart çözümünü kullanmak
gösterilecek iş açısından da daha iyi.

### 6.4 Veritabanı sağlayıcısı: SQL Server mı, PostgreSQL mi

PostgreSQL'e geçiş değerlendirildi ve **reddedildi**; SQL Server Express
kalıyor.

Önce bir yanlış varsayımın düzeltilmesi: bu bir maliyet kararı değil.
PostgreSQL ücretsiz, ama SQL Server Express de ücretsiz. Express'in
sınırları (veritabanı başına 10 GB, ~1,4 GB RAM, 4 çekirdek) bu proje
için anlamsız — mevcut veri birkaç megabayt.

**PostgreSQL'in artıları:** Bol ücretsiz bulut katmanı var (Neon,
Supabase gibi), yani ileride "makine kapalıyken site kapalı"
sorunundan vazgeçilmek istenirse veritabanı para ödemeden buluta
taşınabilir — SQL Server'da bu seçenek neredeyse yok. Linux'ta
çalıştığı için ucuz VPS/Docker yolu da açık kalır.

**Reddetme gerekçesi:** Geçişin bedeli para değil, iş — ve bu bedel
*şimdi* ödenirse boşa gidebilir:

- Veritabanında gerçek içerik var: 2 proje, 18 yetenek, 8 referans, 2
  deneyim, 2 eğitim, 3 hizmet, 1 banner, 1 hakkında kaydı ve 133
  ziyaretçi mesajı. Geçiş, 7 migration'ı sıfırdan üretmenin yanında bu
  verinin taşınmasını da gerektirir (bkz. anti-hedef 3).
- Identity geçişiyle sağlayıcı değişikliğini üst üste bindirmek, bir şey
  bozulduğunda nedeni ayırt etmeyi zorlaştırır.
- Geçiş **sonra da ucuz**: projede tek bir raw SQL ya da sağlayıcıya
  özel tip eşlemesi yok (tarandı). Yani kapı kapanmıyor.

**Sonraki geçiş için not edilmesi gereken tuzak:** Npgsql `DateTime`'ı
`timestamptz` olarak eşler ve `DateTime.Now` (yerel saatli) değer
yazmayı reddeder. Projede üç yerde `DateTime.Now` var:
`UserMessage.CreatedDate` varsayılanı ve `DashboardController` içinde
iki kullanım. Postgres'e geçilecekse bunlar `DateTime.UtcNow`'a
çevrilmeli.

## 7. Kararlar

Bu bölümdeki her madde, kullanıcıyla konuşulmuş olanlar dışında,
tasarımı yapanın kararıdır ve itiraz halinde değiştirilebilir.

### 7.1 Identity modeli

- `AppUser : IdentityUser` sınıfı eklenecek; `FullName` ve `ImageUrl`
  özellikleri bu sınıfa taşınacak (mevcut `Admin` entity'sinin taşıdığı
  bilgiler).
- `AppDbContext`, `DbContext` yerine `IdentityDbContext<AppUser,
  IdentityRole, string>` türetecek. Mevcut 14 `DbSet` olduğu gibi
  kalacak.
- `Admin` entity'si ve `Admins` DbSet'i kaldırılacak. Bu tabloda
  gösterilecek bir içerik yok, yalnızca giriş bilgisi var; Identity
  tabloları onun yerini alıyor.
- Rol: tek bir `Admin` rolü seed edilecek ve kullanıcı bu role
  eklenecek.

**Karar — `fullname`/`imageUrl` nasıl taşınacak:** Şu an bu iki değer
hem claim'e hem `Session`'a yazılıyor, sidebar ise `Session`'dan
okuyor — ve `"fullName"`/`"fullname"` harf farkı yüzünden hep boş
geliyor. Çözüm: özel bir `IUserClaimsPrincipalFactory<AppUser>` bu iki
değeri claim olarak ekleyecek, sidebar `User.FindFirst(...)` ile
okuyacak. Bunun sonucu olarak **`AddSession` tamamen kaldırılacak** —
session'ın projede başka kullanımı yok.

### 7.2 Tek admin ve ilk kullanıcı

- **Kayıt yolu yok.** Register controller/action/view eklenmeyecek.
  `UserManager.CreateAsync` yalnızca seeder içinden çağrılacak.
- **Seed:** Uygulama ayağa kalkarken, hiç kullanıcı yoksa
  `korayhanavcu@gmail.com` oluşturulacak ve `Admin` rolüne eklenecek.
  Kullanıcı varsa seeder hiçbir şey yapmayacak (idempotent).
- **Şifre kaynağı:** `ADMIN_SEED_PASSWORD` environment variable'ından
  okunacak. Değişken yoksa uygulama **açık bir hata mesajıyla
  başlamayı reddedecek** — varsayılan şifreye düşmeyecek. Bu, 4.
  maddedeki anti-hedefin teknik karşılığı.
- **Şifre politikası:** en az 10 karakter, rakam + büyük harf + küçük
  harf zorunlu, özel karakter zorunlu değil, benzersiz karakter şartı
  yok.
- **Hesap kilitleme:** 5 başarısız denemeden sonra 15 dakika kilit.
  `PasswordSignInAsync(..., lockoutOnFailure: true)` ile. Bu ayarın
  tek hesaplı bir sistemde yarattığı risk 7.3'te ele alındı.

**Karar — şifrenin doğru kaynağı veritabanıdır, environment variable
değil.** Seeder yalnızca "hiç kullanıcı yok" durumunda çalışır ve
kullanıcıyı oluşturur; sonrasında şifreyi asla geri yazmaz. Bunun
sonuçları:

- `ADMIN_SEED_PASSWORD` yalnızca ilk açılışta zorunludur. Başlangıç
  kontrolü "kullanıcı yok **ve** değişken yok" durumunda hata verir;
  kullanıcı varsa değişkenin varlığı aranmaz. Böylece artık geçerli
  olmayan bir şifreyi sonsuza kadar bir değişkende tutmak gerekmez.
- Panelden şifre değiştirmek kalıcıdır (bir sonraki açılışta
  ezilmez). "Şifre Değiştir" sayfası anlamlı kalır.
- **Kurtarma yolu:** Şifre sıfırlama e-postası kapsam dışı (bkz. 3) ve
  kayıt yolu yok, yani unutulan şifrenin kendiliğinden kurtulma yolu da
  yok. Prosedür README'ye yazılacak: `ADMIN_SEED_PASSWORD`'ü yeni
  değerle set et, `AspNetUsers` tablosundaki satırı sil, uygulamayı
  başlat — seeder kullanıcıyı yeniden oluşturur. İçerik etkilenmez,
  çünkü hiçbir tablo admin kullanıcısına bağlı değil.

**Karar — ilk girişte şifre değiştirme zorunluluğu kaldırıldı.**
Konuşmanın başında "ilk girişte değiştirilsin" denmişti; şifreyi
kullanıcının kendisi environment variable ile koyduğu için bu gereksiz
bir sürtünme. Yerine admin paneline bir **"Şifre Değiştir"** sayfası
eklenecek (`UserManager.ChangePasswordAsync`).

**Karar — giriş alanı e-posta olacak.** `LoginViewModel.UserName` yerine
`Email`; `Views/Auth/Login.cshtml` alanı buna göre güncellenecek.

### 7.3 Yetkilendirme

- Global filtre `new AuthorizeFilter()` yerine
  `[Authorize(Roles = "Admin")]` eşdeğeri bir global politika olacak.
  Böylece yalnızca "giriş yapmış olmak" yetmeyecek.
- `AuthController` ve `DefaultController` `[AllowAnonymous]` kalacak.
- `HomeController` `[AllowAnonymous]` olacak (bkz. 7.4, madde 2).

**Karar — admin paneli internete hiç açılmayacak (kullanıcı kararı).**
Sistemde tasarım gereği tek bir hesap var ve 5 denemede 15 dakika kilit
uygulanıyor. Bu ikisi birlikte, `/Auth/Login` adresini bulan herhangi
birinin on beş dakikada beş istekle tek admin hesabını süresiz kilitli
tutabilmesi anlamına gelir. Çözüm olarak kilit ayarını zayıflatmak
yerine panel dışarıya hiç yayınlanmıyor:

- Tünel yalnızca public tarafı yayınlayacak (bkz. 7.6).
- Admin yolları (`/Auth/*`, `/Dashboard`, ve tüm CRUD controller'ları)
  yalnızca `localhost`'tan gelen isteklere yanıt verecek; dışarıdan
  gelen istek 404 dönecek.
- Uygulama kullanıcının kendi makinesinde çalıştığı için bu, panele
  erişimini hiçbir şekilde kısıtlamıyor.

Yan fayda: dışarıdan giriş denemesi imkânsız hale geldiği için giriş
sayfasına ayrıca IP bazlı rate limit koymak gerekmiyor.

### 7.4 Dört kritik düzeltme

1. **Connection string koddan çıkacak.** `AppDbContext.OnConfiguring`
   silinecek; bağlantı `AddDbContext` içinde
   `Configuration.GetConnectionString("DefaultConnection")` ile
   verilecek. Geliştirme değeri `appsettings.Development.json`'da,
   canlı değer `ConnectionStrings__DefaultConnection` environment
   variable'ında. `appsettings.*.json` dosyalarının gizli değer taşıyan
   sürümleri `.gitignore`'a eklenecek.

2. **Hata sayfası erişilebilir olacak.** Şu an global yetki filtresi
   `HomeController`'ı da kapsadığı için canlıda bir hata oluştuğunda
   `/Home/Error`'a giden ziyaretçi login'e yönleniyor.
   `HomeController` `[AllowAnonymous]` olacak. Ayrıca
   `UseStatusCodePagesWithReExecute("/Home/Error/{0}")` eklenerek 404
   gibi durumlar da karşılanacak.

3. **CSRF koruması açılacak — ve 12 GET silme action'ı POST'a
   çevrilecek.**
   `options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())`
   ile tüm POST/PUT/DELETE istekleri antiforgery doğrulamasından
   geçecek. Razor formları token'ı zaten üretiyor; doğrulama eksikti.

   Ancak bu tek başına yetmiyor: projedeki **12 silme action'ının
   tamamı GET** ve düz bir link üzerinden çağrılıyor
   (`DeleteAbout`, `DeleteBanner`, `DeleteCategory`,
   `DeleteContactInfo`, `DeleteEducation`, `DeleteExperience`,
   `DeleteProject`, `DeleteService`, `DeleteSkill`,
   `DeleteTestimonial`, `DeleteUserMessage`,
   `ProjectTechStacks/Delete`). `AutoValidateAntiforgeryToken` yalnızca
   POST/PUT/DELETE metodlarını doğruladığı için bunlara hiç dokunmuyor.
   Yani filtre eklenmiş olsa bile açık açık kalır: giriş yapmış bir
   admin kötü niyetli bir sayfayı açtığında basit bir `<img src>`
   etiketi içerik silebilir.

   Bu yüzden 12 action `[HttpPost]` olacak ve ilgili admin index
   view'larındaki silme linkleri forma (ya da `fetch`'e) çevrilecek;
   mevcut SweetAlert onayları korunacak. Kullanıcı bu kapsam artışını
   bilerek onayladı.

4. **Tailwind CDN'den çıkarılacak.** Şu an public site ve login sayfası
   `cdn.tailwindcss.com` script'ini kullanıyor; bu üretim için
   tasarlanmamış. Tailwind CLI ile build edilip çıktı CSS'i
   `wwwroot/css/` altına yazılacak ve `<link>` ile bağlanacak. Dikkat:
   projede **üç ayrı renk paleti** var, iki değil — public koyu tema
   (`_DefaultHead`, `primary: #c0c1ff`), admin açık tema
   (`_AdminLayoutHead`, `primary: #3525cd`) ve login sayfasının kendi
   paleti (`Views/Auth/Login.cshtml`, `primary: #004ac6`). Token
   değerleri çakıştığı için tek bir stylesheet'te birleşemezler;
   **üç ayrı çıktı** üretilecek ve üçü de mevcut görünümle birebir aynı
   kalacak.

   Ayrıca `Views/AdminLayout/HeadPartial.cshtml` admin head'inin ölü
   bir kopyası — hiçbir view onu render etmiyor, yalnızca
   `AdminLayoutController.HeadPartial()` action'ı döndürüyor. Dosya ve
   action silinecek.

   **Karar — bu madde en sona bırakılacak.** Görsel regresyon riski en
   yüksek iş bu; diğer her şey çalıştıktan sonra yapılacak ki
   gerekirse canlıya çıkışı bloklamadan ertelenebilsin.

### 7.5 İkinci sıra düzeltmeler

- **Public klasördeki artıklar silinecek:** `wwwroot/stitch_portfolio.zip`,
  `wwwroot/stitch_modern_portfolio_admin_login.zip`,
  `wwwroot/AdminTemplate/code.html` ve açılmış şablon klasörleri. Bunlar
  şu an dışarıdan indirilebilir durumda.
- **Sayfa başlığı ve meta bilgileri:** `<title>` hâlâ
  "Alex Dev | Portfolio". Başlık, meta description, Open Graph etiketleri
  ve favicon gerçek bilgilerle doldurulacak.
- **İletişim formu kötüye kullanıma karşı:** gizli bir honeypot alanı +
  `AddRateLimiter` ile IP başına sabit pencereli sınır (öneri: 5
  dakikada 3 mesaj). E-posta bildirimi yok (bkz. 3).
- **Migration'ların canlıda uygulanması:** uygulama ayağa kalkarken
  `db.Database.Migrate()` çağrılacak. Tek örnek çalışan bir self-host
  senaryosunda bu kabul edilebilir ve elle adım gerektirmez.
- **Loglama:** Serilog + rolling file sink. Log klasörü `.gitignore`'a
  eklenecek. Canlıda hata teşhisi için gerekli, çünkü konsol çıktısı
  servis olarak çalışırken görünmüyor.
- **Admin layout'taki JS hatası:** `Layout.cshtml` içindeki
  `searchInput.addEventListener`, sayfada `input[type=text]` yoksa
  hata atıyor. Null kontrolü eklenecek.
- **Site adresi config'den gelecek:** `Site:BaseUrl` ayarı eklenecek;
  Open Graph ve canonical etiketleri bu değerden üretilecek. Böylece
  6.2'deki adres kararı kesinleştiğinde tek satır config değişiyor,
  kod değişmiyor.
- **Yedekleme.** Yayına çıkıldığında içeriğin tek kopyası, aynı zamanda
  web sunucusu olan tek bir makinede duruyor. Günlük zamanlanmış bir
  `BACKUP DATABASE` işi kurulacak ve çıktı ikinci bir diske (ya da
  kullanıcının seçeceği başka bir konuma) yazılacak. **Yedeğin
  `wwwroot/uploads` klasörünü de kapsaması şart** — yüklenen görseller
  veritabanında değil dosya sisteminde duruyor (bkz. 7.10).
- **Uydurma referanslar kaldırılacak.** Veritabanındaki 8 referans
  şablondan kalma; isim verilmiş ("Ahmet Aras / Product Manager,
  DataSync" gibi) ama gerçek değiller. Kullanıcı bunların silinmesini
  seçti. Referans bölümü, veri yoksa kendini gizleyecek şekilde
  ayarlanacak — boş bir bölüm render edilmeyecek.

### 7.6 Tünel arkasında çalışmak

- `UseForwardedHeaders` eklenecek (`XForwardedFor` + `XForwardedProto`),
  böylece uygulama isteğin gerçekte HTTPS geldiğini ve ziyaretçinin IP
  adresini görecek. Rate limiting'in doğru çalışması için bu şart.
- `UseHttpsRedirection` canlı profilde **kapatılacak.** TLS'i tünel
  sağlayıcısı sonlandırıyor; Kestrel'e istek HTTP olarak geliyor.
  Yönlendirme açık kalırsa sonsuz döngü oluşur.
- `AllowedHosts` `"*"` yerine gerçek alan adına ayarlanacak.
- Kestrel `http://localhost:5080` dinleyecek; dışarıdan erişilemeyecek.
- **Tünel yalnızca public tarafı yayınlayacak.** 7.3'teki karar gereği
  admin yolları dışarıya kapalı. İki katmanlı uygulanacak: tünel
  konfigürasyonunda yalnızca public yollar yayınlanacak, ayrıca
  uygulama tarafında admin controller'ları `localhost` dışından gelen
  isteklere 404 dönecek. İkinci katman, tünel konfigürasyonu yanlış
  kurulursa diye var.

Bu bölümdeki ayarlar, 6.2'deki adres kararından bağımsızdır — hangi
tünel seçilirse seçilsin aynı kalır.

### 7.7 Testler

Yeni proje: `Portfolio.Tests` (xUnit), solution'a eklenecek.

**Birim testleri — seeder mantığı:**
- Hiç kullanıcı yokken seeder kullanıcıyı oluşturuyor ve `Admin` rolüne
  ekliyor.
- Kullanıcı varken seeder hiçbir şey yapmıyor (ikinci çalıştırmada
  ikinci kullanıcı oluşmuyor).
- `ADMIN_SEED_PASSWORD` yokken uygulama hata ile duruyor.

**Entegrasyon testleri — `WebApplicationFactory<Program>`:**
- Anasayfa (`/`) anonim olarak 200 dönüyor.
- `/Dashboard` anonim olarak `/Auth/Login`'e yönleniyor.
- Doğru şifreyle giriş `/Dashboard`'a yönleniyor.
- 5 kez yanlış şifre sonrası hesap kilitleniyor; 6. denemede doğru
  şifre de kabul edilmiyor.
- Antiforgery token'ı olmayan POST isteği 400 dönüyor.
- İletişim formu geçerli veriyle `UserMessages`'a kayıt ekliyor;
  geçersiz veriyle eklemiyor.

**Karar — testler gerçek SQL Server Express kullanacak.** SQLite'a
geçmek farklı bir EF sağlayıcısı demek; mevcut migration'lar SQL
Server'a özel olduğu için testte `EnsureCreated`'a düşmek gerekirdi ve
test ettiğimiz şema canlıdakinden farklı olurdu. Bunun yerine testler
makinedeki SQL Express üzerinde ayrı bir `PortfolioDb_Test`
veritabanı oluşturup (migration ile), test sonunda düşürecek. Bedeli:
testler SQL Server kurulu olmayan bir makinede çalışmaz — bu README'de
yazılacak.

`Program.cs` top-level statement kullandığı için
`WebApplicationFactory`'nin görebilmesi adına `public partial class
Program` eklenecek.

### 7.8 Yayın adımları

1. `dotnet publish -c Release` ile çıktı bir klasöre alınacak.
2. Uygulama Windows servisi olarak kaydedilecek, böylece makine açıldığında
   kendiliğinden başlayacak.
3. Tünel istemcisi de Windows servisi olarak kurulacak.
4. Environment variable'lar (`ADMIN_SEED_PASSWORD`,
   `ConnectionStrings__DefaultConnection`, `ASPNETCORE_ENVIRONMENT=Production`)
   makine düzeyinde tanımlanacak.
5. İlk giriş yapılıp panelden şifre değiştirilecek.

**Karar — servis hesabı ve SQL Server erişimi.** Bu, deploy'u son
adımda tamamen durdurabilecek noktaydı; instance kontrol edildi:
`KORAYHAN\SQLEXPRESS` yalnızca Windows kimlik doğrulaması kabul ediyor
(`IsIntegratedSecurityOnly = 1`), yani SQL login'leri (`sa` dahil)
mixed mode'a geçip servisi yeniden başlatmadan kullanılamaz.
`NT AUTHORITY\SYSTEM` için sunucu login'i **var**, ancak sysadmin
değil ve `PortfolioDb` içinde kullanıcısı yok — sysadmin olanlar
yalnızca `sa`, `Korayhan\marve` ve SQL'in kendi servis hesapları.
Dolayısıyla varsayılan hesapla kurulan bir Windows servisi veritabanına
bağlanamaz; üstelik 7.5'teki başlangıç `Migrate()` çağrısı DDL yetkisi
de istiyor.

Üç yol değerlendirildi: (a) servisi `Korayhan\marve` olarak çalıştırmak
— o hesabın şifresi olmasını gerektirir ve servise sysadmin yetkisi
verir; (b) servis hesabına login/kullanıcı açıp yalnızca `PortfolioDb`
üzerinde yetki vermek; (c) SQL Server'ı mixed mode'a alıp connection
string'e şifre koymak.

**Seçilen: (b).** Servis `LocalSystem` olarak çalışacak ve
`NT AUTHORITY\SYSTEM` principal'ına yalnızca `PortfolioDb` üzerinde
`db_owner` verilecek. Gerekçe: (c) ortaya yönetilmesi gereken yeni bir
sır çıkarıyor ve 4. maddedeki anti-hedefe yaklaşıyor; (a) bir arka plan
servisine makine çapında sysadmin yetkisi veriyor. (b) ne sır
üretiyor ne sysadmin veriyor, ve yetkiyi tek veritabanına sınırlıyor.

### 7.9 CI/CD

Maliyeti sıfır ve kullanıcının kendisi kurabileceği bir hat kurulacak.
GitHub Actions public repolarda sınırsız ücretsiz; private repoda Free
planda ayda 2000 dakika — bu projenin build + test akışı için fazlasıyla
yeterli.

**CI — her push'ta derle ve test et.** `.github/workflows/ci.yml`
(şu an klasör boş duruyor): `dotnet restore`, `dotnet build`,
`dotnet test`. GitHub'ın kendi runner'ında çalışacak.

Burada bir kısıt var: 7.7'deki karara göre entegrasyon testleri gerçek
SQL Server istiyor. GitHub'ın Windows runner'ında SQL Express kurulu
değil. İki seçenek: (a) CI'da yalnızca build + birim testleri
koşturmak, entegrasyon testlerini lokalde bırakmak; (b) CI'ı
self-hosted runner'da koşturup makinedeki SQL Express'i kullanmak.
**Karar: (b)** — zaten self-hosted runner kurulacak (aşağı bakınız) ve
entegrasyon testlerinin her push'ta koşması asıl değeri sağlayan şey.

**CD — kendi makinesine deploy.** GitHub Actions self-hosted runner,
kullanıcının makinesine kurulacak: GitHub'dan işi çeker, lokalde
`dotnet publish` eder, Windows servisini yeniden başlatır. Ücreti yok.
Kendi makinesinde barındırma senaryosuna doğal olarak oturuyor ve
"CI/CD hattı bulut gerektirir" varsayımını ortadan kaldırıyor.

**Güvenlik — bu madde atlanmayacak.** Repo public ise, yabancı birinin
açtığı pull request varsayılan ayarlarda self-hosted runner üzerinde,
yani kullanıcının kendi makinesinde kod çalıştırabilir. İki önlem
birlikte uygulanacak:

- Workflow yalnızca `master`'a push'ta tetiklenecek
  (`pull_request` tetikleyicisi self-hosted runner'da kullanılmayacak).
- Repo ayarlarından dış katkıcıların workflow çalıştırması için onay
  zorunlu kılınacak.

### 7.10 Görsel yükleme

Bu bölüm kapsama sonradan eklendi. Nedeni: canlı veritabanındaki
görsellerin **tamamı** ölmekte olan üçüncü taraf URL'lerine bağlı —
banner bir Google AI Studio (`lh3.googleusercontent.com/aida-public/...`)
adresi, iki proje görseli Google Resimler thumbnail'i
(`encrypted-tbn0.gstatic.com`, hem geçici hem başkasına ait), profil
fotoğrafı imzalı bir LinkedIn URL'si. Site yayına girdikten günler
sonra görünür şekilde bozulurdu; bu da 4. anti-hedefin tam karşılığı.
Kullanıcı geçici çözüm yerine kalıcı olanı seçti.

**Kapsam:** `ImageUrl` taşıyan dört yer — `About`, `Banner`,
`Project` ve admin avatarı (`AppUser.ImageUrl`). `Service.Icon` bir
Material Symbols ikon adı, görsel değil; kapsam dışı.

**Kararlar:**

- **Alan korunuyor, anlamı genişliyor.** `ImageUrl` string kolonu
  olduğu gibi kalacak; yüklenen dosyalar ona `/uploads/...` biçiminde
  göreli yol yazacak. Böylece alan hem yüklenen dosyayı hem harici bir
  URL'yi kabul eder ve **veri taşıma gerekmez** (anti-hedef 3).
- **Form:** ilgili ViewModel'lere `IFormFile? ImageFile` eklenecek.
  Dosya seçilmişse yüklenir ve `ImageUrl` güncellenir; seçilmemişse
  mevcut değer korunur.
- **Depolama:** `wwwroot/uploads/<varlık>/<guid>.<uzantı>`. Klasör
  `.gitignore`'a eklenecek (içerik repoya girmeyecek) ama **yedeklemeye
  dahil edilecek** (bkz. 7.5).
- **Doğrulama:** izinli uzantılar `.jpg/.jpeg/.png/.webp`, azami 2 MB,
  content-type kontrolü **ve** dosyanın ilk baytlarından tür doğrulaması
  (uzantıya güvenilmeyecek). Dosya adı kullanıcıdan hiç alınmayacak,
  GUID ile yeniden adlandırılacak — yol enjeksiyonu ve üzerine yazma
  riskini kökten kaldırıyor.
- **Yeniden kodlama yapılmayacak.** ImageSharp gibi bir bağımlılık
  eklemek yerine doğrulama + yeniden adlandırma ile yetinilecek. Tek
  kişinin kendi görsellerini yüklediği bir panelde bu yeterli.
- **Değiştirilen dosya silinecek:** yeni görsel yüklendiğinde eski
  dosya diskten kaldırılacak, böylece klasör şişmeyecek.

**Testler:** izinsiz uzantı reddediliyor; 2 MB üstü reddediliyor;
uzantısı `.png` ama içeriği görsel olmayan dosya reddediliyor; geçerli
yükleme `ImageUrl`'i `/uploads/...` olarak güncelliyor ve dosyayı
diske yazıyor.

## 8. İş sırası

Sıra, riski sona bırakacak ve her adımın tek başına doğrulanabilir
olmasını sağlayacak şekilde kuruldu:

1. Config düzeltmeleri (7.4 madde 1) + `.gitignore`.
2. Test projesi iskeleti; anasayfa 200 testi (geçmesi gerek).
3. Identity geçişi + seeder + migration (7.1, 7.2); ilgili testler.
4. Yetkilendirme, hata sayfası, CSRF + 12 GET silme action'ının POST'a
   çevrilmesi (7.3, 7.4 madde 2-3); ilgili testler.
5. Görsel yükleme (7.10); ilgili testler.
6. İkinci sıra düzeltmeler (7.5) + tünel ayarları (7.6).
7. CI/CD hattı ve self-hosted runner (7.9).
8. Tailwind build'e geçiş (7.4 madde 4) — görsel karşılaştırmayla.
9. **İçerik kapısı** (bkz. 10): görseller yüklenmiş, uydurma
   referanslar kaldırılmış, başlık/meta metinleri yazılmış, adres
   kararı verilmiş olmalı. Bu adım geçilmeden 10. adıma geçilmeyecek.
10. Yayın (7.8).

Görsel yükleme (5), testleri olan bağımsız bir dikey olduğu için
Tailwind'den önce ve yetki işlerinden sonra duruyor: içerik kapısında
görsellerin projeye taşınabilmesi buna bağlı.

## 9. Uygulayana bırakılanlar

- Dosya/klasör yerleşimi, namespace düzeni, sınıf isimleri.
- Migration dosya isimleri.
- Serilog sink konfigürasyonunun ayrıntıları (dosya yolu, rolling
  aralığı, tutulma süresi).
- Test projesinin iç klasör yapısı ve yardımcı sınıfları (fixture,
  factory).
- Rate limiter'ın tam parametreleri (öneri 7.5'te, ayar serbest).
- "Şifre Değiştir" sayfasının görsel yerleşimi (mevcut admin layout'a
  uyacak şekilde).
- Admin yollarının `localhost` dışından 404 dönmesinin nasıl
  uygulanacağı (middleware mi, filtre mi).

## 10. Kullanıcıdan beklenen içerik

Teknik işler bunlara bağlı değil, ama **9. adımdaki içerik kapısı**
bunlar tamamlanmadan geçilemez:

| Ne | Durum |
|----|-------|
| Banner, 2 proje ve profil görselleri (yüklenecek dosyalar) | Bekliyor. Mevcut iki proje görseli Google Resimler'den alınmış, başkasına ait — bunların değişmesi gerekiyor. |
| Site başlığı (`<title>`), bir cümlelik açıklama, OG görseli | Bekliyor. Şu an "Alex Dev \| Portfolio" yazıyor. |
| `ContactInfos.Address` alanının yayınlanıp yayınlanmayacağı | Bekliyor. Alan dolu (24 karakter) ve public iletişim bölümünde render ediliyor. Karar verilmezse varsayılan olarak **yayınlanmayacak**. |
| Gerçek referanslar (varsa) | Karar verildi: şablondan kalanlar silinecek, yerine yeni veri konmayacak. |
| Alan adı rotası (ücretli domain mi, ücretsiz Funnel adresi mi) | Bekliyor (bkz. 6.2). 7. adıma kadar bekletilebilir. |
