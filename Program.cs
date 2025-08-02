using BrettGravesPortfolio.Services;
using BrettGravesPortfolio.HostedServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSingleton<IEmbeddingStore, SqliteEmbeddingStore>();
builder.Services.AddSingleton<PdfChunker>();
builder.Services.AddSingleton<OpenAiClients>();
builder.Services.AddScoped<IResumeRagService, ResumeRagService>();

// Email sender: use SendGrid if key exists; otherwise log to file (dev)
if (!string.IsNullOrWhiteSpace(builder.Configuration["SENDGRID_API_KEY"]))
    builder.Services.AddSingleton<IEmailSender, SendGridEmailSender>();
else
    builder.Services.AddSingleton<IEmailSender, FileEmailSender>();

//builder.Services.AddHostedService<ResumeIngestionHostedService>();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<ResumeIngestionHostedService>();
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();   // force HTTPS only in prod
}

app.UseStaticFiles();
app.UseRouting();

// sample endpoint
app.MapGet("/api/github", async (HttpClient http, string user) =>
{
    var req = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/users/{user}/repos");
    req.Headers.UserAgent.ParseAdd("BrettPortfolio/1.0");
    var resp = await http.SendAsync(req);
    var json = await resp.Content.ReadAsStringAsync();
    return Results.Content(json, "application/json");
});

// enable attribute-routed controllers (e.g., /api/chat, /Contact)
app.MapControllers();

// MVC conventional routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();