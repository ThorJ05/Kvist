using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _config;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
    }

    // Selvregistrering giver ALTID rollen "User". Brugeren kan ikke vælge rolle selv.
    // Fjern dette endpoint, hvis kun SuperUser/Admin skal kunne oprette brugere.
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = new IdentityUser { UserName = dto.Username };
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, "User");
        return Ok(new { message = "User registered successfully!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Username);
        if (user == null)
            return Unauthorized("Invalid username or password.");

        // lockoutOnFailure: true => konto låses efter for mange forkerte forsøg (brute force-beskyttelse)
        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
            return StatusCode(423, "Account is locked. Try again later.");
        if (!result.Succeeded)
            return Unauthorized("Invalid username or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            username = user.UserName,
            roles
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new
    {
        id = User.FindFirstValue(ClaimTypes.NameIdentifier),
        username = User.Identity?.Name,
        roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value)
    });
}
