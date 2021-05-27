using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
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
using MyUniversity.Gateway.Models.UserManager.University;
using MyUniversity.Gateway.Models.UserManager.User;
using MyUniversity.Gateway.Role;
using MyUniversity.Gateway.University;
using MyUniversity.Gateway.User;

namespace MyUniversity.Gateway.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserManagerController : ControllerBase
    {
        private readonly ILogger<UserManagerController> _logger;
        private readonly User.User.UserClient _userClient;
        private readonly Role.Role.RoleClient _roleClient;
        private readonly University.University.UniversityClient _universityClient;
        private readonly IMapper _mapper;

        public UserManagerController(
            ILogger<UserManagerController> logger,
            User.User.UserClient userClient,
            Role.Role.RoleClient roleClient,
            University.University.UniversityClient universityClient,
            IMapper mapper)
        {
            _logger = logger;
            _userClient = userClient;
            _mapper = mapper;
            _roleClient = roleClient;
            _universityClient = universityClient;
        }

        #region User

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
        public async Task<IActionResult> LoginAsync([FromBody] LoginUserModel loginUserModel)
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
                return HandleError(ex, ProcessFlows.UserLogin);
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
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterUserModel registerUserModel)
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
                return HandleError(ex, ProcessFlows.UserRegistration);
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
        public async Task<IActionResult> GetAllUsersAsync()
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
                return HandleError(ex, ProcessFlows.GetAllUsers);
            }
        }

        /// <summary>
        /// Gives user by id based on user access.
        /// </summary>
        /// <remarks>
        /// Only logged-in users can get user.
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
        ///     GET /api/user/1
        /// 
        /// </remarks>
        /// <returns>Available user by id</returns>
        [HttpGet("/api/users/{id}")]
        [Authorize(Roles = "SuperAdmin,UniversityAdmin,Teacher")]
        public async Task<IActionResult> GetUserByIdAsync([Range(1, int.MaxValue)] int id)
        {
            _logger.LogInformation($"Start to process {ProcessFlows.GetUserById} request");

            var accessToken = Request.Headers[HeaderNames.Authorization];

            try
            {
                var response = await _userClient.GetUserByIdAsync(
                    new GetUserRequest { Id = id },
                    new Metadata
                    {
                        {HeaderKeys.AccessToken, accessToken.ToString()}
                    });

                var user = _mapper.Map<UserModel>(response);

                _logger.LogInformation($"{ProcessFlows.GetUserById} request was processed successfully");

                return Ok(user);
            }
            catch (RpcException ex)
            {
                return HandleError(ex, ProcessFlows.GetUserById);
            }
        }

        /// <summary>
        /// Update user by id based on user access.
        /// </summary>
        /// <remarks>
        /// Only logged-in users can update user.
        /// 
        /// SuperAdmin -> SuperAdmin, Service, UniversityAdmin, Teacher, Student.
        /// 
        /// Service -> SuperAdmin, Service, UniversityAdmin, Teacher, Student.
        /// 
        /// UniversityAdmin -> own university: UniversityAdmin, Teacher, Student.
        /// 
        /// Teacher -> own university: Teacher, Student.
        ///
        /// Student -> himself.
        /// 
        /// Sample request:
        /// 
        ///     PUT /api/user/1
        /// 
        /// </remarks>
        /// <returns>Updated user</returns>
        [HttpPut("/api/users/{id}")]
        [Authorize(Roles = "SuperAdmin,UniversityAdmin,Teacher,Student")]
        public async Task<IActionResult> UpdateUserAsync([Range(1, int.MaxValue)] int id, [FromBody] UpdateUserModel updateModel)
        {
            _logger.LogInformation($"Start to process {ProcessFlows.UpdateUser} request");

            var accessToken = Request.Headers[HeaderNames.Authorization];

            var updateUserRequest = _mapper.Map<UpdateUserRequest>(updateModel);
            updateUserRequest.Id = id;

            try
            {
                var response = await _userClient.UpdateUserAsync(
                    updateUserRequest,
                    new Metadata
                    {
                        {HeaderKeys.AccessToken, accessToken.ToString()}
                    });

                var user = _mapper.Map<UserModel>(response);

                _logger.LogInformation($"{ProcessFlows.UpdateUser} request was processed successfully");

                return Ok(user);
            }
            catch (RpcException ex)
            {
                return HandleError(ex, ProcessFlows.UpdateUser);
            }
        }

        /// <summary>
        /// Soft delete user by id based on user access.
        /// </summary>
        /// <remarks>
        /// Only logged-in users can delete user.
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
        ///     Delete /api/user/1
        ///
        ///     {
        ///         "id": 7,
        ///         "firstName": "test",
        ///         "lastName": "test",
        ///         "emailAddress": "testUA@example.com",
        ///         "phoneNumber": "+380 (50) 123 4566",
        ///         "universityId": "11116258-9207-4bdc-b101-fb560cc8cb20",
        ///         "password": "Admin",
        ///         "roles": [
        ///             4
        ///         ]
        ///     }
        /// 
        /// </remarks>
        /// <returns>Deleting success</returns>
        [HttpDelete("/api/users/{id}")]
        [Authorize(Roles = "SuperAdmin,UniversityAdmin,Teacher")]
        public async Task<IActionResult> SoftDeleteUserAsync([Range(1, int.MaxValue)] int id)
        {
            _logger.LogInformation($"Start to process {ProcessFlows.SoftDeleteUser} request");

            var accessToken = Request.Headers[HeaderNames.Authorization];

            try
            {
                var response = await _userClient.SoftDeleteUserAsync(
                    new DeleteUserRequest { Id = id },
                    new Metadata
                    {
                        {HeaderKeys.AccessToken, accessToken.ToString()}
                    });

                _logger.LogInformation($"{ProcessFlows.SoftDeleteUser} request was processed successfully");

                return Ok(response);
            }
            catch (RpcException ex)
            {
                return HandleError(ex, ProcessFlows.SoftDeleteUser);
            }
        }

        /// <summary>
        /// Hard delete user by id based on user access.
        /// </summary>
        /// <remarks>
        /// Only logged-in users can delete user.
        /// Only soft deleted users can completely deleted.
        /// 
        /// SuperAdmin -> SuperAdmin, Service, UniversityAdmin, Teacher, Student.
        /// 
        /// Service -> SuperAdmin, Service, UniversityAdmin, Teacher, Student.
        /// 
        /// Sample request:
        /// 
        ///     Delete /api/user/1/hard
        /// 
        /// </remarks>
        /// <returns>Deleting success</returns>
        [HttpDelete("/api/users/{id}/hard")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> HardDeleteUserAsync([Range(1, int.MaxValue)] int id)
        {
            _logger.LogInformation($"Start to process {ProcessFlows.HardDeleteUser} request");

            var accessToken = Request.Headers[HeaderNames.Authorization];

            try
            {
                var response = await _userClient.HardDeleteUserAsync(
                    new DeleteUserRequest { Id = id },
                    new Metadata
                    {
                        {HeaderKeys.AccessToken, accessToken.ToString()}
                    });

                _logger.LogInformation($"{ProcessFlows.HardDeleteUser} request was processed successfully");

                return Ok(response);
            }
            catch (RpcException ex)
            {
                return HandleError(ex, ProcessFlows.HardDeleteUser);
            }
        }
        #endregion

        #region Role

        /// <summary>
        /// Gives all available roles based on user access.
        /// </summary>
        /// <returns>available roles</returns>
        [HttpGet("/api/roles")]
        [Authorize(Roles = "SuperAdmin,UniversityAdmin,Teacher")]
        public async Task<IActionResult> GetRolesAsync()
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
                return HandleError(ex, ProcessFlows.GetRoles);
            }
        }

        #endregion

        #region University

        /// <summary>
        /// Create new university.
        /// </summary>
        /// <remarks>
        /// Only for SuperAdmin
        /// </remarks>
        /// <returns>created university</returns>
        [HttpPost("/api/universities")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateUniversityAsync([FromBody] CreateUniversityModel model)
        {
            _logger.LogInformation($"Start to process {ProcessFlows.CreateUniversity} request");

            var accessToken = Request.Headers[HeaderNames.Authorization];

            try
            {
                var requestModel = _mapper.Map<CreateUniversityRequest>(model);

                var response = await _universityClient.CreateUniversityAsync(
                    requestModel,
                    new Metadata
                    {
                        {HeaderKeys.AccessToken, accessToken.ToString()}
                    });

                var createdModel = _mapper.Map<UniversityModel>(response);

                _logger.LogInformation($"{ProcessFlows.CreateUniversity} request was processed successfully");

                return Ok(createdModel);
            }
            catch (RpcException ex)
            {
                return HandleError(ex, ProcessFlows.CreateUniversity);
            }
        }

        /// <summary>
        /// Gives all available universities based on user access.
        /// </summary>
        /// <remarks>
        /// SuperAdmin see all universities
        /// 
        /// UniversityAdmin, Teacher and Student its own
        /// </remarks>
        /// <returns>available universities</returns>
        [HttpGet("/api/universities")]
        [Authorize(Roles = "SuperAdmin,UniversityAdmin,Teacher,Student")]
        public async Task<IActionResult> GetUniversitiesAsync()
        {
            _logger.LogInformation($"Start to process {ProcessFlows.GetAllUniversities} request");

            var accessToken = Request.Headers[HeaderNames.Authorization];

            try
            {
                var response = await _universityClient.GetUniversitiesAsync(
                    new GetUniversitiesRequest(),
                    new Metadata
                    {
                        {HeaderKeys.AccessToken, accessToken.ToString()}
                    });

                var universities = response.Universities.Select(_mapper.Map<UniversityModel>);

                _logger.LogInformation($"{ProcessFlows.GetAllUniversities} request was processed successfully");

                return Ok(universities);
            }
            catch (RpcException ex)
            {
                return HandleError(ex, ProcessFlows.GetAllUniversities);
            }
        }

        /// <summary>
        /// Update university.
        /// </summary>
        /// <remarks>
        /// Only for SuperAdmin
        /// </remarks>
        /// <returns>created university</returns>
        [HttpPut("/api/universities/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateUniversityAsync(string id, [FromBody] UpdateUniversityModel model)
        {
            _logger.LogInformation($"Start to process {ProcessFlows.UpdateUniversity} request");

            var accessToken = Request.Headers[HeaderNames.Authorization];

            try
            {
                var requestModel = _mapper.Map<UpdateUniversityRequest>(model);
                requestModel.Id = id;

                var response = await _universityClient.UpdateUniversityAsync(
                    requestModel,
                    new Metadata
                    {
                        {HeaderKeys.AccessToken, accessToken.ToString()}
                    });

                var updatedModel = _mapper.Map<UniversityModel>(response);

                _logger.LogInformation($"{ProcessFlows.UpdateUniversity} request was processed successfully");

                return Ok(updatedModel);
            }
            catch (RpcException ex)
            {
                return HandleError(ex, ProcessFlows.UpdateUniversity);
            }
        }

        /// <summary>
        /// Delete university.
        /// </summary>
        /// <remarks>
        /// Only for SuperAdmin
        /// </remarks>
        /// <returns>created university</returns>
        [HttpDelete("/api/universities/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteUniversityAsync([NotNull] string id)
        {
            _logger.LogInformation($"Start to process {ProcessFlows.DeleteUniversity} request");

            var accessToken = Request.Headers[HeaderNames.Authorization];

            try
            {
                var response = await _universityClient.DeleteUniversityAsync(
                    new DeleteUniversityRequest { Id = id },
                    new Metadata
                    {
                        {HeaderKeys.AccessToken, accessToken.ToString()}
                    });

                _logger.LogInformation($"{ProcessFlows.DeleteUniversity} request was processed successfully");

                return Ok(response);
            }
            catch (RpcException ex)
            {
                return HandleError(ex, ProcessFlows.DeleteUniversity);
            }
        }

        #endregion

        private ObjectResult HandleError(RpcException ex, string processFlow)
        {
            var httpStatusCode = StatusCodeConverter.FromGrpcToHttp(ex.StatusCode);

            _logger.LogWarning($"{processFlow} request finished with error {ex.Status.Detail}, status code {httpStatusCode}");

            return Problem(ex.Status.Detail, statusCode: httpStatusCode);
        }
    }
}