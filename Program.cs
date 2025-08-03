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

// ✅ Only run Resume ingestion when not disabled.
// In Azure set DISABLE_RAG_INGEST=1 so it does NOT re-ingest on every deploy/start.
// Locally you can leave it unset so it runs once when you need it.
if (builder.Configuration["DISABLE_RAG_INGEST"] != "1")
{
    builder.Services.AddHostedService<ResumeIngestionHostedService>();
}

var app = builder.Build();

// In production: error page + HSTS + HTTPS redirection
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

// sample endpoint (optional)
app.MapGet("/api/github", async (HttpClient http, string user) =>
{
    var req = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/users/{user}/repos");
    req.Headers.UserAgent.ParseAdd("BrettPortfolio/1.0");
    var resp = await http.SendAsync(req);
    var json = await resp.Content.ReadAsStringAsync();
    return Results.Content(json, "application/json");
});

// attribute-routed controllers (e.g., /api/chat, /Contact)
app.MapControllers();

// MVC conventional routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();