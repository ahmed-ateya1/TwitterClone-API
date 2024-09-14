using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.Domain.IdentityEntites;
using SocialMediaApp.Infrastructure.Data;
using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SocialMediaApp.Core.ServicesContract;
using SocialMediaApp.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using SocialMediaApp.Core.DTO.AuthenticationDTO;
using SocialMediaApp.API.FileServices;
using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Infrastructure.Repositories;
using SocialMediaApp.Core.IUnitOfWorkConfig;
using SocialMediaApp.Infrastructure.UnitOfWorkConfig;
using SocialMediaApp.Core.MappingProfile;

namespace SocialMediaApp.API.StartupExtensions
{
    public static class ConfigureServiceExtension
    {
        public static IServiceCollection ServiceConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("connstr"));
            });
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 5;
            })
               .AddEntityFrameworkStores<ApplicationDbContext>()
               .AddDefaultTokenProviders()
               .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
               .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
               .AddJwtBearer(o =>
               {
                   o.RequireHttpsMetadata = false;
                   o.SaveToken = false;
                   o.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuerSigningKey = true,
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ValidIssuer = configuration["JWT:Issuer"],
                       ValidAudience = configuration["JWT:Audience"],
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"])),
                       ClockSkew = TimeSpan.Zero
                   };
               });
            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(1);
            });
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod();
                });
            });
            services.Configure<JwtDTO>(configuration.GetSection("JWT"));
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthenticationServices, AuthenticationServices>();
            services.AddTransient<IMailingService, MailingService>();
            services.AddScoped<IFileServices, FileService>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IProfileServices, ProfileServices>();
            services.AddScoped<IGenreRepository, GenreRepository>();
            services.AddScoped<IGenreServces, GenreServces>();
            services.AddScoped<ITweetFilesRepository, TweetFilesRepository>();
            services.AddScoped<ITweetFilesServices, TweetFilesServices>();
            services.AddScoped<ITweetServices , TweetServices>();
            services.AddScoped<ITweetRepositroy , TweetRepository>();
            services.AddScoped<IUserConnectionsServices,UserConnectionsServices>();
            services.AddAutoMapper(typeof(ProfileConfig));
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Social Media APP", Version = "v1" });
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"));
            });
            return services;
        }
    }
}
