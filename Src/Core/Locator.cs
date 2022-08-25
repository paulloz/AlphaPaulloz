using AlphaPaulloz.Twitch;

namespace AlphaPaulloz.Core;

public static class Locator
{
    public static ITwitchService Twitch { get; private set; } = new NullTwitchService();
    public static ILoggerService Logger { get; private set; } = new DefaultLogger();

    public static void Provide<TService>(TService? service)
    where TService : IService
    {
        switch (service)
        {
            case ITwitchService twitchService:
                Twitch = twitchService;
                break;
            case ILoggerService loggerService:
                Logger = loggerService;
                break;
            case null:
                if (typeof(TService) == typeof(ITwitchService))
                    Twitch = new NullTwitchService();
                else if (typeof(TService) == typeof(ILoggerService))
                    Logger = new DefaultLogger();
                break;
        }
    }
}
