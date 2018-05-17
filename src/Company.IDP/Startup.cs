using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Company.IDP.Entities;
using Company.IDP.Services;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Company.IDP
{
    public class Startup
    {
        public static IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public X509Certificate2 LoadCertificateFromStore()
        {
            var thumbPrint = "30c991845afd9b89233eea8eabcd36e0f3f6d276";

            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                var certCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbPrint, true);
                if (certCollection.Count == 0)
                {
                    throw new Exception("The specified certificate wasn't found.");
                }

                return certCollection[0];
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration["connectionStrings:userDbConnectionString"];
            services.AddDbContext<UserContext>(o => o.UseSqlServer(connectionString));

            services.AddScoped<IUserRepository, UserRepository>();

            var identityServerDataDbConnectionString = Configuration["connectionStrings:identityServerDataDbConnectionString"];

            var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddMvc();

            services.AddIdentityServer()
                .AddSigningCredential(LoadCertificateFromStore())
                //.AddTemporarySigningCredential()
                //.AddTestUsers(Config.GetUsers())
                .AddCompanyUserStore()
                .AddConfigurationStore(builder =>
                    builder.UseSqlServer(identityServerDataDbConnectionString,
                    options => options.MigrationsAssembly(migrationAssembly)))
                .AddOperationalStore(builder =>
                    builder.UseSqlServer(identityServerDataDbConnectionString,
                    options => options.MigrationsAssembly(migrationAssembly)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory, UserContext userContext, ConfigurationDbContext configurationDbContext, PersistedGrantDbContext persistedGrantDbContext)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            configurationDbContext.Database.Migrate();
            configurationDbContext.EnsureSeedDataForContext();

            persistedGrantDbContext.Database.Migrate();

            userContext.Database.Migrate();
            userContext.EnsureSeedDataForContext();

            app.UseIdentityServer();

            app.UseFacebookAuthentication(new FacebookOptions
            {
                AuthenticationScheme = "Facebook",
                DisplayName = "Facebook",
                SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                AppId = "457627938026038",
                AppSecret = "20744c09c5e8f24a678527eb66b4ba6d"
            });

            app.UseStaticFiles();

            app.UseMvcWithDefaultRoute();
        }
    }
}
