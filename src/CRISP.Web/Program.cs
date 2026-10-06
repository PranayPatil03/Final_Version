using MudBlazor.Services;
using CRISP.Web.Components;
using CRISP.Web.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddMudServices();

var apiUrls = builder.Configuration.GetSection("ApiBaseUrls").Get<string[]>() ?? [];
if (apiUrls.Length == 0)
{
    var single = builder.Configuration["ApiBaseUrl"];
    if (!string.IsNullOrWhiteSpace(single)) apiUrls = [single];
}
if (apiUrls.Length == 0) apiUrls = ["http://localhost:5101/"];

builder.Services.AddHttpClient("CRISP.Api", client =>
{
    client.Timeout = TimeSpan.FromSeconds(8);
});
builder.Services.AddScoped(sp => new FrameworkApiClient(
    sp.GetRequiredService<IHttpClientFactory>(),
    apiUrls));

var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/error"); app.UseHsts(); }
app.UseStaticFiles();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
