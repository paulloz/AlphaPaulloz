using AlphaPaulloz.Core;

namespace AlphaPaulloz.Twitch;

public interface ITwitchService : IService
{
    public bool Connected { get; }
    public bool Connecting { get; }
    public string ChannelName { get; }

    public void Connect();
    public void Disconnect();
    public void ConfigureChannelName(string newChannelName);
    public void SendMessage(string message);
}

public class NullTwitchService : ITwitchService
{
    public bool Connected => false;
    public bool Connecting => false;
    public string ChannelName => string.Empty;

    public void Connect() { }
    public void Disconnect() { }
    public void ConfigureChannelName(string _) { }
    public void SendMessage(string _) { }
}
