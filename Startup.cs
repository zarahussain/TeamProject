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
using Swashbuckle.AspNetCore.Swagger;

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
      // Register the Swagger generator, defining one or more Swagger documents
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new Info { Title = "AdventureWords", Version = "v1" });
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app,
                          IHostingEnvironment env,
                          IOptions<AppSettings> settings)
    {
      if (env.IsDevelopment())
        app.UseDeveloperExceptionPage();

      // using static files with default files
      app.UseFileServer();
      // Enable middleware to serve generated Swagger as a JSON endpoint.
      app.UseSwagger();
      // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AdventureWords V1");
      });
      // using mvc
      app.UseMvcWithDefaultRoute();
      // finall request pipline fallback
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
