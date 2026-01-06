using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using UserService.DTOs;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<ApplicationUser> userManager,
                              IEmailService emailService,
                              ITokenService tokenService)
        {
            _userManager = userManager;
            _emailService = emailService;
            _tokenService = tokenService;
        }

        // REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                FullName = dto.FullName,
                UserName = dto.Email,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Auth", new { userId = user.Id, token }, Request.Scheme);

            await _emailService.SendEmailAsync(user.Email, "Confirm your email", $"Click <a href='{link}'>here</a> to confirm.");

            return Ok("Registration successful. Check your email.");
        }

        // CONFIRM EMAIL
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return BadRequest("Invalid user");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded) return BadRequest("Email confirmation failed");

            return Ok("Email confirmed successfully");
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid credentials");

            if (!user.EmailConfirmed) return Unauthorized("Email not confirmed");

            var token = _tokenService.CreateToken(user);
            return Ok(new { token });
        }

        // FORGOT PASSWORD
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest("Email not registered");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action(nameof(ResetPassword), "Auth", new { email = user.Email, token }, Request.Scheme);

            await _emailService.SendEmailAsync(user.Email, "Reset your password", $"Click <a href='{resetLink}'>here</a> to reset your password.");

            return Ok("Password reset link sent to your email.");
        }

        // RESET PASSWORD
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest("Invalid email");

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok("Password has been reset successfully");
        }

        // UPDATE PASSWORD (authenticated)
        [HttpPost("update-password")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized("User not found");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok("Password updated successfully");
        }
    }
}
