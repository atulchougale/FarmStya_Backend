using FarmStay.Application.DTOs.Auth;
using FarmStay.Application.Interfaces.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [HttpPost("verify-otp")] 
        public async Task<IActionResult> VerifyOtp( [FromBody] VerifyOtpRequestDto dto) 
        { 
            var result = await _userService.VerifyOtpAsync(dto); 
            if (result.Success) { return Ok(result); } 
            return BadRequest(result); 
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _userService.LoginAsync(dto);


            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);

        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var result = await _userService.ForgotPasswordAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("reset-password-email")]
        public async Task<IActionResult> ResetPasswordEmail([FromBody] ResetPasswordByEmailDto dto)
        {
            var result = await _userService.ResetPasswordEmailAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("reset-password-otp")]
        public async Task<IActionResult> ResetPasswordOtp([FromBody] ResetPasswordOtpDto dto)
        {
            var result = await _userService.ResetPasswordOtpAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var result = await _userService.ChangePasswordAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _userService.GetProfileAsync();

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


        [Authorize]
        [HttpPut("profile/email")]
        public async Task<IActionResult> UpdateEmail(UpdateEmailRequest request)
        {
            var result = await _userService.UpdateEmailAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize]
        [HttpPost("profile/contact/send-code")]
        public async Task<IActionResult> SendProfileContactCode(ProfileContactRequest request)
        {
            var result = await _userService.RequestProfileContactAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize]
        [HttpPost("profile/contact/verify-code")]
        public async Task<IActionResult> VerifyProfileContactCode(VerifyProfileContactRequest request)
        {
            var result = await _userService.VerifyProfileContactAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            var result = await _userService.UpdateProfileAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("verification-status")]
        public async Task<IActionResult> VerificationStatus(int farmHouseId, int userId)
        {
            var result = await _userService.VerificationStatusAsync(farmHouseId, userId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken( [FromBody] RefreshTokenDto dto)
        {
            var result = await _userService.RefreshTokenAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDto dto)
        {
            var result = await _userService.LogoutAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


        // =============================================
        // Resend OTP
        // =============================================

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequestDto dto)
        {
            var result = await _userService.ResendOtpAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


        // =============================================
        // Login with Mobile OTP - Send OTP
        // =============================================

        [HttpPost("send-login-otp")]
        public async Task<IActionResult> SendLoginOtp([FromBody] SendLoginOtpRequestDto dto)
        {
            var result = await _userService.SendLoginOtpAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // =============================================
        // Login with Mobile OTP - Verify OTP
        // =============================================

        [HttpPost("verify-login-otp")]
        public async Task<IActionResult> VerifyLoginOtp([FromBody] VerifyLoginOtpRequestDto dto)
        {
            var result = await _userService.VerifyLoginOtpAsync(dto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}