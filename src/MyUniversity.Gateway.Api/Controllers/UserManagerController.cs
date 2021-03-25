using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyUniversity.Gateway.Api.Constants;
using MyUniversity.Gateway.Api.Utils;
using MyUniversity.Gateway.Models.UserManager.User;

namespace MyUniversity.Gateway.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserManagerController : ControllerBase
    {
        private readonly ILogger<UserManagerController> _logger;
        private readonly User.UserClient _userManagerClient;
        private readonly IMapper _mapper;

        public UserManagerController(
            ILogger<UserManagerController> logger,
            User.UserClient userManagerClient,
            IMapper mapper)
        {
            _logger = logger;
            _userManagerClient = userManagerClient;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<object> RegisterAsync([FromBody] RegisterUserModel registerUserModel)
        {
            _logger.LogInformation($"Start to process {ProcessFlows.UserRegistration} request");

            var requestModel = _mapper.Map<RegistrationRequest>(registerUserModel);

            try
            {
                var response = await _userManagerClient.RegisterUserAsync(requestModel, Metadata.Empty);

                _logger.LogInformation($"{ProcessFlows.UserRegistration} request was processed successfully");

                return Ok(response);
            }
            catch (RpcException ex)
            {
                var httpStatusCode = StatusCodeConverter.FromGrpcToHttp(ex.StatusCode);

                _logger.LogWarning($"{ProcessFlows.UserRegistration} request finished with error {ex.Status.Detail}, status code {httpStatusCode}");

                return Problem(ex.Status.Detail, statusCode: httpStatusCode);
            }
        }
    }
}