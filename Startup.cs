using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AdventureWorks
{
   public class Startup
   {
      public IConfiguration Config { get; private set; }
      public Startup(IConfiguration config)
      {
         Config = config;
      }
      // This method gets called by the runtime. Use this method to add services to the container.
      // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
      public void ConfigureServices(IServiceCollection services)
      {
         // adding appSetting to DI
         services.Configure<AppSettings>(Config.GetSection("AppSettings"));
         // adding db context to DI
         services.AddDbContext<AdventureWorksContext>();
         // adding MVC
         services.AddMvc()
                 .AddJsonOptions(options =>
                  {
                     options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                  });
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app,
                            IHostingEnvironment env,
                            IOptions<AppSettings> settings)
      {
         if (env.IsDevelopment())
         {
            app.UseDeveloperExceptionPage();
         }
         // using static files with default files
         app.UseFileServer();
         // using mvc
         app.UseMvcWithDefaultRoute();

         app.Run(async (context) =>
         {
            await context.Response.WriteAsync($"SqlServerConnection: {settings.Value.SqlServerConnection}");
         });
      }

      public class AppSettings
      {
         public string SqlServerConnection { get; set; }
      }
   }
}
