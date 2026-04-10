using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TaskFlow.API.Authorization;
using TaskFlow.Application.Common.Behaviors;
using TaskFlow.Infrastructure.Auth;
using TaskFlow.Infrastructure.BackgroundJobs;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.RealTime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskFlow API",
        Version = "v1",
        Description = "Project Management Platform with real-time collaboration"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<JwtTokenService>();

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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/board"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddHangfire(cfg => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(ValidationBehavior<,>).Assembly,
        typeof(TaskFlow.Domain.Events.DomainEvent).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>),
                   typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(
    typeof(ValidationBehavior<,>).Assembly);

builder.Services.AddScoped<IAuthorizationHandler, WorkspaceAuthHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("WorkspaceViewer", policy =>
        policy.Requirements.Add(new WorkspaceRequirement(WorkspaceRole.Viewer)));
    options.AddPolicy("WorkspaceMember", policy =>
        policy.Requirements.Add(new WorkspaceRequirement(WorkspaceRole.Member)));
    options.AddPolicy("WorkspaceAdmin", policy =>
        policy.Requirements.Add(new WorkspaceRequirement(WorkspaceRole.Admin)));
    options.AddPolicy("WorkspaceOwner", policy =>
        policy.Requirements.Add(new WorkspaceRequirement(WorkspaceRole.Owner)));
});

builder.Services.AddScoped<MentionEmailJob>();
builder.Services.AddScoped<OverdueTasksJob>();
builder.Services.AddSignalR();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var retries = 5;
    while (retries > 0)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch
        {
            retries--;
            Thread.Sleep(5000);
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<BoardHub>("/hubs/board");
app.UseHangfireDashboard("/hangfire");

try
{
    RecurringJob.AddOrUpdate<OverdueTasksJob>(
        "overdue-tasks",
        job => job.ExecuteAsync(),
        "0 8 * * *");
}
catch (Exception ex)
{
    Console.WriteLine($"Hangfire recurring job registration failed: {ex.Message}");
}

app.Run();