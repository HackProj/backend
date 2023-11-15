namespace RatFriendBackend.Requests;
public record GetRatMomentsRequest(string SteamApiKey, ulong SteamId);