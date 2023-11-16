namespace RatFriendBackend.Requests;

public record GetUserProfileRequest(string SteamApiKey, List<ulong> SteamIds);