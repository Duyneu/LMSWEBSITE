// File: Program.cs
using Microsoft.AspNetCore.Identity;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostgraduateManagement.Data;
using PostgraduateManagement.Repositories.Implementations;
using PostgraduateManagement.Repositories.Interfaces;
using PostgraduateManagement.Services.Implementations;
using PostgraduateManagement.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình chuỗi kết nối và DbContext (hỗ trợ SQL Server hoặc SQLite linh hoạt khi deploy Cloud/Container)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Nếu chạy trên Linux / Container mà chuỗi kết nối là LocalDB (hoặc rỗng) -> Chuyển sang SQLite tự động
    if (!OperatingSystem.IsWindows() && (string.IsNullOrEmpty(connectionString) || connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase)))
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "PostgraduateManagement.db");
        options.UseSqlite($"Data Source={dbPath}");
    }
    else if (!string.IsNullOrEmpty(connectionString) && 
        (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) || 
         (connectionString.Contains("Database=", StringComparison.OrdinalIgnoreCase) && !connectionString.EndsWith(".db", StringComparison.OrdinalIgnoreCase))))
    {
        options.UseSqlServer(connectionString);
    }
    else if (!string.IsNullOrEmpty(connectionString) && 
             (connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase) || connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase)))
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        if (OperatingSystem.IsWindows())
        {
            options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=PostgraduateManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
        }
        else
        {
            var dbPath = Path.Combine(AppContext.BaseDirectory, "PostgraduateManagement.db");
            options.UseSqlite($"Data Source={dbPath}");
        }
    }
});

// 2. Cấu hình ASP.NET Core Identity cho phân quyền theo vai trò
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    // Chính sách mật khẩu mạnh: tối thiểu 12 ký tự, đủ hoa, thường, số và ký tự đặc biệt
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
    // Khóa tài khoản 5 phút sau 5 lần sai (đặt tường minh thay vì dựa vào mặc định)
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
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
    // Phiên hết hạn sau 30 phút không hoạt động, buộc đăng nhập lại
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// 6. Đăng ký dịch vụ MVC Controllers và Views với bộ lọc chống tấn công CSRF toàn cục
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// 7. Giới hạn tần suất đăng nhập theo địa chỉ IP để giảm dò mật khẩu và quá tải (HTTP 429)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
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
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | 
                       Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Tiêu đề bảo mật cơ bản cho mọi phản hồi
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Thesis}/{action=Index}/{id?}");

app.Run();
