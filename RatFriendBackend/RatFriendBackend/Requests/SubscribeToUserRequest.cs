namespace RatFriendBackend.Requests;

public record SubscribeToUserRequest(string SteamApiKey, ulong SteamId, ulong FriendId);