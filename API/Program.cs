using API.Configurations;
using API.Entities;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(configuration.GetConnectionString("Default"));
});

builder.Services
    .AddIdentityCore<ApiUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

var googleSignInSettingsSection = configuration.GetSection(GoogleSigninSettings.SectionName);
var googleSignInSettings = googleSignInSettingsSection.Get<GoogleSigninSettings>();

var jwtSettingsSection = configuration.GetSection(JwtSettings.SectionName);
var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

builder.Services.Configure<JwtSettings>(jwtSettingsSection);


builder.Services
    .AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = true;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings!.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.JwtTokenSecret)),
            ValidAudience = jwtSettings!.Audience,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        x.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    })
    .AddCookie(IdentityConstants.ExternalScheme)
    .AddGoogle(options =>
       {
           options.ClientId = googleSignInSettings!.ClientId;
           options.ClientSecret = googleSignInSettings.ClientSecret;

           options.SignInScheme = IdentityConstants.ExternalScheme;
       });

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TokenService>();

var corsPolicyName = "AllowAll";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowCredentials()
            .AllowAnyHeader();

        // Get this from configuration
        builder
            .AllowAnyMethod()
            .AllowCredentials()
            .AllowAnyHeader()
            .WithOrigins("https://localhost:3000");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.UseHttpsRedirection();

app.UseCors(corsPolicyName);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
