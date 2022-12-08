using AWSServerless_MVC.Repositories;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Patika.Framework.Shared.Entities;
using Microsoft.AspNetCore.Identity;
using Patika.Framework.Shared.Consts;
using Patika.Framework.Shared.Extensions;

namespace AWSServerless_MVC;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        AddConfiguration(services);
        services.AddRazorPages();
        //services.AddControllers();
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
    }
    private void AddConfiguration(IServiceCollection services)
    {
        var config = new Configuration();
        Configuration.GetSection(nameof(Configuration)).Bind(config);
        services.AddSingleton(config);
        LogWriterExtensions.ApplicationName = config.ApplicationName;
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        var authctx = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<AuthDbContext>();
        authctx.Database.Migrate();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapRazorPages();
        });
    }
}
