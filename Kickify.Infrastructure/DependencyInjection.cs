using BrewView.Infrastructure.Authentication;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.OTP;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Infrastructure.Authentication;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Mail;
using Kickify.Infrastructure.Persistence;
using Kickify.Infrastructure.Redis;
using Kickify.Infrastructure.Repositories;
using Kickify.Infrastructure.Services;
using Kickify.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Minio;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickify.Infrastructure.ChatConnection;

namespace Kickify.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddAuthenticationInternal(configuration)
            .AddDatabase(configuration)
            .AddService(configuration)
            .AddRepository()
            .AddFirebase()
            .AddRedisStore(configuration)
            .AddMinioStorage(configuration)
            .AddSignalRServices();

        private static IServiceCollection AddAuthenticationInternal(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.TokenValidationParameters = new TokenValidationParameters()
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Authentication:SecretKey"]!)),
                        ValidIssuer = configuration["Authentication:Issuer"],
                        ValidAudience = configuration["Authentication:Audience"],
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddHttpContextAccessor();

            return services;
        }

        private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("Database");
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);

                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.System);
                });
            });

            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            return services;
        }
        private static IServiceCollection AddService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("Email"));
            services.AddScoped<IAuthenticationServices, AuthenticationServices>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<EmailTemplateService>();
            services.AddScoped<IMailService, MailService>();
            services.AddScoped<IOtpGenerator, OtpGenerator>();
            services.AddScoped<IRedisOtpStore, RedisOtpStore>();
            services.AddTransient<IResetPasswordGenerator, ResetPasswordGenerator>();
            return services;
        }
        private static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IPostRepository, PostRepository>();      
            services.AddScoped<IPlayerProfileRepository, PlayerProfileRepository>();
            services.AddScoped<IVenueRepository, VenueRepository>();
            services.AddScoped<IFieldRepository, FieldRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IVenueWalletRepository, VenueWalletRepository>();
            services.AddScoped<IMatchRoomRepository, MatchRoomRepository>();
            services.AddScoped<IRoomParticipantRepository, RoomParticipantRepository>();
            services.AddScoped<IPostLikeRepository, PostLikeRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ICommentLikeRepository, CommentLikeRepository>();
            services.AddScoped<IFriendshipRepository, FriendshipRepository>();
            return services;
        }
        private static IServiceCollection AddFirebase(this IServiceCollection services)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile("firebase.json"),
            });
            return services;
        }
        private static IServiceCollection AddRedisStore(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")))
                    .AddTransient<IRedisOtpStore, RedisOtpStore>();
            return services;
        }
        private static IServiceCollection AddMinioStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MinioSettings>(configuration.GetSection(MinioSettings.SectionName));

            var minioSettings = configuration.GetSection(MinioSettings.SectionName).Get<MinioSettings>()!;

            services.AddSingleton<IMinioClient>(_ =>
            {
                var client = new MinioClient()
                    .WithEndpoint(minioSettings.Endpoint)
                    .WithCredentials(minioSettings.AccessKey, minioSettings.SecretKey);

                if (minioSettings.UseSSL)
                {
                    client.WithSSL();
                }

                return client.Build();
            });

            services.AddScoped<IStorageService, MinioStorageService>();

            return services;
        }

        private static IServiceCollection AddSignalRServices(this IServiceCollection services)
        {
            services.AddSignalR();
            services.AddSingleton<ConnectionMapping>();
            return services;
        }
    }
}
