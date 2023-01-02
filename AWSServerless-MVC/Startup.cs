using AWSServerless_MVC.Repositories;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Patika.Framework.Shared.Consts;
using Patika.Framework.Shared.Extensions;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using AWSServerless_MVC.Interfaces.Repositories;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using AWSServerless_MVC.Entities;
using Patika.Framework.Shared.Interfaces;
using Patika.Framework.Shared.Entities;
using ApplicationUser = AWSServerless_MVC.Models.ApplicationUser;
using AWSServerless_MVC.DbContexts;
using AWSServerless_MVC.Services;
using AWSServerless_MVC.Interfaces.Services;
using Microsoft.IdentityModel.Tokens;

namespace AWSServerless_MVC;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }
    public AuthConfiguration AuthConfiguration { get; set; }

    public void ConfigureServices(IServiceCollection services)
    {
        AddConfiguration(services);
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        //DYNMODB
        var awsOptions = Configuration.GetAWSOptions();
        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<IAmazonDynamoDB>();
        services.AddScoped<IDynamoDBContext, DynamoDBContext>();

        //MYSQL
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddDbContext<ApplicationDbContext>((sp, opt) =>
        {
            var connectionString = sp.GetService<Configuration>().RDBMSConnectionStrings.Single(m => m.Name.Equals(DbConnectionNames.Main)).FullConnectionString;
            opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }, ServiceLifetime.Scoped);

        //IDENTITY
        services.AddScoped<AuthDbContext>();
        services.AddScoped<IdentityDbContext<ApplicationUser>, AuthDbContext>();
        services.AddDbContext<AuthDbContext>((sp, opt) =>
        {
            var connectionString = sp.GetService<Configuration>().RDBMSConnectionStrings.Single(m => m.Name.Equals(DbConnectionNames.Main)).FullConnectionString;
            opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }, ServiceLifetime.Scoped);

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
        })
          .AddRoles<IdentityRole>()
          .AddEntityFrameworkStores<AuthDbContext>()
          .AddDefaultTokenProviders();

        //OKTA
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = AuthConfiguration.JWT.RequireHttpsMetadata;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidIssuer = AuthConfiguration.JWT.ValidIssuer,
                ValidateAudience = AuthConfiguration.JWT.ValidateAudience,
                ValidateLifetime = AuthConfiguration.JWT.ValidateLifetime,
                ValidateIssuerSigningKey = AuthConfiguration.JWT.ValidateIssuerSigningKey,
                ClockSkew = TimeSpan.Zero,
                ValidAudience = AuthConfiguration.JWT.ValidAudience,
                ValidateIssuer = AuthConfiguration.JWT.ValidateIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(AuthConfiguration.JWT.Secret)),
                RequireExpirationTime = AuthConfiguration.JWT.RequireExpirationTime,
            };
        })
        .AddOpenIdConnect(options =>
        {
            options.ClientId = Configuration.GetValue<string>("Okta:ClientId");
            options.ClientSecret = Configuration.GetValue<string>("Okta:ClientSecret");
            options.Authority = Configuration.GetValue<string>("Okta:Issuer");
            options.CallbackPath = "/callback";
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.SaveTokens = true;
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters.ValidateIssuer = false;
            options.TokenValidationParameters.NameClaimType = "name";
        });
        services.AddScoped<IMigrationStep, AuthDefaultDataMigration>();

        services.AddScoped<IUserRefreshTokenRepository, UserRefreshTokenRepository>(); 
        services.AddScoped<IIdentityApplicationService, IdentityApplicationService>();
        services.AddScoped<ITokenHandlerService, TokenHandlerService>();
    }
    private void AddConfiguration(IServiceCollection services)
    {
        var config = new Configuration(); 
        Configuration.GetSection(nameof(Configuration)).Bind(config);
        services.AddSingleton(config);

        AuthConfiguration = new AuthConfiguration();
        Configuration.GetSection(nameof(AuthConfiguration)).Bind(AuthConfiguration);
        services.AddSingleton(AuthConfiguration);
        LogWriterExtensions.ApplicationName = config.ApplicationName;
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        Migrate(app);

        app.UseDeveloperExceptionPage();

        app.UseForwardedHeaders();

        app.UseHttpsRedirection();

        app.UseSwagger();

        if (env.IsDevelopment())
        {
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });
        }
        else
        {
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/Prod/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });
        }

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
    private static void Migrate(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var migrator = services.GetRequiredService<IMigrationStep>(); //AuthDbContext
            migrator.EnsureMigrationAsync().GetAwaiter().GetResult();

            var ctx = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
            ctx.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }
}
