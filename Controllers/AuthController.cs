using Microsoft.AspNetCore.Mvc;
using BoardGameBackend.Repositories;
using BoardGameBackend.Models;
using AutoMapper;
using BoardGameBackend.Helpers;
using BoardGameFrontend.Models;

namespace BoardGameBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public AuthController(IAuthService authService, IMapper mapper)
        {
            _mapper = mapper;
            _authService = authService;
        }

        public bool CheckGameVersion(int iVersion)
        {
            return GameConstants.m_iGameVersion == iVersion;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Register(RegisterUserDto registerUserDto)
        {
            try
            {
                UserModel user = new UserModel
                {
                    Username = registerUserDto.Username,
                    Password = registerUserDto.Password,
                    Id = Guid.NewGuid(),
                };

                var result = await _authService.SignUpAsync(user);

                if (result == null)
                {
                    return Ok(new { Message = "Registration successful" });
                }

                if (result.Message.Contains("duplicate key error"))
                {
                    return BadRequest(new { Error = "Username is already taken" });
                }

                return BadRequest(new { Error = "Unexpected error occurred during registration" });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, new { Error = "An internal server error occurred" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDto loginUserDto)
        {
            try
            {
                var data = await _authService.LoginAsync(loginUserDto);
                if (data == null)
                {
                    return Unauthorized("Invalid login attempt.");
                }

                var userDto = _mapper.Map<UserModelDto>(data.User);
                return Ok(new { User = userDto, data.Token });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }
        }
        
        [HttpPost("GameVersion")]
        public async Task<IActionResult> GameVersion(GameVersionCheckDto dto)
        {
            try
            {
                var bValidVersion = CheckGameVersion(dto.VersionId);
                if (!bValidVersion)
                {
                    return BadRequest(new { Error = "Invalid version attempt." });
                }
                return Ok(new { JsonDictionary = InitJsonManager.GetJSONDictionary(), ChangelogList = InitJsonManager.GetChangelogList(), Constants = GameConstants.GetGameConstants() });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { Error = "Unexpected error" });
            }
        }
    }
}