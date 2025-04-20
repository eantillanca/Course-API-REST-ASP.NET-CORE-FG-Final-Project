using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MoviesAPI.Dtos.Auth;
using MoviesAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;

namespace MoviesAPI.Controllers;


[ApiController]
[Route("api/accounts")]
public class AccountsController : CustomBaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly HashService _hashService;
    private readonly IDataProtector _dataProtector;
    private const string ROLE_USER = "User";
    private const string ROLE_ADMIN = "Admin";

    public AccountsController(ApplicationDbContext context, IMapper mapper, UserManager<IdentityUser> userManager, IConfiguration configuration,
        SignInManager<IdentityUser> signInManager, IDataProtectionProvider dataProtectionProvider,
        HashService hashService) : base(context, mapper)
    {
        _context = context;
        _mapper = mapper;
        _userManager = userManager;
        _configuration = configuration;
        _signInManager = signInManager;
        _hashService = hashService;
        _dataProtector = dataProtectionProvider.CreateProtector("accounts-data-protector");
    }

    [HttpPost("register", Name = "register")]
    public async Task<ActionResult<AuthenticationResponseDto>> Register(UserCredentialsDto userCredentialsDto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == userCredentialsDto.Email))
        {
            return BadRequest("User already exists.");
        }

        var user = new IdentityUser { UserName = userCredentialsDto.Email, Email = userCredentialsDto.Email };
        var result = await _userManager.CreateAsync(user, userCredentialsDto.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, ROLE_USER);
            await _userManager.AddClaimAsync(user, new Claim("isAdmin", "0"));
            return await GenerateTokenAsync(user);
        }
        else
        {
            return BadRequest(result.Errors);
        }
    }

    private async Task<AuthenticationResponseDto> GenerateTokenAsync(IdentityUser identityUser)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, identityUser.Email),
            new Claim(ClaimTypes.NameIdentifier, identityUser.Id)
        };

        var claimsDb = await _userManager.GetClaimsAsync(identityUser);
        claims.AddRange(claimsDb.Where(c => c.Type != "exp"));
        var roles = await _userManager.GetRolesAsync(identityUser);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT_KEY"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var issuedAt = DateTime.UtcNow;
        var expiration = issuedAt.AddYears(1);

        // Usa el constructor más explícito posible
        var securityToken = new JwtSecurityToken(
            issuer: string.Empty,
            audience: string.Empty,
            claims: claims,
            notBefore: issuedAt,
            expires: expiration,
            signingCredentials: creds);

        return new AuthenticationResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
            Expiration = expiration
        };
    }

    private async Task<AuthenticationResponseDto> GenerateTokenAsync(UserCredentialsDto userCredentialsDto)
    {
        var identityUser = await _userManager.FindByEmailAsync(userCredentialsDto.Email);
        return await GenerateTokenAsync(identityUser);
    }

    [HttpPost("login", Name = "login")]
    public async Task<ActionResult<AuthenticationResponseDto>> Login(UserCredentialsDto userCredentialsDto)
    {
        var result = await _signInManager.PasswordSignInAsync(userCredentialsDto.Email,
            userCredentialsDto.Password,
            isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return await GenerateTokenAsync(userCredentialsDto);
        }
        else
        {
            return BadRequest("Login error");
        }
    }

    [HttpPost("logout", Name = "logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        await HttpContext.SignOutAsync();

        foreach (var cookie in Request.Cookies.Keys)
        {
            Response.Cookies.Delete(cookie);
        }

        return Ok(new { message = "Logout successful" });
    }

    [HttpGet("refreshToken", Name = "refreshToken")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<AuthenticationResponseDto>> RefreshToken()
    {
        var emailClaim = HttpContext.User.Claims.Where(c => c.Type == "email").FirstOrDefault();
        if (emailClaim == null) { return NotFound("email not found"); }
        var email = emailClaim.Value;

        var userCredentials = new UserCredentialsDto()
        {
            Email = email
        };

        return await GenerateTokenAsync(userCredentials);
    }

    [HttpPost("makeAdmin", Name = "makeAdmin")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminRolePolicy")]
    public async Task<ActionResult> MakeAdmin(AdminEditDto adminEditDto)
    {
        var user = await _userManager.FindByEmailAsync(adminEditDto.Email);
        if (user == null) { return NotFound("User not found"); }
        if (!await _userManager.IsInRoleAsync(user, ROLE_ADMIN))
        {
            await _userManager.AddToRoleAsync(user, ROLE_ADMIN);
        }
        await _userManager.AddClaimAsync(user, new Claim("isAdmin", "1"));
        return NoContent();
    }

    [HttpPost("removeAdmin", Name = "removeAdmin")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminRolePolicy")]
    public async Task<ActionResult> RemoveAdmin(AdminEditDto adminEditDto)
    {
        var user = await _userManager.FindByEmailAsync(adminEditDto.Email);
        if (user == null) { return NotFound("User not found"); }
        if (await _userManager.IsInRoleAsync(user, ROLE_ADMIN))
        {
            await _userManager.RemoveFromRoleAsync(user, ROLE_ADMIN);
        }
        await _userManager.RemoveClaimAsync(user, new Claim("isAdmin", "1"));
        return NoContent();
    }

    [HttpGet("encrypt", Name = "encrypt")]
    public ActionResult Encrypt()
    {
        var text = "My original text";
        var encryptedText = _dataProtector.Protect(text);
        var decryptedText = _dataProtector.Unprotect(encryptedText);
        return Ok(new
        {
            text = text,
            encryptedText = encryptedText,
            decryptedText = decryptedText
        });
    }

    [HttpGet("encrypt-with-time-limit", Name = "encryptWithTimeLimit")]
    public ActionResult EncryptWithTimeLimit()
    {
        var timeLimitedProtector = _dataProtector.ToTimeLimitedDataProtector();
        var text = "My original text";
        var timeLimitedEncryptedText = timeLimitedProtector.Protect(text, lifetime: TimeSpan.FromSeconds(5));
        Thread.Sleep(6000);
        var timeLimitedDecryptedText = _dataProtector.Unprotect(timeLimitedEncryptedText);
        return Ok(new
        {
            text = text,
            timeLimitedEncryptedText = timeLimitedEncryptedText,
            timeLimitedDecryptedText = timeLimitedDecryptedText
        });
    }

    [HttpGet("hash/{text}", Name = "hash")]
    public ActionResult HashResult(string text)
    {
        var result1 = _hashService.Hash(text);
        var result2 = _hashService.Hash(text);

        return Ok(new
        {
            text = text,
            result1 = result1,
            result2 = result2
        });
    }

    [HttpGet("users", Name = "getUsers")]
    [Authorize(Policy = "AdminRolePolicy")]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var usersDto = users.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email
        }).ToList();

        return Ok(usersDto);
    }

    [HttpGet("claims")]
    [Authorize]
    public ActionResult DebugClaims()
    {
        var claims = HttpContext.User.Claims
                         .Select(c => new { c.Type, c.Value })
                         .ToList();
        return Ok(claims);
    }
}
