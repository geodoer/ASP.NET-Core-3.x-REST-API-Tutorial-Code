using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Routine.Api.Data;

namespace Routine.Api
{
    public class Program
    {
        //原先是一个dotnet core控制台项目
        public static void Main(string[] args)
        {
            //这是一个外部项目，需要一个宿主
            var host = CreateHostBuilder(args)
                .Build();   //Build一个ASP.NET Core应用（控制台项目就变成了ASP.NET Core应用）

            using (var scope = host.Services.CreateScope()) //创建一个服务的范围
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetService<RoutineDbContext>(); 
                    //这里不能使用构造函数的方式获取，可以通过此方法从IoC容器中拿出服务

                    dbContext.Database.EnsureDeleted(); //每次运行时，把数据库给删了
                    dbContext.Database.Migrate();       //删完之后把数据库迁移一下
                    //这句完成之后，就会创建routine.db文件
                }
                catch (Exception e)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>(); //获得日志
                    logger.LogError(e, "Database Migration Error!");
                }
            }

            host.Run(); //运行ASP.NET Core应用
        }

        //配置ASP.NET Core应用，并返回一个宿主Builder接口
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args) //创建一个默认的Builder
                //配置WebHost默认值
                //如，配置ASP.NET如何处理配置文件、外部应用服务器、路由等
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //Startup内也是一些配置，但它相对与配置文件比较动态
                    //此句即注册Startup，并调用它内部的方法，以对此应用进行配置
                    webBuilder.UseStartup<Startup>();
                });
    }
}
