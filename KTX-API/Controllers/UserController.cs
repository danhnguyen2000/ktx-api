using KTX_BLL.Security;
using KTX_BLL.Service;
using KTX_DAL.IRepository;
using KTX_DAL.Models;
using KTX_DAL.Models.Entity;
using KTX_DAL.Models.PostModel;
using KTX_DAL.Models.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KTX_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;

        private readonly UserService _userLoginService;
        private readonly IUserRepository _userRepository;

        public UserController(IConfiguration configuration,
            ILogger<UserController> logger,
            UserService userLoginService,
            IUserRepository userRepository
            )
        {
            _configuration = configuration;
            _logger = logger;
            _secretKey = _configuration.GetValue<string>("SecretKey");
            _userLoginService = userLoginService;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Login(LoginPostModel model)
        {
            try
            {
                User user = _userRepository.GetByKey(nameof(KTX_DAL.Models.Entity.User.Username), model.Username, KTX_DAL.QueryFlags.Enabled);
                if (user == null)
                    return Unauthorized(new DefaultResponseContext { Message = $"Username {ResponeMessage.NOT_EXISTED}"});
                if (CryptoService.AESHash(model.Password, _secretKey) != user.Password)
                    return Unauthorized(new DefaultResponseContext { Message = "Password incorrect" });
                string JwtToken = _userLoginService.Login();

                var data = new { 
                    Token = JwtToken,
                    User = user
                };

                return Ok(new DefaultResponseContext { Message = "Success", Data = data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Empty);
                return BadRequest(new DefaultResponseContext { Message = "somethingwhenwrong" });
            }
        }

        /// <summary>
        /// Create admin
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreatedUser(LoginPostModel model)
        {
            try
            {
                var user = new User
                {
                    Id = 1,
                    Username = model.Username,
                    Password = CryptoService.AESHash(model.Password, _secretKey),
                    FullName = "System",
                    CreatedAt = DateTime.Now
                };
                var result = await _userRepository.Insert(user);

                if (result > 0)
                {
                    _logger.LogError($"Insert operation failed. Result: {result}");

                    // Ném một exception mới
                    throw new Exception("Test exception: Insert operation failed.");
                }

                return Ok(new DefaultResponseContext { Message = "Success", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Empty);
                return BadRequest(new DefaultResponseContext { Message = "somethingwhenwrong" });
            }


        }
    }
}
