# 🚀 URL Shorter API (Minimal API)

Kısa bağlantılar oluşturmanızı, yönlendirmenizi ve tıklanma analizlerini görüntülemenizi sağlayan basit bir **Minimal API** projesidir.

---

## 🧩 Kullanılan Teknolojiler

- **.NET 8 Minimal API**
- **Entity Framework Core**
- **EFCore.InMemory** — Geçici veritabanı kullanımı
- **UAParser** — Tarayıcı, cihaz ve işletim sistemi tespiti
- **MaxMind.GeoIP2** — IP adresinden ülke ve şehir bilgisi

---

## 🔗 API Endpointleri

### ➕ 1. Kısa Link Oluşturma

`POST /link/add`
**Body:**

```json
"https://example.com"
```

**Response:**

```json
{
  "shortUrl": "https://localhost/abc123",
  "expirationDate": "2025-11-01T21:00:00Z"
}
```

---

### 🔁 2. Kısa Link Yönlendirme

`GET /{code}`
Kullanıcıyı kısa linkin temsil ettiği orijinal URL’ye yönlendirir.
Ayrıca:

- IP adresi,
- Tarayıcı, cihaz, işletim sistemi,
- Ülke ve şehir bilgilerini **Analytic** tablosuna kaydeder.

---

### 📊 3. Analizleri Görüntüleme

`GET /link/{code}/analytic`
Kısa linke ait tıklanma geçmişini döner.
**Response:**

```json
{
  "originalUrl": "https://example.com",
  "createdDate": "2025-10-30T21:00:00Z",
  "analytics": [
    {
      "ipAddress": "192.168.1.5",
      "country": "Turkey",
      "browser": "Chrome",
      "os": "Windows 11"
    }
  ]
}
```

---

## ⚙️ Kurulum ve Kullanım

1. **Projeyi klonla veya indir:**

   ```bash
   git clone https://github.com/berkaycelik66/UrlShorter.API.git
   cd UrlShorter.API
   ```

2. **Gerekli paketleri yükle:**

   ```bash
   dotnet add package Microsoft.EntityFrameworkCore
   dotnet add package Microsoft.EntityFrameworkCore.InMemory
   dotnet add package UAParser
   dotnet add package MaxMind.GeoIP2
   ```

3. **Veritabanı yapılandırması:**

   - Bu proje `InMemory` veritabanı kullanır.
     Kalıcı hale getirmek istersen `UseSqlServer()` ile değiştirebilirsin.

4. **GeoIP veritabanını ekle:**

   - Proje dizinine `GeoLite2-City.mmdb` dosyasını ekle.
     (MaxMind hesabından ücretsiz olarak indirilebilir.)

5. **Projeyi çalıştır:**

   ```bash
   dotnet run
   ```

6. **Test et (örnek):**
   - `POST http://localhost:5000/link/add`
     Body: `"https://example.com"`

---

## 💡 Örnek Kullanım Akışı

1. Uzun link gönder → kısa link al.
2. Kısa linke tıkla → orijinal linke yönlendiril.
3. `GET /link/{code}/analytic` ile tıklanma analizlerini görüntüle.
