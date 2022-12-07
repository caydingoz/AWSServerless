using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using AWSServerless1.Repositories;
using Patika.Framework.Shared.Consts;
using Patika.Framework.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using AWSServerless1.Interfaces.Repositories;
using Patika.Framework.Shared.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Okta.AspNetCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace AWSServerless1;
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        AddConfiguration(services);
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

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
        })
          .AddRoles<IdentityRole>()
          .AddEntityFrameworkStores<AuthDbContext>()
          .AddDefaultTokenProviders();
        //OKTA
        //services.AddAuthentication(options =>
        //{
        //    options.DefaultAuthenticateScheme = OktaDefaults.ApiAuthenticationScheme;
        //    options.DefaultChallengeScheme = OktaDefaults.ApiAuthenticationScheme;
        //    options.DefaultSignInScheme = OktaDefaults.ApiAuthenticationScheme;
        //})
        //.AddOktaWebApi(OpenIdConnectDefaults.AuthenticationScheme, new OktaWebApiOptions()
        //{
        //    OktaDomain = Configuration["Okta:Domain"],
        //    //AuthorizationServerId = Configuration["Okta:AuthorizationServerId"],
        //    //Audience = Configuration["Okta:ClientId"],
            
        //});
        //.AddOpenIdConnect(options =>
        //{
        //    options.ClientId = Configuration.GetValue<string>("Okta:ClientId");
        //    options.ClientSecret = Configuration.GetValue<string>("Okta:ClientSecret");
        //    options.CallbackPath = "/authorization-code/callback";
        //    options.Authority = Configuration.GetValue<string>("Okta:Issuer");
        //    options.ResponseType = "code";
        //    options.SaveTokens = true;
        //    options.Scope.Add("openid");
        //    options.Scope.Add("profile");
        //    options.Scope.Add("email");
        //    options.RequireHttpsMetadata = false;
        //    options.TokenValidationParameters.ValidateIssuer = false;
        //    options.TokenValidationParameters.NameClaimType = "name";
        //});
        //.AddOktaMvc(new OktaMvcOptions
        //{
        //    OktaDomain = Configuration.GetValue<string>("Okta:Domain"),
        //    ClientId = Configuration.GetValue<string>("Okta:ClientId"),
        //    ClientSecret = Configuration.GetValue<string>("Okta:ClientSecret"),
        //    Scope = new List<string> { "openid", "profile", "email" },
        //});
    }
    private void AddConfiguration(IServiceCollection services)
    {
        var config = new Configuration();
        Configuration.GetSection(nameof(Configuration)).Bind(config);
        services.AddSingleton(config);
        LogWriterExtensions.ApplicationName = config.ApplicationName;
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseDeveloperExceptionPage();

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

        var ctx = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        ctx.Database.Migrate();
        var authctx = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<AuthDbContext>();
        authctx.Database.Migrate();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
            });
        });
    }
}