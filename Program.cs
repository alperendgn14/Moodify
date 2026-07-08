using MusicAiAgent.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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
