using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignupIdentity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

var app = builder.Build();

app.MapPost("/register", async ([FromBody] RegisterUserDto dto, UserManager<User> userManager) =>
{
    User user = new() { Email = dto.Email, UserName = dto.UserName };

    var result = await userManager.CreateAsync(user, dto.Password);
    if (!result.Succeeded) return Results.BadRequest(result.Errors);

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

app.Run();
