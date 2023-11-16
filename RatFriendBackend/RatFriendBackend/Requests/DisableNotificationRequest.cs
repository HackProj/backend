namespace RatFriendBackend.Requests;

public record DisableNotificationRequest(long TelegramId, string FriendUrl, bool IsEnabled);