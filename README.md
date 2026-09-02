# 🎵 Moodify AI

> Your mood, your music. Powered by AI.

Moodify AI, kullanıcıların anlık ruh hallerine veya betimledikleri durumlara göre kişiselleştirilmiş Spotify çalma listeleri oluşturan akıllı bir müzik asistanıdır.

Örneğin: *"Otoyolda gece sürüşü yaparken dinlemelik hareketli Türkçe şarkılar"* gibi karmaşık istekler, OpenAI GPT-4o-mini modeli kullanılarak optimize edilir ve Spotify'dan en uygun şarkılar seçilir.

---

## ✨ Öne Çıkan Özellikler

### 🧠 Doğal Dil İşleme ile Duygu Analizi
Kullanıcının karmaşık duygu durumlarını veya senaryolarını anlayıp, Spotify'ın arama algoritmasına en uygun optimize edilmiş arama terimlerini oluşturur.

### 🎯 Yapay Zeka Kalite Kontrolü
Spotify'dan gelen sonuçlar doğrudan kabul edilmez. İkinci bir AI süzgeci, gelen listeyi analiz ederek bağlama uymayan içerikleri (ninniler, coverlar, alakasız ses kayıtları) tespit edip listeden atar.

### 🔄 Otonom Tamamlama Döngüsü
Filtreleme sonrası listede eksilen şarkıların yerine yenilerini bulmak için yapay zeka, 10 şarkılık kusursuz hedefe ulaşana kadar API limitine takılmadan işlem yapar.

### ⚡ Tek Tıkla Çalma Listesi Oluşturma
Beğenilen 10 şarkılık liste, OAuth2 yetkilendirmesi üzerinden doğrudan kullanıcının kendi Spotify hesabına "Gizli Çalma Listesi" olarak saniyeler içinde kaydedilir.

---

## 🛠️ Teknoloji Stack'i

| Bileşen | Teknoloji |
|---------|-----------|
| **Backend** | C# / .NET 8 (Web API) |
| **AI Entegrasyonu** | OpenAI API (GPT-4o-mini) |
| **Müzik Verisi & Auth** | Spotify Web API & SpotifyAPI.Web |
| **Mimari Desenler** | Dependency Injection, Agentic Workflow, Recursive Backfilling |

---

## 🚀 Kurulum ve Çalıştırma

### Ön Gereksinimler
- **.NET 8.0 SDK** veya daha yeni bir sürüm
- **Spotify Developer** hesabından alınmış **Client ID** ve **Client Secret**
- **OpenAI API Key** (GPT-4o-mini erişimi için)

### Adım 1: Ortam Değişkenlerini Ayarlayın

Proje dizinindeki `appsettings.json` dosyasını açın ve API anahtarlarınızı girin:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "SpotifyOptions": {
    "ClientId": "YOUR_SPOTIFY_CLIENT_ID",
    "ClientSecret": "YOUR_SPOTIFY_CLIENT_SECRET"
  },
  "AiOptions": {
    "ApiKey": "YOUR_OPENAI_API_KEY"
  }
}
```

### Adım 2: Spotify Geliştirici Paneli Ayarları

Spotify Developer Dashboard üzerinde:
1. **Redirect URIs**: `https://localhost:7133/api/auth/callback` (veya canlı sunucunuzun callback adresi)
2. **User Management**: "Development" modundaysanız, test edeceğiniz Spotify hesabının e-posta adresini listeye ekleyin

### Adım 3: Projeyi Çalıştırın

```bash
cd Moodify
dotnet restore
dotnet run
```

Proje ayağa kalktığında tarayıcınızdan uygulamaya erişebilirsiniz.

---

## 🧠 Nasıl Çalışır?

```
┌─────────────────────────────────────────────────────────────┐
│                                                               │
│  1️⃣  İstek Aşaması                                           │
│     → Kullanıcı: "Gece otoyolda sürüşlük Türkçe parçalar"  │
│                                                               │
│  2️⃣  AI Analizi (OpenAI GPT-4o-mini)                        │
│     → Text → Optimize edilmiş Spotify Search Query           │
│                                                               │
│  3️⃣  Spotify Havuzu                                         │
│     → API'den random offset ile 10 şarkı çekme              │
│                                                               │
│  4️⃣  Kalite Kontrol (AI 2)                                  │
│     → Alakasız içerikleri tespit ve filtreleme              │
│                                                               │
│  5️⃣  Tamamlama Döngüsü                                      │
│     → Eksik şarkılar için yeniden sorgu (10 şarkıya kadar)  │
│                                                               │
│  6️⃣  Spotify'a Kaydet                                       │
│     → Kullanıcı profiline OAuth2 ile bağlan                 │
│     → Gizli Çalma Listesi oluştur                           │
│     → Şarkı URI'lerini ekle                                 │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

---

## ���� Proje Yapısı

```
Moodify/
├── Controllers/       # API Endpoints
├── Services/          # Business Logic
├── Models/            # Data Models
├── Integrations/      # OpenAI & Spotify API
├── Middleware/        # Auth & Error Handling
└── appsettings.json   # Konfigürasyon
```

---

## 📝 Lisans

Bu proje açık kaynaklıdır ve [MIT Lisansı](LICENSE) altında yayımlanmaktadır.

---

## 👨‍💻 Geliştirici

**Muhammet Alperen Doğan** - [GitHub Profili](https://github.com/alperendgn14)

---

## 🤝 Katkı

Katkılarınız için şimdiden teşekkür ederim. Herhangi bir bug v.s için bana ulaşırsanız sevinirim.

---

**Made with ❤️ for music lovers and AI enthusiasts**
