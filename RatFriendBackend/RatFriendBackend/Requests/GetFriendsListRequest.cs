namespace RatFriendBackend.Requests;

public record GetFriendsListRequest(string SteamApiKey, ulong SteamId);