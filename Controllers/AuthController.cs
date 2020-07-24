using System.Threading.Tasks;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Auth;
using dotnet_rpg.Models;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterUserDto request)
        {
            ServiceResponse<int> response = await _authRepository.Register(new User { UserName = request.UserName }, request.Password);

            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            ServiceResponse<string> response = await _authRepository.Login(request.UserName, request.Password);

            if (!response.Success)
            {
                BadRequest(response);
            }
            return Ok(response);
        }
    }
}