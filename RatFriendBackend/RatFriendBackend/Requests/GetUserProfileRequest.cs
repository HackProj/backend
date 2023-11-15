namespace RatFriendBackend.Requests;

public record GetUserProfileRequest(string SteamApiKey, ulong SteamId);