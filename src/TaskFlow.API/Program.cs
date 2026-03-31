using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskFlow.API.Authorization;
using TaskFlow.Application.Common.Behaviors;
using TaskFlow.Infrastructure.Auth;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.RealTime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddAuthorization();

builder.Services.AddSignalR();

builder.Services.AddMediatR(cfg => {
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
        policy.Requirements.Add(
            new WorkspaceRequirement(WorkspaceRole.Viewer)));

    options.AddPolicy("WorkspaceMember", policy =>
        policy.Requirements.Add(
            new WorkspaceRequirement(WorkspaceRole.Member)));

    options.AddPolicy("WorkspaceAdmin", policy =>
        policy.Requirements.Add(
            new WorkspaceRequirement(WorkspaceRole.Admin)));

    options.AddPolicy("WorkspaceOwner", policy =>
        policy.Requirements.Add(
            new WorkspaceRequirement(WorkspaceRole.Owner)));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<BoardHub>("/hubs/board");
app.Run();