// Backend/HostedServices/ModbusPollingHostedService.cs

namespace ForpostModbusTcpPoller.Services
{
    public class ModbusPollingHostedService : BackgroundService
    {
        private readonly ModbusPollerService _pollerService;
        private readonly ILogger<ModbusPollingHostedService> _logger;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);

        public ModbusPollingHostedService(ModbusPollerService pollerService, ILogger<ModbusPollingHostedService> logger)
        {
            _pollerService = pollerService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Фоновая служба опроса начата.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Начинаю опрос устройств.");
                    await _pollerService.PollDevicesAsync();
                    _logger.LogInformation("Опрос устройств завершен.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при опросе устройств.");
                }

                try
                {
                    await Task.Delay(_pollingInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Игнорируем, если задача была отменена
                }
            }

            _logger.LogInformation("Фоновая служба опроса остановлена.");
        }
    }
}