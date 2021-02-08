using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyUniversity.Gateway.Services.MessageClient;
using MyUniversity.Gateway.Services.Configs;

namespace MyUniversity.Gateway.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IMessageClient _messageClient;

        private readonly MessageClientConfigs _messageClientConfigs;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IMessageClient messageClient, MessageClientConfigs messageClientConfigs)
        {
            _logger = logger;
            _messageClient = messageClient;

            _messageClientConfigs = messageClientConfigs;
        }

        [HttpGet]
        public MessageClientConfigs Get()
        {
            var a = _messageClient.RequestAsync<int, int>("rpc_queue", 5).Result;

            return _messageClientConfigs;
        }
    }
}
