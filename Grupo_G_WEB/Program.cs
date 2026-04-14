using Grupo_G_WEB.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".CostaRicaMusic.Session";
    options.IdleTimeout = TimeSpan.FromHours(6);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.Configure<MusicApiOptions>(builder.Configuration.GetSection(MusicApiOptions.SectionName));
builder.Services.AddHttpClient<IMusicCatalogService, MusicApiCatalogService>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<MusicApiOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        throw new InvalidOperationException("MusicApi:BaseUrl must be configured.");
    }

    client.BaseAddress = new Uri(options.BaseUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
