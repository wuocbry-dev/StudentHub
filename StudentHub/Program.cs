using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Services;
using StudentHub.Services.HocVuot;

namespace StudentHub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<CanhBaoHocTapService>();
            builder.Services.AddScoped<IGioiHanHocVuotService, GioiHanHocVuotService>();
            builder.Services.AddScoped<IGoiYHocVuotService, GoiYHocVuotService>();
            builder.Services.AddDbContext<SimsDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SimsConnection")));
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(8);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/TaiKhoan/DangNhap";
                    options.AccessDeniedPath = "/TaiKhoan/TuChoiTruyCap";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SimsDbContext>();
                dbContext.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "admin-home",
                pattern: "Admin/Home/{action=Index}/{id?}",
                defaults: new { controller = "Dashboard" });

            app.MapControllerRoute(
                name: "admin",
                pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
