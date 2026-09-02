Moodify AI 🎵🤖

Moodify AI, kullanıcıların anlık ruh hallerine veya betimledikleri durumlara göre (Örn: "Otoyolda gece sürüşü yaparken dinlemelik hareketli Türkçe şarkılar") OpenAI GPT-4o-mini modelini kullanarak spotify çalma listeleri oluşturan akıllı bir yapay zeka ajanıdır.

Sıradan müzik botlarının aksine Moodify, "Agentic Workflow" (Temsilci İş Akışı) mimarisiyle çalışır. Çekilen şarkıları doğrudan kullanıcıya sunmak yerine ikinci bir yapay zeka kalite kontrolünden geçirir; alakasız sonuçları, çocuk şarkılarını veya podcastleri filtreler ve liste eksildikçe kusursuz 10 şarkıya ulaşana kadar ile Spotify'a otonom istekler atar.

🚀 Öne Çıkan Özellikler
 * Doğal Dil İşleme ile Duygu Analizi: Kullanıcının karmaşık duygu durumlarını veya senaryolarını anlayıp, Spotify'ın arama algoritmasına en uygun optimize edilmiş arama terimlerini üretir.

 * Yapay Zeka Kalite Kontrolü : Spotify'dan gelen sonuçlar doğrudan kabul edilmez. İkinci bir AI süzgeci, gelen listeyi analiz ederek bağlama uymayan içerikleri (ninniler, coverlar, alakasız türler) çöpe atar.

 * Otonom Tamamlama Döngüsü: Filtreleme sonrası listede eksilen şarkıların yerine yenilerini bulmak için yapay zeka, 10 şarkılık kusursuz hedefe ulaşana kadar API limitine takılmadan Spotify havuzunda gezinmeye devam eder.

 * Tek Tıkla Çalma Listesi Oluşturma: Beğenilen 10 şarkılık liste, OAuth2 yetkilendirmesi üzerinden doğrudan kullanıcının kendi Spotify hesabına "Gizli Çalma Listesi" olarak saniyeler içinde kaydedilir.

🛠️ Kullanılan Teknolojiler
 * Backend: C# / .NET 8 (Web API)
 * Yapay Zeka Entegrasyonu: OpenAI API (GPT-4o-mini)
 * Müzik Verisi & Yetkilendirme: Spotify Web API & SpotifyAPI.Web NuGet Paketi
 * Mimari Desenler: Dependency Injection, Agentic Workflow, Recursive Backfilling

⚙️ Kurulum ve Çalıştırma
Projeyi yerel ortamınızda çalıştırmak için aşağıdaki adımları izleyin:

1. Gereksinimler
 * .NET 8.0 SDK
 * Spotify Developer hesabından alınmış Client ID ve Client Secret.
 * OpenAI üzerinden alınmış API Key.

2. Ortam Değişkenlerinin (appsettings.json) Ayarlanması
Proje dizinindeki appsettings.json dosyasını açın ve kendi API anahtarlarınızı ilgili yerlere girin:

{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "SpotifyOptions": {
    "ClientId": "SPOTIFY_CLIENT_ID",
    "ClientSecret": "SPOTIFY_CLIENT_SECRET"
  },
  "AiOptions": {
    "ApiKey": "OPENAI_API_KEY"
  }
}

3. Spotify Geliştirici Paneli Ayarları
Spotify'ın yetkilendirme sisteminin çalışması için Spotify Developer Dashboard üzerinde uygulamanıza şu ayarları yapmalısınız:
 * Redirect URIs: https://localhost:7133/api/auth/callback veya canlı sunucunuzun callback adresi.
 * User Management: Uygulamanız "Development" modunda olduğu sürece, test edeceğiniz Spotify hesabının e-posta adresini bu listeye eklemelisiniz. Aksi takdirde 403 Forbidden hatası alırsınız.

4. Projeyi Başlatma
Terminal üzerinden proje dizinine gidin ve aşağıdaki komutu çalıştırın:
dotnet run

Proje ayağa kalktığında tarayıcınızdan uygulamanın URL'sine giderek sistemi kullanmaya başlayabilirsiniz.

🧠 Nasıl Çalışır?
 * İstek Aşaması: Kullanıcı "Gece otoyolda sürüşlük Türkçe parçalar" yazar.

 * Analiz (AI 1): OpenAI bu metni türkçe hareketli şeklinde optimize edilmiş bir Spotify Search Query'sine dönüştürür.

 * Havuz : Spotify'dan rastgele bir sayfalama (offset) ile 10 adet şarkı çekilir.

 * Kalite Kontrol (AI 2): OpenAI listeyi inceler. Aradaki çocuk şarkılarını veya alakasız ses kayıtlarını tespit edip listeden atar.

 * Tamamlama : Liste 10 şarkının altına düştüyse (Örn: 7 şarkı kaldıysa), eksik kalan 3 şarkı için Spotify'dan yeni bir istek yapılır ve tekrar kalite kontrole sokulur. Bu işlem liste 10 şarkıya ulaşana kadar veya maksimum deneme sınırına gelene kadar sürer.

 * Oluşturma: Kullanıcı "Spotify'a Kaydet" dediğinde, kullanıcının profiline bağlanılır, gizli bir çalma listesi açılır ve şarkı URI'leri tek seferde bu listeye post edilir.