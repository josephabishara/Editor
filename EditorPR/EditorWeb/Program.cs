using EditorDataLayer.Data;
using EditorEntitiesLayer.Entities;
using EditorLogicLayer.Auth;
using EditorLogicLayer.Channel;
using EditorLogicLayer.Client;
using EditorLogicLayer.ClientArticleLogic;
using EditorLogicLayer.ClientNewsPaperLogic;
using EditorLogicLayer.ClientVideoLogic;
using EditorLogicLayer.Dashboard;
using EditorLogicLayer.GeneralArticle;
using EditorLogicLayer.News;
using EditorLogicLayer.Publication;
using EditorLogicLayer.Reports;
using EditorLogicLayer.Website;
using EditorLogicLayer.Writer;
using EditorRepositoryLayer.IRepositories;
using EditorRepositoryLayer.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── 2. ASP.NET Core Identity ──────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── 3. Cookie / Login path ────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ── 4. Dependency Injection ───────────────────────────────────────────────────

// ──  Repositories Injection ───────────────────────────────────────────────────
builder.Services.AddScoped<IWebsiteRepository, WebsiteRepository>();
builder.Services.AddScoped<IWriterRepository, WriterRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IClientCategoryRepository, ClientCategoryRepository>();
builder.Services.AddScoped<IAssistantRepository, AssistantRepository>();
builder.Services.AddScoped<IPublicationRepository, PublicationRepository>();
builder.Services.AddScoped<IChannelRepository, ChannelRepository>();
builder.Services.AddScoped<IWebsiteCustomerCategoryRepository, WebsiteCustomerCategoryRepository>();
builder.Services.AddScoped<IPublicationCustomerCategoryRepository, PublicationCustomerCategoryRepository>();
builder.Services.AddScoped<IChannelCustomerCategoryRepository, ChannelCustomerCategoryRepository>();
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<IClientNewsRepository, ClientNewsRepository>();
builder.Services.AddScoped<INewsPaperRepository, NewsPaperRepository>();
builder.Services.AddScoped<IClientNewsPaperRepository, ClientNewsPaperRepository>();
builder.Services.AddScoped<IGeneralArticleRepository, GeneralArticleRepository>();
builder.Services.AddScoped<IClientArticleRepository, ClientArticleRepository>();
builder.Services.AddScoped<IGeneralVideosRepository, GeneralVideosRepository>();
builder.Services.AddScoped<IClientVideoRepository, ClientVideoRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IGeneralArticleRepository, GeneralArticleRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportArticleRepository, ReportArticleRepository>();
builder.Services.AddScoped<IReportNewspaperRepository, ReportNewspaperRepository>();


// ──    Services Injection    ───────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>(); 
builder.Services.AddScoped<IWebsiteService, WebsiteService>();
builder.Services.AddScoped<IWriterService, WriterService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IClientCategoryService, ClientCategoryService>();
builder.Services.AddScoped<IAssistantService, AssistantService>();
builder.Services.AddScoped<IPublicationService, PublicationService>();
builder.Services.AddScoped<IChannelService, ChannelService>();
builder.Services.AddScoped<IClientNewsService, ClientNewsService>();
builder.Services.AddScoped<IClientNewsPaperService, ClientNewsPaperService>();
builder.Services.AddScoped<IClientArticleService, ClientArticleService>();
builder.Services.AddScoped<IClientVideoService, ClientVideoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IGeneralArticleService, GeneralArticleService>();
builder.Services.AddScoped<IReportService, ReportService>();

// ── 5. MVC ────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── 6. Seed Roles on startup ──────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    await authService.SeedRolesAsync();
}

// ── 7. Middleware pipeline ────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
