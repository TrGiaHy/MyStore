using BusinessLogic.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.DBContext;
using MyStore.Web.Services;
var builder = WebApplication.CreateBuilder(args);

// Kết nối DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
// Dependency Injection
builder.Services.ConfigureServices();
builder.Services.ConfigureRepository();


builder.Services.AddHttpClient();



builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 0;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    //options.SignIn.RequireConfirmedAccount = true;
    //options.SignIn.RequireConfirmedEmail = true;
});

//builder.Services.AddScoped<IPasswordValidator<AppUser>, CustomPasswordValidator<AppUser>>();

// Cấu hình Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Home/Login";
    options.LogoutPath = "/Home/Logout";
    options.AccessDeniedPath = "/Error/404";
    options.ReturnUrlParameter = "ReturnUrl";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;

});

//Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Home/Login";
    options.LogoutPath = "/Home/Logout";
    options.AccessDeniedPath = "/Error/404";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});


builder.Services.ConfigureJwtAuthentication(builder.Configuration);
builder.Services.ConfigureSwagger();

var app = builder.Build();


// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // hiện lỗi chi tiết
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyStore API V1");
        c.InjectJavascript("/swagger/custom-swagger.js"); // 👈 nạp file JS
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();
//app.UseSwagger();
//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
//    c.RoutePrefix = "swagger"; // Swagger UI sẽ hiển thị khi truy cập /swagger
//    c.DefaultModelExpandDepth(-1);
//});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
