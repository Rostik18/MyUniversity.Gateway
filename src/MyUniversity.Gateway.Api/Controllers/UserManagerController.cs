using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using MyUniversity.Gateway.Api.Constants;
using MyUniversity.Gateway.Api.Utils;
using MyUniversity.Gateway.Models.User;
using MyUniversity.Gateway.Models.UserManager.Role;
using MyUniversity.Gateway.Models.UserManager.User;

namespace MyUniversity.Gateway.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserManagerController : ControllerBase
    {
        private readonly ILogger<UserManagerController> _logger;
        private readonly User.UserClient _userClient;
        private readonly Role.RoleClient _roleClient;
        private readonly IMapper _mapper;

        public UserManagerController(
            ILogger<UserManagerController> logger,
            User.UserClient userClient,
            Role.RoleClient roleClient,
            IMapper mapper)
        {
            _logger = logger;
            _userClient = userClient;
            _mapper = mapper;
            _roleClient = roleClient;
        }

        /// <summary>
        /// Login into the system.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/login
        ///     {
        ///        "emailAddress": "Super.Admin@gmail.com",
        ///        "password": "Admin"
        ///     }
        ///
        /// </remarks>
        /// <param name="loginUserModel"></param>
        /// <returns></returns>
        [HttpPost("/api/login")]
        [AllowAnonymous]
        public async Task<object> LoginAsync([FromBody] LoginUserModel loginUserModel)
        {
            _logger.LogInformation($"Start to process {ProcessFlows.UserLogin} request");

            var requestModel = _mapper.Map<LoginRequest>(loginUserModel);

            try
            {
                var response = await _userClient.LoginUserAsync(requestModel, Metadata.Empty);

                _logger.LogInformation($"{ProcessFlows.UserLogin} request was processed successfully");

                return Ok(response);
            }
            catch (RpcException ex)
            {
                var httpStatusCode = StatusCodeConverter.FromGrpcToHttp(ex.StatusCode);

                _logger.LogWarning($"{ProcessFlows.UserLogin} request finished with error {ex.Status.Detail}, status code {httpStatusCode}");

                return Problem(ex.Status.Detail, statusCode: httpStatusCode);
            }
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <remarks>
        /// Only logged-in users can create new users.
        /// 
        /// SuperAdmin -> SuperAdmin, Service, UniversityAdmin, Teacher, Student.
        /// 
        /// Service -> SuperAdmin, Service, UniversityAdmin, Teacher, Student.
        /// 
        /// UniversityAdmin -> UniversityAdmin, Teacher, Student.
        /// 
        /// Teacher -> Student.
        ///
        /// TenantId is optional for SuperAdmin and Service accounts.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/registration
        ///     {
        ///         "firstName": "Ad",
        ///         "lastName": "Min",
        ///         "emailAddress": "user@example.com",
        ///         "phoneNumber": "+380 (50) 123 4567",
        ///         "tenantId": "11116258-9207-4bdc-b101-fb560cc8cb20",
        ///         "password": "12345678",
        ///         "roles": [
        ///             1,
        ///             3
        ///         ]
        ///     }
        ///
        /// </remarks>
        /// <param name="registerUserModel">New user model</param>
        /// <returns>Registration success</returns>
        [HttpPost("/api/registration")]
        [Authorize(Roles = "SuperAdmin,UniversityAdmin,Teacher")]
        public async Task<object> RegisterAsync([FromBody] RegisterUserModel registerUserModel)
        {
            _logger.LogInformation($"Start to process {ProcessFlows.UserRegistration} request");

            var requestModel = _mapper.Map<RegistrationRequest>(registerUserModel);
            var accessToken = Request.Headers[HeaderNames.Authorization];

            try
            {
                var response = await _userClient.RegisterUserAsync(
                    requestModel,
                    new Metadata
                    {
                        {HeaderKeys.AccessToken, accessToken.ToString()}
                    });

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

        /// <summary>
        /// Gives all available roles based on user access.
        /// </summary>
        /// <returns>available roles</returns>
        [HttpGet("/api/roles")]
        [AllowAnonymous]
        public async Task<object> GetRolesAsync()
        {
            _logger.LogInformation($"Start to process {ProcessFlows.GetRoles} request");

            var accessToken = Request.Headers[HeaderNames.Authorization];

            try
            {
                var response = await _roleClient.GetRolesAsync(
                    new RoleRequest(),
                    new Metadata
                    {
                        {HeaderKeys.AccessToken, accessToken.ToString()}
                    });

                var roles = response.Roles.Select(_mapper.Map<RoleModel>);

                _logger.LogInformation($"{ProcessFlows.GetRoles} request was processed successfully");

                return Ok(roles);
            }
            catch (RpcException ex)
            {
                var httpStatusCode = StatusCodeConverter.FromGrpcToHttp(ex.StatusCode);

                _logger.LogWarning($"{ProcessFlows.GetRoles} request finished with error {ex.Status.Detail}, status code {httpStatusCode}");

                return Problem(ex.Status.Detail, statusCode: httpStatusCode);
            }
        }

        /// <summary>
        /// Gives all available users based on user access.
        /// </summary>
        /// <remarks>
        /// Only logged-in users can get users.
        /// 
        /// SuperAdmin -> SuperAdmin, Service, UniversityAdmin, Teacher, Student.
        /// 
        /// Service -> SuperAdmin, Service, UniversityAdmin, Teacher, Student.
        /// 
        /// UniversityAdmin -> own university: UniversityAdmin, Teacher, Student.
        /// 
        /// Teacher -> own university: Teacher, Student.
        ///
        /// Sample request:
        /// 
        ///     GET /api/users
        /// 
        /// </remarks>
        /// <returns>all available users</returns>
        [HttpGet("/api/users")]
        [Authorize(Roles = "SuperAdmin,UniversityAdmin,Teacher")]
        public async Task<object> GetAllUsersAsync()
        {
            _logger.LogInformation($"Start to process {ProcessFlows.GetAllUsers} request");

            var accessToken = Request.Headers[HeaderNames.Authorization];

            try
            {
                var response = await _userClient.GetAllUsersAsync(
                    new GetUsersRequest(),
                    new Metadata
                    {
                        {HeaderKeys.AccessToken, accessToken.ToString()}
                    });

                var users = response.Users.Select(_mapper.Map<UserModel>);

                _logger.LogInformation($"{ProcessFlows.GetAllUsers} request was processed successfully");

                return Ok(users);
            }
            catch (RpcException ex)
            {
                var httpStatusCode = StatusCodeConverter.FromGrpcToHttp(ex.StatusCode);

                _logger.LogWarning($"{ProcessFlows.GetAllUsers} request finished with error {ex.Status.Detail}, status code {httpStatusCode}");

                return Problem(ex.Status.Detail, statusCode: httpStatusCode);
            }
        }
    }
}