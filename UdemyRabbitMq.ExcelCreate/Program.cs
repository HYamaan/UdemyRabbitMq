using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using UdemyRabbitMq.ExcelCreate.Hubs;
using UdemyRabbitMq.ExcelCreate.Models;
using UdemyRabbitMq.ExcelCreate.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"));
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddSingleton(sp => new ConnectionFactory()
{
    Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMqConnection")),
    DispatchConsumersAsync = true
});
builder.Services.AddSingleton<RabbitMqClientServices>();
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<MyHub>("/myhub");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())

{
    var services = scope.ServiceProvider;

    var appDbContext = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    await appDbContext.Database.MigrateAsync();
    if (!appDbContext.Users.Any())

    {

        await userManager.CreateAsync(new IdentityUser { UserName = "deneme", Email = "deneme@outlook.com" },
            "Password12*");

        await userManager.CreateAsync(new IdentityUser { UserName = "deneme2", Email = "deneme2@outlook.com" },
            "Password12*");

    }
}

app.Run();
