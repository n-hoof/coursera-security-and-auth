using Microsoft.EntityFrameworkCore;
using MyWebApp.Data;
//using MyWebApp.Repositories;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Register EF Core DbContext using the InMemory database provider.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("MyInMemoryDb"));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

/* // Register the UserRepository as a scoped service.
builder.Services.AddScoped<UserRepository>(); */

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    string[] roles = { "admin", "user" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Optional: seed an admin user
    var adminEmail = "admin@example.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser { UserName = "admin", Email = adminEmail };
        await userManager.CreateAsync(adminUser, "Admin#123");
        await userManager.AddToRoleAsync(adminUser, "admin");
    }
}

/* using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Seed initial data if the Users table is empty.
    if (!dbContext.Users.Any())
    {
        dbContext.Users.AddRange(
            new MyWebApp.Models.User { Username = "alice", Email = "alice@example.com" },
            new MyWebApp.Models.User { Username = "bob", Email = "bob@example.com" }
        );
        dbContext.SaveChanges();
    }
} */

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}



app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
