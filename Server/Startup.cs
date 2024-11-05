using ForpostModbusTcpPoller.Database;
using ForpostModbusTcpPoller.Hubs;
using ForpostModbusTcpPoller.Services;
using Microsoft.EntityFrameworkCore;

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
            services.AddDbContextFactory<ApplicationDbContext>(options =>
                options.UseSqlite("Data Source=devices.db"));

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddCors(options =>
            {
                options.AddPolicy(_corsPolicyName, policy =>
                    policy.AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed((host) => true)
                        .AllowCredentials());
            });
            services.AddSignalR();
            services.AddSingleton<DeviceManagerService>();
            services.AddSingleton<ModbusPollerService>();
            services.AddHostedService<ModbusPollingHostedService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DeviceManagerService deviceManager,
            IServiceScopeFactory scopeFactory)
        {
            app.UseCors(_corsPolicyName);

            app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseAuthorization();
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ModbusHub>("/modbusHub");
                endpoints.MapFallbackToFile("index.html");
            });
            
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty; 
            });
        }
    }
}