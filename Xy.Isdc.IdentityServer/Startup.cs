using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xy.Isdc.IdentityServer.CustomProvider;
using Xy.Isdc.IdentityServer.Data;
using Xy.Isdc.IdentityServer.Models;
using Xy.Isdc.IdentityServer.OAuth;
using Xy.Isdc.IdentityServer.Services;
using Microsoft.AspNetCore.Http;

namespace Xy.Isdc.IdentityServer
{
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(
                        options =>
                        {
                            // 配置身份选项
                            // 密码配置
                            options.Password.RequireDigit = false; //是否需要数字(0-9).
                            options.Password.RequiredLength = 6; //设置密码长度最小为6
                            options.Password.RequireNonAlphanumeric = false; //是否包含非字母或数字字符。
                            options.Password.RequireUppercase = false; //是否需要大写字母(A-Z).
                            options.Password.RequireLowercase = false; //是否需要小写字母(a-z).

                            // 锁定设置
                            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30); //账户锁定时长30分钟
                            options.Lockout.MaxFailedAccessAttempts = 10; //10次失败的尝试将账户锁定
                        })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddIdentityServer()
            #region 添加IdentityServer4 认证证书相关处理  By Liyouming 2017-11-29

                //AddSigningCredential 添加登录证书 这个是挂到IdentityServer4中间件上  提供多种证书处理  RsaSecurityKey\SigningCredentials
                //这里可以采用IdentiServe4的证书封装出来
                //添加一个签名密钥服务，该服务将指定的密钥材料提供给各种令牌创建/验证服务。您可以从证书存储中传入X509Certificate2一个SigningCredential或一个证书引用
                //.AddSigningCredential(new System.Security.Cryptography.X509Certificates.X509Certificate2()
                //{
                //    Archived = true,
                //    FriendlyName = "",
                //    PrivateKey = System.Security.Cryptography.AsymmetricAlgorithm.Create("key")
                //})
                //AddDeveloperSigningCredential在启动时创建临时密钥材料。这是仅用于开发场景，当您没有证书使用。
                //生成的密钥将被保存到文件系统，以便在服务器重新启动之间保持稳定（可以通过传递来禁用false）。
                //这解决了在开发期间client / api元数据缓存不同步的问题

                .AddDeveloperSigningCredential()
                //添加验证令牌的密钥。它们将被内部令牌验证器使用，并将显示在发现文档中。
                //您可以从证书存储中传入X509Certificate2一个SigningCredential或一个证书引用。这对于关键的转换场景很有用
                //.AddValidationKeys(new AsymmetricSecurityKey[] {

                //}) 

            #endregion
            #region 添加对IdentityServer4 EF数据库持久化支持 By Liyouming 2017-11-29

                //黎又铭 Add 2017-11-28 添加IdentityServer4对EFCore数据库的支持
                // this adds the config data from DB (clients, resources)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                //  // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                    {
                        builder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                            builderoptions => { builderoptions.MigrationsAssembly(migrationsAssembly); });
                    };

                    options.EnableTokenCleanup = true; //允许对Token的清理
                    options.TokenCleanupInterval = 1800; //清理周期时间Secends
                })
                .AddAspNetIdentity<ApplicationUser>()
                .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
                .AddProfileService<ProfileService>();

            #endregion




            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            InitializeDatabase(app);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }



            app.UseStaticFiles();

            // app.UseAuthentication(); // not needed, since UseIdentityServer adds the authentication middleware
            app.UseIdentityServer();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }



        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in Config.GetClients())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.GetApiResources())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}