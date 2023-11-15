namespace RatFriendBackend.Requests;

public record GetRecentlyPlayedGamesRequest(string SteamApiKey, ulong SteamId);