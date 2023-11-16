namespace RatFriendBackend;

public static class Constants
{
    private static string? _steamApiToken;

    public static string? SteamApiToken
    {
        get
        {
            if (String.IsNullOrEmpty(_steamApiToken))
                _steamApiToken = Environment.GetEnvironmentVariable("STEAM_API_TOKEN");

            if (String.IsNullOrEmpty(_steamApiToken))
                _steamApiToken = "E81CB9C4807B5DA6CED7A4B3B8CECFCF";
            
            return _steamApiToken;
        }
    }
}