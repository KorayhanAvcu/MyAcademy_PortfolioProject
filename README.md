# 🚀 Dynamic Portfolio Management System

### ASP.NET Core MVC · Admin Panel · Portfolio Management

**MyAcademy Portfolio Project**, ASP.NET Core MVC kullanılarak geliştirilmiş, kişisel portfolyo içeriklerinin dinamik olarak yönetilebildiği web tabanlı bir portfolio management uygulamasıdır.

Proje; ziyaretçilere modern ve dinamik bir portfolyo deneyimi sunarken, yönetici tarafında portfolio içerisindeki içeriklerin merkezi bir admin panel üzerinden yönetilmesini sağlar.

Admin panelinde yapılan değişiklikler doğrudan kullanıcı tarafındaki portfolio sayfasına yansıtılmaktadır.

---

## 📌 Project Overview

Bu proje, statik bir portfolio sayfası yerine içeriklerin veritabanından yönetildiği **dinamik bir portfolio management system** oluşturmak amacıyla geliştirilmiştir.

Sistem iki temel bölümden oluşmaktadır:

```text
                 Portfolio System
                       │
          ┌────────────┴────────────┐
          │                         │
          ▼                         ▼
   Public Portfolio            Admin Panel
          │                         │
          │                         ├── Dashboard
          │                         ├── Projects
          │                         ├── Experiences
          │                         ├── Skills
          │                         ├── Services
          │                         ├── Education
          │                         ├── Testimonials
          │                         └── Messages
          │
          ▼
     Visitors
```

---

## ✨ Features

### 🌐 Dynamic Portfolio

Ziyaretçiler portfolio içerisinde dinamik olarak oluşturulan içerikleri görüntüleyebilir.

Portfolio içerisinde:

* Hero / Banner
* About
* Services
* Projects
* Project Technologies
* Skills
* Experiences
* Education
* Testimonials
* Contact Information

gibi bölümler bulunmaktadır.

İçerikler doğrudan veritabanından alınarak kullanıcı arayüzünde gösterilmektedir.

---

### 🔐 Authentication

Admin paneline erişim **Cookie Authentication** kullanılarak güvence altına alınmıştır.

Authentication sistemi:

* Login
* Logout
* Session management
* Authentication cookie
* Authorization
* Access control
* Sliding expiration

özelliklerini kullanmaktadır.

Uygulamada `PortfolioCookie` isimli authentication cookie yapılandırılmış ve login/logout route'ları `AuthController` üzerinden yönetilmektedir.

---

### 🛡️ Authorization

Admin panelindeki controller işlemleri authorization mekanizması üzerinden korunmaktadır.

Uygulama genelinde:

```csharp
options.Filters.Add(new AuthorizeFilter());
```

kullanılarak controller action'larının varsayılan olarak yetkilendirme kontrolünden geçmesi sağlanmıştır.

Bu yapı sayesinde yetkisiz kullanıcıların yönetim paneline erişmesi engellenmektedir.

---

## 🖥️ Admin Panel

Admin paneli, portfolio içerisindeki tüm içeriklerin merkezi olarak yönetilebilmesini sağlar.

### 📊 Dashboard

Dashboard üzerinden sistem hakkında özet bilgiler görüntülenebilir.

Dashboard içerisinde:

* Total Projects
* Total Experiences
* Total Skills
* Total Testimonials
* Total Services
* Incoming Messages
* Unread Messages
* Monthly Message Statistics
* Latest Projects
* Latest Messages

gibi bilgiler gösterilmektedir.

Bu sayede yönetici, sistemin genel durumunu tek bir ekran üzerinden takip edebilir.

---

## 📦 Content Management

Admin panelinde birçok farklı içerik modülü bulunmaktadır.

### 👤 About

Kişisel tanıtım ve hakkımda bilgilerinin yönetilmesini sağlar.

### 🖼️ Banner

Portfolio ana sayfasındaki banner / hero alanının yönetilmesini sağlar.

### 💼 Experiences

Profesyonel deneyim bilgilerinin eklenmesi, güncellenmesi ve silinmesini sağlar.

### 🎓 Education

Eğitim bilgilerinin yönetilmesini sağlar.

### 🛠️ Skills

Teknik yeteneklerin portfolio üzerinde yönetilmesini sağlar.

### 💻 Projects

Portfolio içerisinde gösterilecek projelerin yönetilmesini sağlar.

