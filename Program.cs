using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using SignupIdentity;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<User, IdentityRole>(opts => opts.SignIn.RequireConfirmedEmail = true)
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

var app = builder.Build();

app.MapPost("/register", async ([FromBody] RegisterUserDto dto, UserManager<User> userManager) =>
{
    User user = new() { Email = dto.Email, UserName = dto.UserName };

    var result = await userManager.CreateAsync(user, dto.Password);
    if (!result.Succeeded) return Results.BadRequest(result.Errors);

    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
    var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

    var confirmationLink = $"https://localhost:7266/confirm-email?email={user.Email}&code={code}";
    await new EmailService(builder.Configuration).SendEmailConfirmation(user.Email, confirmationLink);

    return Results.Created();
});

app.MapPost("/login", async ([FromBody] LoginDto dto, UserManager<User> userManager, SignInManager<User> signInManager) =>
{
    var user = await userManager.FindByEmailAsync(dto.Email);
    if (user is null) return Results.NotFound("User not found.");

    var confirmedEmail = await userManager.IsEmailConfirmedAsync(user);
    if (!confirmedEmail) return Results.BadRequest("Email not confirmed.");

    var result = await signInManager.PasswordSignInAsync(user, dto.Password, false, false);
    return result.Succeeded ? Results.Ok("Successful login!") : Results.BadRequest("Failed sign-in.");
});

app.MapGet("/confirm-email", async ([FromQuery] string email, string code, UserManager<User> userManager) =>
{
    var user = await userManager.FindByEmailAsync(email);
    if (user is null) return Results.NotFound("User not found.");

    var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
    var result = await userManager.ConfirmEmailAsync(user, token);

    return result.Succeeded ? Results.Ok("Confirmed!") : Results.BadRequest(result.Errors);
});

app.Run();
