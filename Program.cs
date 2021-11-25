using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var username = "account";
var email = username + "@mail.com";
var password = "P4ssw0rd@#123";
var connectionString = "DataSource=identity.db";

var builder = WebApplication.CreateBuilder(args);

// add dbcontext
builder.Services
    .AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));

// add identity service
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// create user
app.MapGet("/", async (UserManager<IdentityUser> userManager) =>
{
    var user = await userManager.FindByNameAsync(username);

    if (user != null)
    {
        return user;
    }

    var result = await userManager
        .CreateAsync(new IdentityUser
        {
            Email = email,
            UserName = username,
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        }, password);

    return await userManager.FindByNameAsync(username);
});

// checklogin
app.MapGet("/check-login", async (SignInManager<IdentityUser> signInManager) =>
{
    var user = await signInManager
        .UserManager
        .FindByNameAsync(username);

    return await signInManager
        .CheckPasswordSignInAsync(user, password, true);
});

// start kestrel server
(await EnsureDb()).Run();

// migrate db
async Task<WebApplication> EnsureDb()
{
    var db = app.Services.GetRequiredService<ApplicationDbContext>();

    await db.Database.MigrateAsync();

    return app;
}

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}