### ⚙️ Project Technologies

Projelerde kullanılan teknolojilerin ayrı bir yapı üzerinden yönetilmesini sağlar.

### 🧩 Services

Sunulan hizmetlerin yönetilmesini sağlar.

### ⭐ Testimonials

Referans / testimonial bilgilerinin yönetilmesini sağlar.

### 📞 Contact Information

Portfolio üzerindeki iletişim bilgilerinin yönetilmesini sağlar.

### 🗂️ Categories

Projelerin veya içeriklerin kategorilendirilmesine yönelik yapı sağlar.

---

## 📩 Contact & Messaging

Portfolio üzerinde ziyaretçilerin iletişim formu aracılığıyla mesaj göndermesine olanak sağlayan bir iletişim sistemi bulunmaktadır.

Mesajlar admin paneli üzerinden yönetilebilir.

Sistem:

* Message creation
* Message listing
* Message details
* Read / unread status
* Message management

işlemlerini desteklemektedir.

Bir mesajın detayına girildiğinde mesajın otomatik olarak **read** durumuna geçirilmesi sağlanmaktadır.

---

## 🔄 CRUD Operations

Admin panelindeki içerik modülleri temel CRUD işlemlerini desteklemektedir.

```text
Create
  ↓
Read
  ↓
Update
  ↓
Delete
```

Desteklenen başlıca modüller:

| Module               | CRUD |
| -------------------- | :--: |
| Banner               |   ✅  |
| About                |   ✅  |
| Contact Information  |   ✅  |
| Services             |   ✅  |
| Projects             |   ✅  |
| Project Technologies |   ✅  |
| Categories           |   ✅  |
| Skills               |   ✅  |
| Experiences          |   ✅  |
| Education            |   ✅  |
| Testimonials         |   ✅  |
| User Messages        |   ✅  |

---

## 🏗️ Architecture

Proje **ASP.NET Core MVC** mimarisi kullanılarak geliştirilmiştir.

Genel proje yapısı:

```text
MyAcademy_PortfolioProject
│
├── Portfolio
│   │
│   ├── Controllers
│   │   ├── AboutController
│   │   ├── AdminLayoutController
│   │   ├── AuthController
│   │   ├── BannerController
│   │   ├── CategoryController
│   │   ├── ContactInfoController
│   │   ├── DashboardController
│   │   ├── DefaultController
│   │   ├── EducationController
│   │   ├── ExperienceController
│   │   ├── HomeController
│   │   ├── ProjectController
│   │   ├── ProjectTechStacksController
│   │   ├── ServiceController
│   │   ├── SkillController
│   │   ├── TestimonialController
│   │   └── UserMessageController
│   │
│   ├── Data
│   ├── Migrations
│   ├── Models
│   ├── ViewComponents
│   ├── Views
│   ├── wwwroot
│   │
│   ├── Program.cs
│   ├── appsettings.json
│   └── Portfolio.csproj
│
└── MyAcademy_PortfolioProject.sln
```

---

## 🧩 ViewComponents

Projede **ViewComponent** yapısından da yararlanılmıştır.

ViewComponents sayesinde tekrar kullanılabilir UI parçalarının controller'lardan ayrılması ve ilgili verilerin kendi component'leri üzerinden hazırlanması sağlanmaktadır.

Bu yaklaşım özellikle:

* Navbar
* Banner
* About
* Services
* Projects
* Skills
* Experiences
* Testimonials
* Footer

gibi tekrar kullanılabilir portfolio bölümlerinin yönetiminde faydalı olmaktadır.

---

## 🗄️ Database

Proje **Microsoft SQL Server** ve **Entity Framework Core** kullanmaktadır.

Database işlemleri:

```text
ASP.NET Core MVC
       │
       ▼
Entity Framework Core
       │
       ▼
AppDbContext
       │
       ▼
SQL Server
```

Database migration'ları da proje içerisinde bulunmaktadır.

---

## 🔄 Application Flow

### Public Portfolio

```text
Visitor
   │
   ▼
Portfolio Homepage
   │
   ├── About
   ├── Services
   ├── Projects
   ├── Skills
   ├── Experiences
   ├── Education
   ├── Testimonials
   └── Contact
```

---

### Admin Flow

