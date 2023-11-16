namespace RatFriendBackend.Requests;

public record RemoveSubscriptionRequest(string FriendUrl, string AppName, long UserId);