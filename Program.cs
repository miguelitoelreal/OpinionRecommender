using OpinionRecommender.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registro del servicio de análisis de sentimiento
builder.Services.AddSingleton<OpinionRecommender.Services.SentimentService>();
// Registro del servicio de recomendación
builder.Services.AddSingleton<OpinionRecommender.Services.RecommendationService>();
builder.Services.AddHttpClient();
builder.Services.AddControllers(); // Habilitar controladores y endpoints API convencionales

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapControllers(); // Habilitar controladores y endpoints API convencionales

app.Run();