```text
Admin
 │
 ▼
Login
 │
 ▼
Authentication
 │
 ▼
Dashboard
 │
 ├── Manage Portfolio
 ├── Manage Projects
 ├── Manage Skills
 ├── Manage Experiences
 ├── Manage Education
 ├── Manage Services
 ├── Manage Testimonials
 ├── Manage Categories
 └── Manage Messages
```

---

### Contact Flow

```text
Visitor
   │
   ▼
Contact Form
   │
   ▼
User Message
   │
   ▼
Database
   │
   ▼
Admin Panel
   │
   ▼
Message Details
   │
   ▼
Marked as Read
```

---

## 🔐 Session Management

Uygulamada ASP.NET Core Session kullanılmaktadır.

Session yapısı kullanıcı oturum bilgilerinin yönetilmesi ve uygulama içerisindeki geçici kullanıcı verilerinin saklanması için kullanılmaktadır.

Authentication cookie için sliding expiration mekanizması kullanılarak kullanıcı oturumlarının yönetilmesi sağlanmaktadır.

---

## 📦 Main Components

| Component                     | Responsibility                    |
| ----------------------------- | --------------------------------- |
| `AuthController`              | Admin authentication              |
| `DashboardController`         | Dashboard statistics and overview |
| `DefaultController`           | Public portfolio operations       |
| `ProjectController`           | Project management                |
| `ProjectTechStacksController` | Project technology management     |
| `ExperienceController`        | Experience management             |
| `EducationController`         | Education management              |
| `SkillController`             | Skill management                  |
| `ServiceController`           | Service management                |
| `TestimonialController`       | Testimonial management            |
| `BannerController`            | Banner management                 |
| `AboutController`             | About content management          |
| `CategoryController`          | Category management               |
| `ContactInfoController`       | Contact information               |
| `UserMessageController`       | Visitor messages                  |
| `ViewComponents`              | Reusable dynamic UI components    |
| `AppDbContext`                | Database operations               |

---

## 🧠 What I Learned

This project provided practical experience with:

* ASP.NET Core MVC
* .NET 8
* Entity Framework Core
* SQL Server
* LINQ
* Razor View Engine
* Cookie Authentication
* Authorization
* Session Management
* CRUD operations
* Database Migrations
* ViewComponents
* Dynamic content management
* Admin dashboard development
* MVC architecture
* Responsive UI development
* Portfolio management systems
* Contact / messaging systems

---

## 🎯 Project Goals

The main goal of this project was to move from a static portfolio website toward a **fully dynamic, database-driven portfolio management system**.

The architecture can be summarized as:

```text
Static Portfolio
      ↓
Dynamic Content
      ↓
Database Integration
      ↓
Admin Panel
      ↓
Authentication
      ↓
CRUD Management
      ↓
Complete Portfolio Management System
```

---

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/KorayhanAvcu/MyAcademy_PortfolioProject.git
```

### 2. Open the Solution

Open:

```text
MyAcademy_PortfolioProject.sln
```

using Visual Studio.

### 3. Configure Database

Update the database connection string inside:

```text
Portfolio/appsettings.json
```

with your local SQL Server configuration.

### 4. Apply Migrations

Using Package Manager Console:

```powershell
Update-Database
```

or using the .NET CLI:

```bash
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run
```

The application can then be accessed through the local ASP.NET Core development server.

---

## ⚠️ Configuration

Do not commit sensitive information such as:

* Production database credentials
* Passwords
* API keys
* Authentication secrets

to the repository.

Use environment-specific configuration for production deployments.

---

## 👨‍💻 Author

**Korayhan Avcu**

Computer Engineer · Software Engineer

Focused on:

`C#` · `.NET` · `ASP.NET Core` · `Backend Development` · `Software Architecture`

---

## 📫 Connect With Me

<div align="center">

<a href="https://github.com/KorayhanAvcu">
<img src="https://img.shields.io/badge/GitHub-KorayhanAvcu-181717?style=for-the-badge&logo=github&logoColor=white">
</a>

<a href="https://www.linkedin.com/in/korayhan-avcu-094bb41ab/">
<img src="https://img.shields.io/badge/LinkedIn-Korayhan_Avcu-0A66C2?style=for-the-badge&logo=linkedin&logoColor=white">
</a>

</div>

---

<div align="center">

### Building systems. Solving problems. Learning continuously.

**C# · ASP.NET Core · MVC · SQL Server · Software Architecture**

</div>
