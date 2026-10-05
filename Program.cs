using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using LogiTrack.Data;
using LogiTrack.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LogiTrackContext>();
builder.Services.AddControllers();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
	options.User.RequireUniqueEmail = true;
	options.Password.RequiredLength = 8;
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireUppercase = true;
	options.Password.RequireNonAlphanumeric = true;
	options.Password.RequiredUniqueChars = 1;
})
	.AddEntityFrameworkStores<LogiTrackContext>();

var jwtSigningKey = builder.Configuration["Jwt:Key"]
	?? Environment.GetEnvironmentVariable("JWT_SIGNING_KEY");

if (string.IsNullOrWhiteSpace(jwtSigningKey))
{
	throw new InvalidOperationException("Configure Jwt:Key or JWT_SIGNING_KEY before starting the application.");
}

builder.Services.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
			ValidateIssuer = false,
			ValidateAudience = false,
			ValidateLifetime = true
		};
	});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	using var scope = app.Services.CreateScope();
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
	const string managerRole = "Manager";

	if (!await roleManager.RoleExistsAsync(managerRole))
	{
		var result = await roleManager.CreateAsync(new IdentityRole(managerRole));
		if (!result.Succeeded)
		{
			throw new InvalidOperationException(
				$"Could not create the Manager role: {string.Join(", ", result.Errors.Select(error => error.Description))}");
		}
	}

	var user = await userManager.FindByEmailAsync("samir@example.com");
	if (user is not null && !await userManager.IsInRoleAsync(user, managerRole))
	{
		var result = await userManager.AddToRoleAsync(user, managerRole);
		if (!result.Succeeded)
		{
			throw new InvalidOperationException(
				$"Could not assign the Manager role: {string.Join(", ", result.Errors.Select(error => error.Description))}");
		}
	}
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();