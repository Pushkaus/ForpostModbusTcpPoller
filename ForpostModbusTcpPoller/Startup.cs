using ForpostModbusTcpPoller.Hubs;
using ForpostModbusTcpPoller.Services;
using Microsoft.OpenApi.Models;

namespace ForpostModbusTcpPoller
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly string _corsPolicyName = "CorsPolicy";

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddCors(options =>
            {
                options.AddPolicy(_corsPolicyName, policy =>
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });
            services.AddSignalR();
            services.AddSingleton<DeviceManagerService>();
            services.AddSingleton<ModbusPollerService>();
            services.AddHostedService<ModbusPollingHostedService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DeviceManagerService deviceManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty; 
            });
        }
    }
}