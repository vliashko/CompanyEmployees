using Contracts;
using Entities;
using Entities.Models;
using LoggerService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace CompanyEmployees.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder =>
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader());
            });
        }

        public static void ConfigureIISIntegration(this IServiceCollection services)
        {
            services.Configure<IISOptions>(options => { });
        }

        public static void ConfigureLoggerService(this IServiceCollection services)
        {
            services.AddScoped<ILoggerManager, LoggerManager>();
        }

        public static void ConfigireSqlContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<RepositoryDbContext>(options => 
            options.UseSqlServer(
                configuration["ConnectionStrings:sqlConnection"], 
                    b => b.MigrationsAssembly("CompanyEmployees")));
        }

        public static void ConfigureRepositoryManager(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryManager, RepositoryManager>();
        }

        public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder) =>
            builder.AddMvcOptions(config => config.OutputFormatters.Add(new CsvOutputFormatter()));

        public static void ConfigureVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(opt =>
            {
                opt.ReportApiVersions = true;
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            });
        }

        public static void ConfigureIdentity(this IServiceCollection services)
        {
            var builder = services.AddIdentityCore<User>(o =>
            {
                o.Password.RequireDigit = true; 
                o.Password.RequireLowercase = false; 
                o.Password.RequireUppercase = false; 
                o.Password.RequireNonAlphanumeric = false; 
                o.Password.RequiredLength = 10; 
                o.User.RequireUniqueEmail = true;
            });

            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), builder.Services);

            builder.AddEntityFrameworkStores<RepositoryDbContext>().AddDefaultTokenProviders();
        }
        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings"); 
            var secretKey = Environment.GetEnvironmentVariable("SECRET");

            services.AddAuthentication(opt => 
            { 
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; 
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; 
            })
                .AddJwtBearer(options => 
                { 
                    options.TokenValidationParameters = new TokenValidationParameters 
                    { 
                        ValidateIssuer = true, 
                        ValidateAudience = true, 
                        ValidateLifetime = true, 
                        ValidateIssuerSigningKey = true, 
                        ValidIssuer = jwtSettings.GetSection("validIssuer").Value, 
                        ValidAudience = jwtSettings.GetSection("validAudience").Value, 
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)) 
                    }; 
                });
        }
        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new OpenApiInfo { Title = "Code Maze API", Version = "v1" });
                s.SwaggerDoc("v2", new OpenApiInfo { Title = "Code Maze API", Version = "v2" });

                s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme 
                { 
                    In = ParameterLocation.Header, 
                    Description = "Place to add JWT with Bearer", 
                    Name = "Authorization", 
                    Type = SecuritySchemeType.ApiKey, 
                    Scheme = "Bearer" 
                });

                s.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {   
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Name = "Bearer",
                        },
                        new List<string>()
                    }
                });
            });
        }
    }
}
