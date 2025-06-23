using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PilotoIA_Backend.Models;
using System.Collections.Generic;
using System.Text;
using Microsoft.Net.Http.Headers;
using System;

namespace PilotoIA_Backend.Extensions
{
    public static class exServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services) =>
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                    builder.WithOrigins(
                        "https://lemon-ocean-0f32d2710.6.azurestaticapps.net",
                        "https://blue-wave-0e6abdf0f.2.azurestaticapps.net",
                        "http://localhost:5173"
                        )
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });
        public static void ConfigureSwagger(this IServiceCollection services) =>
            services.AddSwaggerGen(opt =>
            {
                var securityDefinition = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    In = ParameterLocation.Header,
                    Name = HeaderNames.Authorization,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                };
                opt.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityDefinition);
                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {securityDefinition, Array.Empty<string>()}
                });

                opt.OperationFilter<AuthResponsesOperationFilter>();
            });

        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
        {
            var appSettingsSection = configuration.GetSection("MySettings");
            var appSettings = appSettingsSection.Get<beMySettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }
    }
}
