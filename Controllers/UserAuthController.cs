using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using User_Authapi.DTO_s;
using User_Authapi.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using AutoMapper;
using System;
using Microsoft.Extensions.Logging;
using Serilog;
using Azure.Core;
using Serilog.Core;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using User_Authapi.Entities;
using System.Net;
using User_Authapi.Services;
// kind having issues with the post endpoint , i want to fix the logs file and finish my auth 
namespace User_Authapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthController : ControllerBase                          
    {
        private readonly IConfiguration _configuration;
        private readonly UsersDbcontext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UserAuthController> _logger;
        private readonly ITokenService _tokenService;

        public UserAuthController(
                            IConfiguration configuration,
                            UsersDbcontext context,
                            IMapper mapper,
                            ILogger<UserAuthController> logger,
                            ITokenService tokenService
  )
        {
            _configuration = configuration;
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _tokenService = tokenService;
        }
        
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO logindto)
        {
            var requestId8 = HttpContext.TraceIdentifier;
            _logger.LogInformation("Request {requestId8} : Method {Method} Queried with args . ", requestId8, nameof(Login));

            string accessToken = String.Empty;
            RefreshTokens refreshToken = null; // Declare refreshToken before usage  

            if (!string.IsNullOrWhiteSpace(logindto.Email) && !string.IsNullOrWhiteSpace(logindto.Password))
            {
                try
                {
                    var Loginentity = await _context.Users
                        .FirstOrDefaultAsync(l => l.Email == logindto.Email);

                    if (Loginentity is null || Loginentity.IsDeleted)
                    {
                        _logger.LogError("Request {TraceId} : User not found !!!!.", requestId8);
                        return Unauthorized("Invalid Credentials !!!");
                    }
                    else
                    {
                        _logger.LogInformation("Request {TraceId} : User with Username {Loginentity.UserName} has been found .", requestId8, Loginentity.UserName);
                    }

                    if (string.IsNullOrWhiteSpace(Loginentity.Password))
                    {
                        _logger.LogError("Request {TraceId} : Stored password is missing!", requestId8);
                        return BadRequest("Password not set for this user.");
                    }

                    if (string.IsNullOrWhiteSpace(logindto.Password))
                    {
                        _logger.LogError("Request {TraceId} : Supplied password is null or empty!", requestId8);
                        return BadRequest("Password is required.");
                    }

                    if (logindto.Password == null)
                    {
                        _logger.LogError("Request {TraceId}: Password from DTO is null!", requestId8);
                        return BadRequest("Password cannot be null.");
                    }

                    var reversepassword = new PasswordHasher<object>();
                    var result = reversepassword.VerifyHashedPassword(new object(), Loginentity.Password, logindto.Password);

                    if (result == PasswordVerificationResult.Success)
                    {
                        _logger.LogInformation("Request {TraceId} : Password successfully verified ", requestId8);
                    }
                    else
                    {
                        _logger.LogError("Request {TraceId}: Wrong Password!!!!", requestId8);
                        return Unauthorized("Invalid credentials");
                    }

                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    accessToken = _tokenService.CreateAccessToken(Loginentity);
                    refreshToken = _tokenService.CreateRefreshToken(ipAddress);

                    Loginentity.RefreshTokens.Add(refreshToken);
                    await _context.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    _logger.LogError("Request {TraceId} : Unable to Login !!!", requestId8);
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                _logger.LogWarning("Request {TraceId}: Email or Password not supplied", requestId8);
                return BadRequest("Email and Password are required.");
            }

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.RefreshToken  
            });
        }
        [Authorize]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDTO logoutdto)
        {

            var requestId9 = HttpContext.TraceIdentifier;
            _logger.LogInformation("Request {TraceId} : Entered Method {Method} with args .", requestId9, nameof(Logout));

            if (String.IsNullOrWhiteSpace(logoutdto.RefreshToken))
                return BadRequest("Refresh Token is required");

            try
            {
                var StoredToken = await _context.RefreshTokens
                    .Include(s => s.User)
                    .SingleOrDefaultAsync(s => s.RefreshToken == logoutdto.RefreshToken);

                if (StoredToken == null || !StoredToken.IsActive)
                {
                    _logger.LogWarning("Request {TraceId}: Invalid or already revoked refresh token.", requestId9);
                    return Unauthorized("Invalid refresh token.");
                }

                StoredToken.Revoked = DateTime.UtcNow;
                StoredToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();

                await _context.SaveChangesAsync();

                _logger.LogInformation("Request {TraceId}: Successfully revoked refresh token for UserId {UserId}.",
                    requestId9, StoredToken.UserId);

            }
            catch (Exception ex)
            {
                _logger.LogError("Request {TraceId} : Unable to log out !!!!", requestId9);
                return StatusCode(500, "An error occurred while logging out.");
            }
            return Ok(new { message = "Logged out successfully." });
        }

        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<IEnumerable<GetAllUserDTO>>> GetUsers()
        {
            
            var requestId = HttpContext.TraceIdentifier;
            _logger.LogInformation("Request {TraceId} : Entered {Method} with args", requestId , nameof(GetUsers));

            try
            {
                var AllUsers = await _context.Users
                     .Select(u => new GetAllUserDTO(u.Id, u.UserName, u.Email))
                        .ToListAsync();

                if (AllUsers is null || AllUsers.Count == 0)
                {
                    _logger.LogWarning("Request {TraceId} : No Users found in the database", requestId);
                    return NotFound();
                }

                    _logger.LogInformation("Fetched users {Count} from the database.", AllUsers.Count);
                    return Ok(AllUsers);
            }
            catch (Exception ex)
            {
               _logger.LogError( ex , "Request {TraceId} : An error occured while fetching the users", requestId);
                return StatusCode(500, "Internal Server Error .");
            }
        }

        [HttpGet("GetUserById/{Id}")]
        public async Task<ActionResult<GetSingleUserDTO>> GetUserById(int Id)
        {
            var requestId1 = HttpContext.TraceIdentifier;
            _logger.LogInformation("Entered {Method} with {Id}." , nameof(GetUserById) , Id);

            try
            {
                var User = await _context.Users
                            .FirstOrDefaultAsync(u => u.Id == Id);

                if (User is null )
                {
                    _logger.LogError("Request {TraceId} : The user with {Id} wasn't found or it doesnt exist", requestId1 , Id);
                    return NotFound();
                }

                var userDto = new GetSingleUserDTO(User.Id, User.UserName, User.Email);
                _logger.LogInformation("Request {TraceId} : The user with Id {Id} and UserName {UserName} has been found" , requestId1 , Id, User.UserName);

                return Ok(userDto);
             }
            catch(Exception ex)
            {
                _logger.LogError(ex ,"Request {TraceId} : An error occurred while fetching the users", requestId1);
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpPost("CreateUser")]   
        public async Task<IActionResult> CreateUser(CreateUserDTO newUser)
        {
            var requestId2 = HttpContext.TraceIdentifier;
            _logger.LogInformation("Request {TraceId} : Method Entered {Method}.", requestId2, nameof(CreateUser));

            try
            {
                if (newUser is null)
                {
                    _logger.LogError("Unable to create instance of CreateUserDto");
                    return BadRequest();
                }

                var passwordHasher = new PasswordHasher<object>();
                string hashedPassword = passwordHasher.HashPassword(new object(), newUser.Password);

                //map the person entity to the newuserdto 
                var userEntity = _mapper.Map<Person>(newUser);
                userEntity.Password = hashedPassword;
                userEntity.CreatedAt = DateTime.UtcNow;

                _context.Users.Add(userEntity);
                await _context.SaveChangesAsync();

                var userResponse = new HashExclusionDTO
                {
                    Id = userEntity.Id,
                    UserName = userEntity.UserName,
                    Email = userEntity.Email
                };

                _logger.LogInformation("Request {TraceId} : New User has been created with Id {Id} and username {UserName}", requestId2 , userEntity.Id , userEntity.UserName);
                return CreatedAtAction(nameof(GetUserById), new { Id = userEntity.Id }, userResponse);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex ,"Request {TraceId} : Error while creating user ", requestId2);
                return BadRequest(ex.Message);
            }
        }
        // add logic for updating the user before querying the endpoint like actually if im logged out and i dont know my pass word , id need to send my new details through an endpoint  
        [HttpPut("UpdateUser/{Id}")]
        public async Task<IActionResult> UpdateUser(int Id, UpdateUserDTO updatedUser )
        {
            var requestId3 = HttpContext.TraceIdentifier;
            _logger.LogInformation("Request {TraceId} : Method Entered {Method}. ", requestId3, nameof(UpdateUser));

            try
            {
                var UpdateUserEntity = await _context.Users
                           .FirstOrDefaultAsync(u => u.Id == Id);

                if (UpdateUserEntity is null)
                {
                    _logger.LogInformation("Request {TraceId} : User with Id {Id} doesn't exist !", requestId3 , Id);
                    return NotFound();
                }

                if (string.IsNullOrWhiteSpace(updatedUser.Password))
                    return BadRequest("Password is required.");

                if (string.IsNullOrWhiteSpace(updatedUser.UserName))
                    return BadRequest("UserName is required.");

                _mapper.Map(updatedUser, UpdateUserEntity);

                var passwordHasher1 = new PasswordHasher<object>();
                UpdateUserEntity.Password = passwordHasher1.HashPassword(new object(), updatedUser.Password);
                UpdateUserEntity.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var updatedResponse = new HashExclusionDTO1
                {
                    UserName = updatedUser.UserName,
                    Email = updatedUser.Email
                };

                _logger.LogInformation("Request {TraceId} : User successfully updated .", requestId3);

                return Ok(updatedResponse);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex ,"Request {TraceId} : Error while Updating User with Id {Id}.", requestId3, Id);
                return BadRequest(ex.Message);
            }
        }
        [HttpPatch("PartiallyUpdateUser/{Id}")]
        public async Task<IActionResult> PartialUpdate(int Id, PartialUpdateUserDTO partialUser)
        {
            var requestId4 = HttpContext.TraceIdentifier;
            _logger.LogInformation("Request {TraceId} : Method Entered {Method} .", requestId4, nameof(PartialUpdate));

            try
            {

                var partialUserEntity = await _context.Users.FirstOrDefaultAsync(p => Id == p.Id);

                if (partialUser is null)
                {
                    _logger.LogError("Request {TraceId} : User with Id {Id} cannot be found", requestId4, Id);
                    return BadRequest("Request body is missing or invalid");
                }

                if (partialUserEntity is null)
                {
                    _logger.LogError("Request {TraceId} : User with Id {Id} cannot be found", requestId4, Id);
                    return NotFound("User not found.");
                }

                if (partialUser.UserName is not null)
                    partialUserEntity.UserName = partialUser.UserName;

                if (partialUser.Password is not null)
                {
                    var passwordHasher2 = new PasswordHasher<object>();
                    string hashedpassword2 = passwordHasher2.HashPassword(new object(), partialUser.Password);
                    partialUserEntity.Password = hashedpassword2;
                }

                partialUserEntity.UpdatedAt = DateTime.UtcNow;

                if (partialUser.Email is not null)
                    partialUserEntity.Email = partialUser.Email;

                if (partialUser.UserName is null && partialUser.Password is null && partialUser.Email is null)
                {
                    _logger.LogError("Request {TraceId} : No field was provided for update" , requestId4);
                    return BadRequest("At least one field must be provided for update.");
                }

                await _context.SaveChangesAsync();

                var partialresponse = new HashExclusionDTO2
                {
                    UserName = partialUserEntity.UserName,
                    Email = partialUserEntity.Email
                };

                _logger.LogInformation("Request {TraceId} : User successfully updated .", requestId4);
                return Ok(partialresponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request {TraceId} : Error while updating User with Id {Id} ." , requestId4 , Id);
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("Softdelete/{Id}")]
        public async Task<IActionResult> SoftDelete(int Id)
        {
            var requestId5 = HttpContext.TraceIdentifier;
            _logger.LogInformation("Request {TraceId} : Endpoint {Method} Succesfully Queried .", requestId5, nameof(SoftDelete));

            try
            {
                var deleteuserEntity = await _context.Users.FirstOrDefaultAsync(d => d.Id == Id);

                if (deleteuserEntity is null)
                {
                    _logger.LogError("Request {TraceId} : Unable to find User with Id {Id} .", requestId5, Id);
                    return NotFound();
                }

                //sets a flag on the database thst is deleted 
                deleteuserEntity.IsDeleted = true;
                deleteuserEntity.DeletedAt = DateTime.UtcNow;

                var deleteresponse = new HashExclusionDTO
                {
                    Id = deleteuserEntity.Id,
                    UserName = deleteuserEntity.UserName,
                    Email = deleteuserEntity.Email
                };

                await _context.SaveChangesAsync();
                _logger.LogInformation("Request {TraceId} : User with Id {Id} Successfully deleted ." , requestId5 , Id);

                return Ok(deleteresponse);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Request {TraceId} : Error in deleting User with Id {Id} .", requestId5, Id);
                return BadRequest(ex.Message);
            }
          }
        [HttpDelete("HardDelete/{Id}")]
        public async Task<IActionResult> HardDelete(int Id)
        {

            var requestId6 = HttpContext.TraceIdentifier;
            _logger.LogInformation("Request {TraceId} : Endpoint {Method} Successfully Queried .", requestId6, nameof(HardDelete));

            try
            {
                var HarddeleteEntity = await _context.Users.FirstOrDefaultAsync(h => h.Id == Id);

                if (HarddeleteEntity is null)
                {
                    _logger.LogError("Request {TraceId} : The User with Id {Id} cannot be found .", requestId6, Id);
                    return NotFound();
                }

                var hardresponse = new HashExclusionDTO
                {
                    Id = HarddeleteEntity.Id,
                    Email = HarddeleteEntity.Email,
                    UserName = HarddeleteEntity.UserName
                };

                _context.Users.Remove(HarddeleteEntity);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Request {TraceId} : User with Id {Id} successfully deleted .", requestId6, Id);

                return Ok(hardresponse);
              }
           catch(Exception ex)
            {
                _logger.LogError(ex , "Request {TraceId} : Error Deleting User With Id {Id} .", requestId6, Id);
                return BadRequest(ex.Message);
            }
        }
        [HttpPatch("RestoreUser/{Id}")]
        public async Task<IActionResult> RestoreUser(int Id, RestoreUserDTO restoredUser)
        {
            var requestId7 = HttpContext.TraceIdentifier;
            _logger.LogInformation("Request {TraceId} : Endpoint {Method} successfully Queried .", requestId7, nameof(RestoreUser));

            try
            {
                var restoreEntity = await _context.Users
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Id == Id && u.IsDeleted);

                if (restoreEntity is null)
                    return NotFound();

                restoreEntity.IsDeleted = false;
                restoreEntity.RestoredAt = DateTime.UtcNow;

                var restoreResponse = new HashExclusionDTO
                {
                    UserName = restoreEntity.UserName,
                    Email = restoreEntity.Email
                };

                await _context.SaveChangesAsync();
                _logger.LogInformation("Request {TraceId} : Successfully restored User .", requestId7);

                return Ok(restoreResponse);
              }
            catch(Exception ex)
            {
                _logger.LogInformation(ex , "Request {TraceId} : Error while Restoring User .", requestId7);
                return BadRequest(ex.Message);
            }
        } 
    }
}

