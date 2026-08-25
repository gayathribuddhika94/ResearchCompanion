using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResearchCompanion.Data;
using ResearchCompanion.Data.Entities;
using ResearchCompanion.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});

// Each academic source is registered as a typed HttpClient (base address set here,
// once) and then exposed through the shared IExternalSearchProvider interface so
// TopicDiscoveryService / LiteratureAggregationService don't care which source they're
// talking to. See the Technical Design document, Section 7.
builder.Services.AddHttpClient<SemanticScholarClient>(client =>
    client.BaseAddress = new Uri("https://api.semanticscholar.org/graph/v1/"));
builder.Services.AddHttpClient<CrossRefClient>(client =>
    client.BaseAddress = new Uri("https://api.crossref.org/"));
builder.Services.AddHttpClient<ArxivClient>(client =>
    client.BaseAddress = new Uri("http://export.arxiv.org/"));

builder.Services.AddScoped<IExternalSearchProvider>(sp => sp.GetRequiredService<SemanticScholarClient>());
builder.Services.AddScoped<IExternalSearchProvider>(sp => sp.GetRequiredService<CrossRefClient>());
builder.Services.AddScoped<IExternalSearchProvider>(sp => sp.GetRequiredService<ArxivClient>());

builder.Services.AddScoped<LiteratureAggregationService>();
builder.Services.AddScoped<TopicDiscoveryService>();
builder.Services.AddScoped<DocumentRenderer>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    await RoleSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
