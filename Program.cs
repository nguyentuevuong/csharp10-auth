using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

var username = "admin";
var password = "P4ssw0rd@#123";

var builder = WebApplication.CreateBuilder(args);

// add identity service
builder
  .Services
  .AddAuthorization()
  .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie(options =>
  {
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.LoginPath = "/forbidden";
    options.AccessDeniedPath = "/forbidden";
  });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", [AllowAnonymous] () => "This a demo for Cookie Authentication using Minimalist Web API");

// đăng nhập
// test trên trình duyệt
// fetch("/login", { body: JSON.stringify( { UserName: "admin", Password: "P4ssw0rd@#123" }), method: "POST", headers: { "Content-Type": "application/json" }, })
// sau khi chạy lệnh fetch xong, chuyển sang url: /do-action
app.MapPost("/login", async (HttpContext httpContext) =>
{
  var userModel = await httpContext.Request.ReadFromJsonAsync<UserModel>();

  if (userModel.UserName == username && userModel.Password == password)
  {
    var claims = new List<Claim>
      {
        // thêm username bắt buộc (thay bắng khóa của bản ghi)
        new Claim(ClaimTypes.Name, userModel.UserName),
        // thêm các quyền vào đây
        new Claim(ClaimTypes.Role, "admin"),
      };

    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

    var authProperties = new AuthenticationProperties
    {
      AllowRefresh = true,
      // chấp nhận tự gia hạn cookie
      ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
      // Giới hạn thời gian sống của cookie
      IsPersistent = true,
      // Xác thực cookie tồn tại theo phiên
      IssuedUtc = DateTimeOffset.UtcNow,
      // Thời gian mà cookie bắt đầu được xác thực
    };

    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
  }

  return Task.CompletedTask;
});

app.MapGet("/forbidden", [AllowAnonymous] () => "Action denied");

app.MapGet("/do-action", [Authorize] () => "Action Succeeded");
app.MapGet("/do-action-by-roles", [Authorize(Roles = "admin")] () => "Action Succeeded");

// start kestrel server
app.Run();

// model đăngg nhập
public readonly record struct UserModel([Required] string UserName, [Required] string Password);