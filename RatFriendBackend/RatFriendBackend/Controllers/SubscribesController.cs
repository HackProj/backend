using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RatFriendBackend.Persistence;
using RatFriendBackend.Persistence.Models;
using RatFriendBackend.Requests;

namespace RatFriendBackend.Controllers;

[Route("api/[controller]")]
public class SubscribesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SubscribesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("subscribeToUser")]
    public async Task<IActionResult> SubscribeToUser(SubscribeToUserRequest request)
    {
        // Проверяем есть ли такой пользователь в базе
        if (await _context.Users.FirstOrDefaultAsync(x => x.Id == request.SteamId) == null)
            _context.Users.Add(new User { ApiToken = request.SteamApiKey });

        // Проверяем есть ли уже такая подписка в базе
        if (await _context.UserFriendActivities.FirstOrDefaultAsync(x =>
                x.FriendActivityInfo != null && x.UserId == request.SteamId &&
                x.FriendActivityInfo.FriendId == request.FriendId) != null)
            _context.UserFriendActivities.Add(new UserFriendActivity(request.SteamId, request.FriendId));
        await _context.SaveChangesAsync();

        return Ok();
    }
}