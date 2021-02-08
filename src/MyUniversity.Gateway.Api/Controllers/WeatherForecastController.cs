using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyUniversity.Gateway.Api.Configs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyUniversity.Gateway.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly MessageClientConfigs _messageClientConfigs;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, MessageClientConfigs messageClientConfigs)
        {
            _logger = logger;
            _messageClientConfigs = messageClientConfigs;
        }

        [HttpGet]
        public MessageClientConfigs Get()
        {
            return _messageClientConfigs;
        }
    }
}
