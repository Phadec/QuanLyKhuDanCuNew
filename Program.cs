using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyKhuDanCu.Data;
using QuanLyKhuDanCu.Models;
using QuanLyKhuDanCu.Data.Seeders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// Add policy-based authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireManagerRole", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("RequireStaffRole", policy => 
        policy.RequireRole("Admin", "Manager", "Staff"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Ensure database is created and apply migrations
        context.Database.EnsureCreated();
        
        // Update database schema directly if needed
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            connection.Open();
            
        using (var command = connection.CreateCommand())
        {
            // Make ThongBao.FileDinhKem nullable
            command.CommandText = @"
                ALTER TABLE ThongBaos MODIFY COLUMN FileDinhKem longtext NULL;";
            try { command.ExecuteNonQuery(); } catch (Exception ex) { 
                Console.WriteLine("Error making FileDinhKem nullable: " + ex.Message); 
            }
            
            // Make YeuCauDichVus.NguoiXuLyId nullable
            command.CommandText = @"
                ALTER TABLE YeuCauDichVus MODIFY COLUMN NguoiXuLyId varchar(255) NULL;";
            try { command.ExecuteNonQuery(); } catch (Exception ex) { 
                Console.WriteLine("Error making NguoiXuLyId nullable: " + ex.Message); 
            }
            
            // Make PhanAnhs.FileDinhKem nullable
            command.CommandText = @"
                ALTER TABLE PhanAnhs MODIFY COLUMN FileDinhKem longtext NULL;";
            try { command.ExecuteNonQuery(); } catch (Exception ex) { 
                Console.WriteLine("Error making PhanAnhs.FileDinhKem nullable: " + ex.Message); 
            }
            
            // Make GhiChu fields nullable
            command.CommandText = @"
                ALTER TABLE HoKhaus MODIFY COLUMN GhiChu longtext NULL;";
            try { command.ExecuteNonQuery(); } catch (Exception ex) { 
                Console.WriteLine("Error making HoKhaus.GhiChu nullable: " + ex.Message); 
            }
        }
        
        // Seed data
        await QuanLyKhuDanCu.Data.Seeders.DataSeeder.SeedDataAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.Run();
