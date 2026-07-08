using MusicAiAgent.Services;
using Microsoft.AspNetCore.HttpOverrides;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
// 1-render proxy ayarı, kullanıcının gerçek ip adresini yakalamak için
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// 2-rate limit, ip başına dakikada maksimum 7-8 istek
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("MoodifyKalkani", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "BilinmeyenIP",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 7, // 1 dakikadaki maksimum istek hakkı
                Window = TimeSpan.FromMinutes(1), // süre 
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0 // 8. istek anında reddedilir
            }));

    // sınır aşıldığında 429 hatası döndür
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<OpenAiService>();
builder.Services.AddScoped<SpotifyService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:5173") // Vite portuna izin veriyoruz
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

app.UseForwardedHeaders(); // proxy'den gerçek ip'yi okumayı başlat
app.UseRateLimiter();      // rate limiting aktif


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Music AI Agent API V1");
    });
}


app.UseDefaultFiles(); // index.html dosyasını ana sayfa yap
app.UseStaticFiles();  // wwwroot klasörünü dış dünyaya aç
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthorization();


app.MapGet("/ping", () => Results.Ok("pong")); // burası uptimemonitor için kullanılacak, uptimemonitor ping atacak ve 200 dönerse uygulama çalışıyor.
app.MapControllers();

app.Run();
