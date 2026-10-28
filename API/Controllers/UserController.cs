using System.Security.Claims;
using API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

/// <summary>
/// Rettigheder:
///   User      : kan kun læse brugere
///   SuperUser : kan oprette, læse, opdatere og slette brugere med rollen User
///   Admin     : kan oprette, læse, opdatere og slette alle (User, SuperUser, Admin)
/// [Authorize(Roles = ...)] styrer hvilke endpoints man kan kalde.
/// Hvem man må ændre (SuperUser må ikke røre SuperUser/Admin) tjekkes i koden nedenfor.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private const string UserRole = "User";
    private static readonly string[] AllRoles = { "User", "SuperUser", "Admin" };

    private readonly UserManager<IdentityUser> _userManager;

    public UsersController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    // ---------- Hjælpefunktioner til rolleregler ----------

    private static int Rank(IEnumerable<string> roles) =>
        roles.Select(r => r switch { "Admin" => 3, "SuperUser" => 2, "User" => 1, _ => 0 })
             .DefaultIfEmpty(0).Max();

    private int CallerRank => Rank(User.FindAll(ClaimTypes.Role).Select(c => c.Value));
    private string? CallerId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    // Admin må ændre alle. SuperUser må kun ændre brugere med rollen User.
    private static bool CanManage(int callerRank, int targetRank) =>
        callerRank == 3 || (callerRank == 2 && targetRank <= 1);

    // Admin må tildele alle roller. SuperUser må kun tildele User.
    private static bool CanAssign(int callerRank, string role) =>
        callerRank == 3 ? AllRoles.Contains(role) : callerRank == 2 && role == UserRole;

    private ObjectResult Forbidden(string message) => StatusCode(StatusCodes.Status403Forbidden, message);

    // ---------- Endpoints ----------

    [HttpGet]
    [Authorize(Roles = "User,SuperUser,Admin")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();
        var result = new List<UserDto>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(new UserDto(u.Id, u.UserName!, roles.ToList()));
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperUser,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (!CanAssign(CallerRank, dto.Role))
            return Forbidden("You are not allowed to create a user with that role.");

        var user = new IdentityUser { UserName = dto.Username };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, dto.Role);

        return Created($"api/users/{user.Id}", new UserDto(user.Id, user.UserName!, new List<string> { dto.Role }));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperUser,Admin")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
    {
        var target = await _userManager.FindByIdAsync(id);
        if (target == null) return NotFound();

        var callerRank = CallerRank;
        var targetRoles = await _userManager.GetRolesAsync(target);

        if (!CanManage(callerRank, Rank(targetRoles)))
            return Forbidden("You are not allowed to modify this user.");

        // Valider rolleændring før noget gemmes
        var changeRole = dto.Role != null && !targetRoles.Contains(dto.Role);
        if (changeRole)
        {
            if (target.Id == CallerId)
                return BadRequest("You cannot change your own role.");
            if (!CanAssign(callerRank, dto.Role!))
                return Forbidden("You are not allowed to assign that role.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Username) && dto.Username != target.UserName)
        {
            var r = await _userManager.SetUserNameAsync(target, dto.Username);
            if (!r.Succeeded) return BadRequest(r.Errors.Select(e => e.Description));
        }

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(target);
            var r = await _userManager.ResetPasswordAsync(target, resetToken, dto.Password);
            if (!r.Succeeded) return BadRequest(r.Errors.Select(e => e.Description));
        }

        if (changeRole)
        {
            await _userManager.RemoveFromRolesAsync(target, targetRoles);
            await _userManager.AddToRoleAsync(target, dto.Role!);
        }

        var newRoles = await _userManager.GetRolesAsync(target);
        return Ok(new UserDto(target.Id, target.UserName!, newRoles.ToList()));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperUser,Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var target = await _userManager.FindByIdAsync(id);
        if (target == null) return NotFound();

        if (target.Id == CallerId)
            return BadRequest("You cannot delete yourself.");

        var targetRoles = await _userManager.GetRolesAsync(target);
        if (!CanManage(CallerRank, Rank(targetRoles)))
            return Forbidden("You are not allowed to delete this user.");

        var result = await _userManager.DeleteAsync(target);
        if (!result.Succeeded) return BadRequest(result.Errors.Select(e => e.Description));

        return NoContent();
    }
}
