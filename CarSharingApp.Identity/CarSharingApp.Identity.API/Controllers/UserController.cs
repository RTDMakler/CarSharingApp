﻿using CarSharingApp.Identity.BusinessLogic.Models.User;
using CarSharingApp.Identity.BusinessLogic.Models.UserInfo;
using CarSharingApp.Identity.BusinessLogic.Services;
using CarSharingApp.Identity.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarSharingApp.Identity.API.Controllers;

[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserManageService _userManageService;
    private readonly ITokenService _tokenService;

    public UserController(IUserManageService userManageService, ITokenService tokenService)
    {
        _userManageService = userManageService;
        _tokenService = tokenService;
    }

    [HttpGet]
    [Route("{id}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> GetAsync([FromRoute] string id, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var userDto = await _userManageService.GetByIdAsync(id);
        
        return Ok(userDto);
    }

    [HttpGet]
    [Route("")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> GetByNameAsync([FromQuery] string firstName, [FromQuery]string lastName, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var result = await _userManageService.GetByNameAsync(firstName, lastName, token);
        
        return Ok(result);
    }

    [HttpGet]
    [Route("expired")]
    [Authorize]
    public async Task<IActionResult> GetExpiredUserInfosAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        
        var result = await _userManageService.GetExpiredUserInfosAsync(token);
        
        return Ok(result);
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginAsync(LogInDto dto, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var userDto = await _userManageService.LogInAsync(dto);
        var accessToken = await _tokenService.GenerateToken(userDto.Id);
        
        Response.Cookies.Delete("Authorization");
        Response.Cookies.Append(
            "Authorization",
            accessToken,
            new CookieOptions()
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true
            });
        
        return Ok(userDto);
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> CreateAsync(UserNecessaryDto dto, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        await _userManageService.RegistrationAsync(dto);
        
        return Created("User Added Successfully", dto);
    }
    
    [HttpPost]
    [Route("{userId}/info")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> AddUserInfoAsync(string userId, UserInfoCleanDto dto, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        await _userManageService.AddUserInfoAsync(userId, dto, token);
        
        return Created("User Added Successfully", dto);
    }

    [Authorize]
    [HttpPut]
    [Route("{id}")]
    public async Task<IActionResult> EditAsync([FromRoute] string id, [FromBody] UserNecessaryDto dto, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var userDto = await _userManageService.UpdateAsync(id, dto);
        
        return Ok(userDto);
    }
    
    [Authorize]
    [HttpPut]
    [Route("{userId}/info")]
    public async Task<IActionResult> EditUserInfoAsync([FromRoute] string userId, [FromBody] UserInfoCleanDto dto, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var userDto = await _userManageService.UpdateUserInfoAsync(userId, dto);
        
        return Ok(userDto);
    }
    
    [Authorize]
    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> DeleteAsync([FromRoute]string id, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var deletedUser = await _userManageService.DeleteAsync(id);
        
        return Ok(deletedUser);
    }
    
    [Authorize]
    [HttpDelete]
    [Route("{userId}/info")]
    public async Task<IActionResult> DeleteInfoAsync([FromRoute]string userId, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        var deletedUser = await _userManageService.DeleteUserInfoAsync(userId, token);
        
        return Ok(deletedUser);
    }
}