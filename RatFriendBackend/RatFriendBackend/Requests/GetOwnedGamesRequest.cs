namespace RatFriendBackend.Requests;

public record GetOwnedGamesRequest(string SteamApiKey, ulong SteamId);