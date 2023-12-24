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
                User user = _userRepository.GetByKey(nameof(KTX_DAL.Models.Entity.User.Email), model.Email, KTX_DAL.QueryFlags.Enabled);
                if (user == null)
                    return Unauthorized(new DefaultResponseContext { Message = "Email does not existed" });
                if (CryptoService.AESHash(model.Password, _secretKey) != user.Password)
                    return Unauthorized(new DefaultResponseContext { Message = "Response.passwordincorrect" });
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
    }
}
