using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
            var requestModel = _mapper.Map<RegistrationRequest>(registerUserModel);

            return await _userManagerClient.RegisterUserAsync(requestModel);
        }

    }
}
