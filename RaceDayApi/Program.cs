using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;    
using Microsoft.IdentityModel.Tokens;
using RaceDayApi.Data;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDayConnection")));

//Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    //Define the security scheme (JWT Bearer)
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token below.\n\nExample: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    //Apply the security scheme globally to all endpoints
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Prepare new databases and update missing legacy Event columns for this local POE.
Exception? databaseStartupError = null;
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
    db.Database.EnsureCreated();
    if (app.Environment.IsDevelopment() && builder.Configuration.GetValue("Database:ApplyCompatibilityUpdates", true))
        DatabaseCompatibility.Apply(db);
}
catch (Exception ex)
{
    databaseStartupError = ex;
    app.Logger.LogError(ex, "RaceDay database initialization failed");
}

// Return useful JSON rather than an opaque HTML 500 page during local debugging.
app.Use(async (context, next) =>
{
    try
    {
        if (databaseStartupError != null && context.Request.Path.StartsWithSegments("/api") && context.Request.Path != "/api/health")
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsJsonAsync(new { message = app.Environment.IsDevelopment()
                ? "Database setup failed: " + databaseStartupError.GetBaseException().Message
                : "The database is currently unavailable." });
            return;
        }
        await next(context);
    }
    catch (Exception ex) when (!context.Response.HasStarted)
    {
        app.Logger.LogError(ex, "API request {Path} failed", context.Request.Path);
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { message = app.Environment.IsDevelopment()
            ? ex.GetBaseException().Message : "The API could not complete this request." });
    }
});
app.MapGet("/api/health", () => Results.Json(new {
    status = databaseStartupError == null ? "ready" : "database_error",
    message = databaseStartupError == null ? "RaceDay API is running."
        : app.Environment.IsDevelopment() ? databaseStartupError.GetBaseException().Message : "Database unavailable."
}, statusCode: databaseStartupError == null ? 200 : 503));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseStaticFiles();
app.Run();