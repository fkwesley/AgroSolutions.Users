using API.Models;
using Application.DTO.User;
using Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1
{
    /// <summary>
    /// Users Controller V1
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    [Route("v{version:apiVersion}/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService usersService)
        {
            _userService = usersService;
        }

        #region GETS
        /// <summary>
        /// Returns all users registered.
        /// </summary>
        /// <returns>List of Users</returns>
        [HttpGet(Name = "GetAllUsers")]
        [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Returns a user by id.
        /// </summary>
        /// <returns>Object User</returns>
        [HttpGet("{id}", Name = "GetUserById")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }
        #endregion

        #region POST
        /// <summary>
        /// Add a user.
        /// </summary>
        /// <returns>Object user added</returns>
        [HttpPost(Name = "User")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add([FromBody] AddUserRequest userRequest)
        {
            // getting user_id and user_email from context (provided by token)
            userRequest.UserId = HttpContext.User?.FindFirst("user_id")?.Value ?? "anonymous"; // getting user_id from context (provided by token)
            userRequest.Email = HttpContext.User?.FindFirst("user_email")?.Value; 

            var addedUser = await _userService.AddUserAsync(userRequest);
            return CreatedAtAction(nameof(GetById), new { id = addedUser.UserId }, addedUser);
        }
        #endregion

        #region PUT
        /// <summary>
        /// Update a user.
        /// </summary>
        /// <returns>Object user updated</returns>
        [HttpPut("{userId}", Name = "UpdateUser")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string userId, [FromBody] UpdateUserRequest userRequest)
        {
            userRequest.UserId = userId;
            var updatedUser = await _userService.UpdateUserAsync(userRequest);
            return Ok(updatedUser);
        }
        #endregion

        #region DELETE
        /// <summary>
        /// Delete a user.
        /// </summary>
        /// <returns>No content</returns>
        [HttpDelete("{userId}", Name = "DeleteUser")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string userId)
        {
            await _userService.DeleteUserAsync(userId);
            return NoContent();
        }
        #endregion
    }
}
