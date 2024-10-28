using ForpostModbusTcpPoller.Hubs;
using ForpostModbusTcpPoller.Services;

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
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors(_corsPolicyName);
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ModbusHub>("/modbusHub");
            });
        }
    }
}