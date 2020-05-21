using System.Text;
using cwiczenie3.DAL;
using cwiczenie3.Middleware;
using cwiczenie3.Models;
using cwiczenie3.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace cwiczenie3
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
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer =  true,
                        ValidateAudience = true,
                        ValidIssuer = "Gakko",
                        ValidAudience = "Students",
                        ValidateLifetime =  true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]))
                    };
                });
            services.AddTransient<IStudentsDbService, SqlServerDbService>();
            services.AddSingleton<IDbService, MockDbService>();
            services.AddControllers();
            services.AddSwaggerGen( config =>
            {
                config.SwaggerDoc("doc", new OpenApiInfo { Title = "STUDENT API", Version = "0.1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IStudentsDbService service)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseMiddleware<LoggingMiddleware>();

           
            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "STUDENT API");
            });

            app.UseMiddleware<LoggingMiddleware>();

            app.Use(async (context, next) =>
            {
                if (!context.Request.Headers.ContainsKey("Index"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("No index number present on headers");
                    return;
                }
                string index = context.Request.Headers["Index"].ToString();

                string checkIndex = service.GetStudent(index);

                if (checkIndex == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("No such student");
                    return;
                }

                await next();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}