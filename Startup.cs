using System;
using System.Linq;
using AutoMapper;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Routine.Api.Data;
using Routine.Api.Services;

namespace Routine.Api
{
    /// <summary>
    /// ASP.NET先调用ConfigureServices方法，再调用Configure方法
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// IoC容器会注入配置文件
        /// </summary>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// 负责依赖注入（注册一些服务到IoC容器里）
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpCacheHeaders(expires =>
            {
                expires.MaxAge = 60;
                expires.CacheLocation = CacheLocation.Private;
            }, validation =>
            {
                validation.MustRevalidate = true;
            });

            // services.AddResponseCaching();

            //AddControllers(); 只使用Controller，不需要Views
            services.AddControllers(setup =>
            {
                setup.ReturnHttpNotAcceptable = true;
                setup.CacheProfiles.Add("120sCacheProfile", new CacheProfile
                {
                    Duration = 120
                });
            }).AddNewtonsoftJson(setup =>
                {
                    setup.SerializerSettings.ContractResolver = 
                        new CamelCasePropertyNamesContractResolver();
                }).AddXmlDataContractSerializerFormatters()
                .ConfigureApiBehaviorOptions(setup =>
                {
                    setup.InvalidModelStateResponseFactory = context =>
                    {
                        var problemDetails = new ValidationProblemDetails(context.ModelState)
                        {
                            Type = "http://www.baidu.com",
                            Title = "有错误！！！",
                            Status = StatusCodes.Status422UnprocessableEntity,
                            Detail = "请看详细信息",
                            Instance = context.HttpContext.Request.Path
                        };

                        problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);

                        return new UnprocessableEntityObjectResult(problemDetails)
                        {
                            ContentTypes = { "application/problem+json" }
                        };
                    };
                });

            services.Configure<MvcOptions>(config =>
            {
                var newtonSoftJsonOutputFormatter =
                    config.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();

                newtonSoftJsonOutputFormatter?.SupportedMediaTypes.Add("application/vnd.company.hateoas+json");
            });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            //每次HTTP请求，会建立一次新的服务实例
            //即一个HTTP请求，则会连接一次数据库
            services.AddScoped<ICompanyRepository, CompanyRepository>();

            //添加一个数据库连接，也需要注册服务
            //但这有提供直接的方法，即AddDbContent
            services.AddDbContext<RoutineDbContext>(option =>
            {
                option.UseSqlite("Data Source=routine.db");
            });

            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();
        }

        /// <summary>
        /// 配置HTTP处理管道中的中间件
        /// 中间件将按顺序插入，注册的顺序是非常重要的
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())    //是否为开发环境。即环境变量ASPNETCORE_ENVIRONMENT=Development
            {
                app.UseDeveloperExceptionPage();
                /*使用“开发者异常页面”中间件，这个中间件的用处：
                如果在开发的时候，如果程序发现了一些异常，并且异常没有被处理
                就会把这个系统展示到一个页面里，这个页面里有这个错误的详细信息
                 */
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("Unexpected Error!");
                    });
                });
            }

            // app.UseResponseCaching();

            app.UseHttpCacheHeaders();

            /*
             * 路由中间件
             * 1. 将请求进行路由，标记此次URL是被谁注册的（有可能是MVC注册的，也有可能是Razor Pages注册的）
             * 2. Routing和Endpoints将决定如何把HTTP请求，分流到特定的Action上
             */
            app.UseRouting();

            /*
             * 检查用户授权
             * 用户授权一般在ConfigureServices中完成的，如果没有设置则不会起作用
             */
            app.UseAuthorization();

            /*
             * 终结点中间件
             */
            app.UseEndpoints(endpoints => //使用匿名函数对endpoints进行配置
            {
                endpoints.MapControllers(); //根据Controller的Attribute来路由
            });
        }
    }
}
