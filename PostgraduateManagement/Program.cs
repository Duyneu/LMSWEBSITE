// File: Program.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostgraduateManagement.Data;
using PostgraduateManagement.Repositories.Implementations;
using PostgraduateManagement.Repositories.Interfaces;
using PostgraduateManagement.Services.Implementations;
using PostgraduateManagement.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình chuỗi kết nối SQL Server và DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=PostgraduateManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Cấu hình ASP.NET Core Identity cho phân quyền theo vai trò
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 3. Đăng ký HttpContextAccessor phục vụ ghi vết kiểm toán (Audit Logging)
builder.Services.AddHttpContextAccessor();

// 4. Đăng ký kho lưu trữ dữ liệu (Repository Pattern) và Unit of Work
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IThesisRepository, ThesisRepository>();
builder.Services.AddScoped<IAcademicPersonRepository, AcademicPersonRepository>();
builder.Services.AddScoped<IDefenseSessionRepository, DefenseSessionRepository>();
builder.Services.AddScoped<ISubmittedDocumentRepository, SubmittedDocumentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 4. Đăng ký các dịch vụ nghiệp vụ (Business Services) và AI Reviewer Suggestion
builder.Services.AddScoped<IThesisService, ThesisService>();
builder.Services.AddScoped<IEligibilityAndAssignmentService, EligibilityAndAssignmentService>();
builder.Services.AddScoped<IDefenseAndDocumentService, DefenseAndDocumentService>();
builder.Services.AddScoped<IAiReviewerSuggestionService, AiReviewerSuggestionService>();

// 5. Cấu hình bảo mật Cookie xác thực ASP.NET Core Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// 6. Đăng ký dịch vụ MVC Controllers và Views với bộ lọc chống tấn công CSRF toàn cục
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

// 4. Tự động khởi tạo cơ sở dữ liệu và nạp dữ liệu mẫu phong phú
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        await DbInitializer.InitializeAsync(services);
        logger.LogInformation("Khởi tạo cơ sở dữ liệu và nạp dữ liệu mẫu M05 thành công.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Đã xảy ra lỗi trong quá trình khởi tạo cơ sở dữ liệu hoặc nạp dữ liệu mẫu.");
    }
}

// 5. Cấu hình HTTP request pipeline (Tuân thủ chuẩn .NET 8, không dùng MapStaticAssets của .NET 10)
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
    pattern: "{controller=Thesis}/{action=Index}/{id?}");

app.Run();
