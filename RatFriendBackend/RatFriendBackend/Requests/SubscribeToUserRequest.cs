namespace RatFriendBackend.Requests;

public record SubscribeToUserRequest(long TelegramId, string FriendUrl, string AppUrl);