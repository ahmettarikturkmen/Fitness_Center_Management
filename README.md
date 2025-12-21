#  Fitness Center Management & AI Coach System

Bu proje, **Sakarya Üniversitesi Web Programlama** dersi kapsamında geliştirilmiş; spor salonlarının yönetimini, antrenör-üye etkileşimini ve yapay zeka destekli kişisel koçluk hizmetlerini dijitalleştiren kapsamlı bir web uygulamasıdır.

## Proje Hakkında
Bu sistem, spor salonu yöneticilerinin (Admin) şube, eğitmen ve hizmetleri yönetmesini sağlarken; üyelerin antrenörlerin müsaitlik durumuna göre çakışma kontrolü ile randevu almasına olanak tanır.

Ayrıca, **Google Gemini AI** entegrasyonu sayesinde üyelere kişisel fiziksel özelliklerine göre diyet/antrenman programı hazırlar ve **Pollinations.ai** ile motivasyonel hedef görselleri üretir.

## Kullanılan Teknolojiler

Bu proje **ASP.NET Core MVC** mimarisi ile geliştirilmiştir.

* **Backend:** C#, ASP.NET Core 8.0, Entity Framework Core (Code First)
* **Veritabanı:** PostgreSQL
* **Frontend:** HTML5, CSS3, Bootstrap 5 (Responsive), JavaScript, jQuery
* **AI Servisleri:** Google Gemini API (Metin Üretimi), Pollinations.ai (Görüntü Üretimi)
* **Güvenlik:** ASP.NET Core Identity (Role Based Authorization)
* **Diğer:** REST API, LINQ, AJAX

## Özellikler

### 1. Yapay Zeka (AI) Modülü
* **Kişisel Koç:** Kullanıcının boy, kilo, yaş, aktivite ve hedeflerine göre Gemini AI tarafından anlık diyet ve antrenman programı oluşturulur.
* **Görsel Motivasyon:** Kullanıcının hedeflediği vücut formuna ulaştığında nasıl görüneceğine dair yapay zeka tarafından görsel üretilir.

### 2. Gelişmiş Randevu Sistemi
* **Çakışma Kontrolü:** Sistem, seçilen hizmetin süresini hesaplar. Eğer eğitmenin o saat aralığında başka bir randevusu varsa veya mesaisi bitiyorsa randevuyu engeller.
* **Hizmet Bazlı Süre:** Her hizmetin (Pilates, PT, Yoga) süresi ve ücreti farklıdır.
* **Onay Mekanizması:** Randevular "Bekliyor", "Onaylandı" veya "Reddedildi" statülerinde yönetilir.

### 3. Yönetim (Admin) Paneli
* **CRUD İşlemleri:** Spor Salonu, Eğitmen ve Hizmetler için Ekle/Sil/Güncelle/Listele özellikleri.
* **Raporlama (REST API):** Admin panelinde tarih aralığına göre randevu bilgileri, REST API üzerinden LINQ sorguları ile filtrelenerek JSON formatında çekilir ve raporlanır.
* **Kullanıcı Yönetimi:** Kullanıcılara rol atama (Admin, Trainer, Member) ve yönetim işlemleri.

### 4. Kullanıcı Deneyimi (UX/UI)
* **Veri Doğrulama:** Hem Client-side (jQuery Validation) hem Server-side (C# Data Annotations) validasyonlar ile hatalı veri girişi engellenir.

## Kurulum ve Çalıştırma

Projeyi yerel makinenizde çalıştırmak için adımları izleyin:

1.  **Projeyi Klonlayın:**

2.  **Veritabanı Ayarları:**
    `appsettings.json` dosyasındaki `ConnectionStrings` alanını kendi SQL sunucunuza göre düzenleyin.

3.  **Migration İşlemleri (Veritabanını Oluşturma):**
    Terminali açın ve şu komutları sırasıyla çalıştırın:
    ```bash
    dotnet ef migrations add InitialCreate
    dotnet ef database update
    ```

4.  **Projeyi Başlatın:**
    ```bash
    dotnet run
    ```
    Tarayıcıda `https://localhost:7000` (veya size verilen port) adresine gidin.

5.  **Admin Girişi:**
    * **Email:** `g221210087@sakarya.edu.tr`
    * **Şifre:** `sau`


**Geliştirici:** Ahmet Tarık Türkmen
**Ders:** Web Programlama  
