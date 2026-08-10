using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FarmStay.Application.DTOs.Auth;
using FarmStay.Application.Interfaces.Services.Auth;

namespace FarmStay.API.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }


        //[HttpPost("register")]
        //public async Task<IActionResult> Register(RegisterRequestDto dto)
        //{
        //    var result = await _userService.RegisterAsync(dto);

        //    return Ok(result);
        //}

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            var result = await _userService.RegisterAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // ============================================= // Verify Email // =============================================
         [HttpGet("verify-email")] 
        public async Task<IActionResult> VerifyEmail( [FromQuery] int farmHouseId, [FromQuery] int userId, [FromQuery] string token)
        { 
            var result = await _userService.VerifyEmailAsync( new VerifyEmailRequestDto { FarmHouseId = farmHouseId, UserId = userId, Token = token }); 
            if (result.Success) { return Ok(result); } return BadRequest(result); 
        } 
        // ============================================= // Verify OTP // =============================================
        [HttpPost("verify-otp")] public async Task<IActionResult> VerifyOtp( [FromBody] VerifyOtpRequestDto dto) 
        { 
            var result = await _userService.VerifyOtpAsync(dto); 
            if (result.Success) { return Ok(result); } 
            return BadRequest(result); 
        }



        //[HttpPost("login")]
        //public async Task<IActionResult> Login(LoginDto dto)
        //{
        //    var result = await _userService.LoginAsync(dto);

        //    if (!result.Success)
        //        return Unauthorized(result);

        //    return Ok(result);
        //}


        //[Authorize]
        //[HttpGet("profile")]
        //public async Task<IActionResult> GetProfile()
        //{
        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        //    if (userIdClaim == null) return Unauthorized();

        //    int userId = int.Parse(userIdClaim.Value);

        //    var result = await _userService.GetProfileAsync(userId);

        //    if (!result.Success) return NotFound(result);

        //    return Ok(result);
        //}


        //[HttpGet("verify-email")]
        //public async Task<IActionResult> VerifyEmail(string token)
        //{
        //    var result = await _userService.VerifyEmailAsync(token);

        //    if (!result.Success) return BadRequest(result);

        //    return Ok(result);
        //}

        //[HttpPost("forgot-password")]
        //public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        //{
        //    var result = await _userService.ForgotPasswordAsync(dto);

        //    if (!result.Success) return BadRequest(result);

        //    return Ok(result);
        //}

        //[HttpPost("reset-password")]
        //public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        //{
        //    var result = await _userService.ResetPasswordAsync(dto);

        //    if (!result.Success) return BadRequest(result);

        //    return Ok(result);
        //}

        ////[HttpGet("reset-password-direct")]
        ////public async Task<IActionResult> ResetPasswordDirect(string token)
        ////{
        ////    var dto = new ResetPasswordDto
        ////    {
        ////        Token = token,
        ////        NewPassword = "12345678"
        ////    };

        ////    var result = await _userService.ResetPasswordAsync(dto);

        ////    if (!result.Success) return BadRequest(result);

        ////    return Ok(result);
        ////}

        //[Authorize]

        //[HttpPost("change-password")]
        //public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        //{
        //    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        //    var result = await _userService.ChangePasswordAsync(userId,dto);

        //    if (!result.Success) return BadRequest(result);

        //    return Ok(result);
        //}
    }
}