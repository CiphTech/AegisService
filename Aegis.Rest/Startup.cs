using System;
using System.IO;
using System.Text;
using Aegis.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Aegis.Rest
{
    public class Startup
    {
        public Startup(IHostEnvironment environment)
        {
            var builder = new ConfigurationBuilder();

            Configuration = builder.SetBasePath(environment.ContentRootPath)
                .AddEnvironmentVariables()
                .Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            const string key = "0EB6F44F-B0E0-47E9-A7BC-BD2460ACD468";

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "AEGISAPP",
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key))
                };
            });
            services.AddAuthorization();
            
            services.AddCors(options => options.AddPolicy("AllowLocalhost", 
                builder =>
                {
                    builder.WithOrigins("http://localhost:4200");
                }));
            
            services.AddControllers();
            services.AddSingleton<IAegisService, InMemoryAegisService>();
            services.AddSingleton<IAegisInitPersonsProvider, DummyInitPersonsProvider>();
            services.AddSingleton<IAegisMessageStorage, DummyMessageStorage>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("AllowLocalhost");
            
            var provider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "ClientApp"));
            var defOpt = new DefaultFilesOptions{FileProvider = provider};
            
            app.UseDefaultFiles(defOpt);
            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}