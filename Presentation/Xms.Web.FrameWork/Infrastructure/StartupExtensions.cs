﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Xms.Core;
using Xms.Identity;
using Xms.Web.Framework.Middlewares;

namespace Xms.Web.Framework.Infrastructure
{
    public static class StartupExtensions
    {
        public static void AddWebDefaults(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvc(options =>
            {
                options.MaxModelValidationErrors = 50;
                //禁用action中对参数的验证
                options.AllowValidatingTopLevelNodes = false;
                //自定义异常过滤
                //options.Filters.Add(typeof(ApiExceptionFilterAttribute));
            })
            //全局配置Json序列化处理
            .AddJsonOptions(options =>
            {
                //忽略循环引用
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                //不使用驼峰样式的key
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                //设置时间格式
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd";
                //忽略空值
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddDistributedMemoryCache();
            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            //允许的字符范围
            services.AddWebEncoders((o) => {
                o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });
            //注册Cookie认证服务
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = XmsAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = XmsAuthenticationDefaults.ExternalAuthenticationScheme;
            }).AddCookie(XmsAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = $"{XmsCookieDefaults.Prefix}{XmsCookieDefaults.AuthenticationCookie}";
                options.Cookie.HttpOnly = true;
                options.LoginPath = XmsAuthenticationDefaults.LoginPath;
                options.AccessDeniedPath = XmsAuthenticationDefaults.AccessDeniedPath;

                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            //注册所有业务服务类
            services.RegisterAll(configuration);
        }

        public static void UseWebDefaults(this IApplicationBuilder app)
        {
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseExceptionHandlerMiddleWare();
            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "org",
                                template: "{org}/{controller=Home}/{action=Index}");
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(
                    "home",
                    "{org}/index",
                                  new { controller = "home", action = "index" });
                routes.MapRoute("error",
                                "error/{action}",
                                new { controller = "error", action = "index" });
                routes.MapRoute(
                 name: "customize",
                 template: "{org}/{area:exists}/{controller=Home}/{action=Index}/{id?}"
               );
                routes.MapRoute(
                    "customize_home",
                    "{org}/customize",
                                  new { area = "customize", controller = "home", action = "index" });
                routes.MapRoute(
                    "customize_home_index",
                    "{org}/customize/index",
                                  new { area = "customize", controller = "home", action = "index" });
            });
        }
    }
}
