using AlphaPaulloz.Twitch;

namespace AlphaPaulloz.Core;

public static class Locator
{
    public static ITwitchService TwitchService { get; private set; } = new NullTwitchService();

    public static void Provide<TService>(TService? service)
    where TService : IService
    {
        switch (service)
        {
            case ITwitchService twitchService:
                TwitchService = twitchService;
                break;
            case null:
                if (typeof(TService) == typeof(ITwitchService))
                    TwitchService = new NullTwitchService();
                break;
        }
    }
}
