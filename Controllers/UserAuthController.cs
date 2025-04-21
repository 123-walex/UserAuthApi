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


namespace User_Authapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthController(UsersDbcontext context) : ControllerBase
    {
        // i want to add a primary consructor here 

        private readonly IMapper _mapper;

        private readonly UsersDbcontext _context;

        private readonly ILogger<UserAuthController> _logger;

        public UserAuthController(UsersDbcontext context, IMapper mapper, ILogger<UserAuthController> logger) : base()
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }



        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetAllUserDTO>>> GetUsers()
        {
            var AllUsers = await _context.Users
                 .Select(u => new GetAllUserDTO(u.Id, u.UserName, u.Email))
                    .ToListAsync();

            return Ok(AllUsers);
        }

        [HttpGet("{Id}")]
        public async Task<ActionResult<GetSingleUserDTO>> GetUserById(int Id)
        {
            var User = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == Id);

            if (User is null)
            {
                return NotFound();
            }
            var userDto = new GetSingleUserDTO(User.Id, User.UserName, User.Email);  // Map to DTO
            return Ok(userDto);
        }
        [HttpPost]
        public async Task<ActionResult<HashExclusionDTO>> CreateUser(CreateUserDTO newUser, IMapper mapper)
        {
            if (newUser is null)
                return BadRequest();

            var passwordHasher = new PasswordHasher<object>();
            string hashedPassword = passwordHasher.HashPassword(new object(), newUser.Password);

            //map the person entity to the newuserdto 
            var userEntity = mapper.Map<Person>(newUser);
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

            return CreatedAtAction(nameof(GetUserById), new { Id = userEntity.Id }, userResponse);
        }
        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdateUser(int Id, UpdateUserDTO updatedUser, IMapper mapper)
        {
            var UpdateUserEntity = await _context.Users
                       .FirstOrDefaultAsync(u => u.Id == Id);

            if (UpdateUserEntity is null)           
                return NotFound();

            if (string.IsNullOrWhiteSpace(updatedUser.Password))
                return BadRequest("Password is required.");

            if (string.IsNullOrWhiteSpace(updatedUser.UserName))
                return BadRequest("UserName is required.");

            mapper.Map(updatedUser, UpdateUserEntity);

            var passwordHasher1 = new PasswordHasher<object>();
            UpdateUserEntity.Password = passwordHasher1.HashPassword(new object(), updatedUser.Password);
            UpdateUserEntity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updatedResponse = new HashExclusionDTO1
            {
                UserName = updatedUser.UserName,
                Email = updatedUser.Email
            };

            return Ok(updatedResponse);
        }
        [HttpPatch("{Id}")]
        public async Task<IActionResult> PartialUpdate(int Id, PartialUpdateUserDTO partialUser)
        {
          var partialUserEntity = await _context.Users.FirstOrDefaultAsync(p => Id == p.Id);
         
          if (partialUserEntity is null)
              return NotFound("User not found.");

          if (partialUser is null)
              return BadRequest("Request body is missing or invalid");

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
                return BadRequest("At least one field must be provided for update.");

         await _context.SaveChangesAsync();

         var partialresponse = new HashExclusionDTO2
            {
                UserName = partialUserEntity.UserName,
                Email = partialUserEntity.Email
            };
        return Ok(partialresponse);
        }
        [HttpDelete("softdelete/{Id}")]
        public async Task<IActionResult> SoftDelete(int Id)
        { 
           var deleteuserEntity = await _context.Users.FirstOrDefaultAsync(d => d.Id == Id);

           if (deleteuserEntity is null)
                  return NotFound();

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

        return Ok(deleteresponse);
        }
        [HttpDelete("hardDelete/{Id}")]
        public async Task<IActionResult> HardDelete(int Id)
        {
            var HarddeleteEntity = await _context.Users.FirstOrDefaultAsync(h => h.Id == Id);

            if (HarddeleteEntity is null)
                return NotFound();

            var hardresponse = new HashExclusionDTO
            {
                Id = HarddeleteEntity.Id ,
                Email = HarddeleteEntity.Email,
                UserName= HarddeleteEntity.UserName
            };

            _context.Users.Remove(HarddeleteEntity);

            await _context.SaveChangesAsync();

            return Ok(hardresponse);
        }
        [HttpPatch("restore/{Id}")]
        public async Task<IActionResult> RestoreUser(int Id , RestoreUserDTO restoredUser)
        {
            var restoreEntity = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == Id && u.IsDeleted);
                                      
            if(restoreEntity is null)     
                return NotFound();

            restoreEntity.IsDeleted = false;
            restoreEntity.RestoredAt = DateTime.UtcNow;

            var restoreResponse = new HashExclusionDTO
            {
                UserName = restoreEntity.UserName ,
                Email = restoreEntity.Email 
            };

            await _context.SaveChangesAsync();

            return Ok(restoreResponse);
        }
    }
}

