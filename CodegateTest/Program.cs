using CodegateTest.DataAccess;
using CodegateTest.Models.CodegateTest.Models;
using CodegateTest.Repositories;
using CodegateTest.Repositories.IRepositories;
using CodegateTest.Services;
using CodegateTest.Services.IServices;
using CodegateTest.Utilites.DbIntialiaion;
using CodegateTest.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
                      policy =>
                      {
                          policy.WithOrigins("http://127.0.0.1:5500",
                                              "http://localhost:5500",
                                              "http://localhost:4200",
                                              "http://localhost:5173")
                                        .AllowAnyMethod()
                                        .AllowAnyHeader()
                                        .AllowCredentials();
                      });
});

// Add Controllers
builder.Services.AddControllers();

builder.Services.AddOpenApi();


// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"
        )
    );
});


// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;

    options.SignIn.RequireConfirmedEmail = true;

    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,

            ValidIssuer =
                builder.Configuration["JWT:Issuer"],


            ValidateAudience = true,

            ValidAudience =
                builder.Configuration["JWT:Audience"],


            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero,


            ValidateIssuerSigningKey = true,

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["JWT:Key"]!
                    )
                )
        };
});


// Services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IJWTHandler, JWTHandler>();
builder.Services.AddScoped<IRepository<ApplicationUserOTP>, Repository<ApplicationUserOTP>>();
builder.Services.AddScoped<IRepository<Course>, Repository<Course>>();
builder.Services.AddScoped<IRepository<Instructor>, Repository<Instructor>>();
builder.Services.AddScoped<IRepository<CourseInstructors>, Repository<CourseInstructors>>();
builder.Services.AddScoped<IRepository<Contact>, Repository<Contact>>();
builder.Services.AddScoped<IRepository<Review>, Repository<Review>>();
builder.Services.AddScoped<IImageService, ImageService>();

builder.Services.AddTransient<IEmailSender, EmailSender>();


builder.Services.AddScoped<DbIntializer>();



var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference();
}


// Database Initializer
using (var scope = app.Services.CreateScope())
{
    var dbInitializer =
        scope.ServiceProvider
            .GetRequiredService<DbIntializer>();

    await dbInitializer.dbIntializer();
}



app.UseHttpsRedirection();



app.UseAuthentication();

app.UseAuthorization();


app.MapControllers();

app.Run();