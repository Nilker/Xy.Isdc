using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Xy.Isdc.WebApi
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
            services.AddMvc();

            /*
             * 方法一：add by lhl 2018年1月24日10:00:09
             * nuget :IdentityServer4.AccessTokenValidation
             */
            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(s =>
                {
                    s.Authority = "http://localhost:5000";//IdentityServer地址
                    s.RequireHttpsMetadata = false; //是否https请求

                    s.ApiName = "api1";//api范围
                });

            /*
             * 方法二：add by lhl
             * 这里使用了Microsoft.AspNetCore.Authentication.JwtBearer包来替换AccessTokenValidation，因为后者还没有更新到.net core 2.0，使用的话，是有问题的
             *  services.AddAuthentication((options) =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters();
            options.RequireHttpsMetadata = false;
            options.Audience = "api1";//api范围
            options.Authority = "http://localhost:5000";//IdentityServer地址
        });
             */
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //add by lhl  您还需要将中间件添加到管道中。它必须在MVC之前添加：
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
