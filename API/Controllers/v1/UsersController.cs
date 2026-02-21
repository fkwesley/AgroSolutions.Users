using API.Helpers;
using API.Models;
using Application.DTO.User;
using Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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

        public UsersController(IUserService userService)
        {
            _userService = userService;
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
            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToUsers(users, Url, version);
            return Ok(users);
        }

        /// <summary>
        /// Returns a user by id.
        /// </summary>
        /// <returns>Object User</returns>
        [HttpGet("{userId}", Name = "GetUserById")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);

            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            if (user is not null)
                HateoasHelper.AddLinksToUser(user, Url, version);

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
            var addedUser = await _userService.AddUserAsync(userRequest);

            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            HateoasHelper.AddLinksToUser(addedUser, Url, version);

            return CreatedAtAction(nameof(Add), new { userId = addedUser.UserId, version }, addedUser);
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

            var version = HttpContext.Request.RouteValues["version"]?.ToString() ?? "1.0";
            if (updatedUser is not null)
                HateoasHelper.AddLinksToUser(updatedUser, Url, version);

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
