using AlphaPaulloz.Core;
using Godot;

namespace AlphaPaulloz.UI;

public partial class ConfigurationTab : VBoxContainer
{
    [Export]
    private LineEdit? channelNameInput;

    [Export]
    private Button? connectButton;

    [Export]
    private Button? disconnectButton;

    public override void _Ready()
    {
        channelNameInput!.Text = Locator.TwitchService.ChannelName;
    }

    public override void _Process(float _)
    {
        var twitch = Locator.TwitchService;

        channelNameInput!.Editable = !twitch.Connecting && !twitch.Connected;
        connectButton!.Visible = !twitch.Connected;
        connectButton!.Disabled = twitch.Connecting;
        disconnectButton!.Visible = twitch.Connected;
    }

    private void OnChannelNameInputChanged(string text)
    {
        Locator.TwitchService.ConfigureChannelName(text);
    }

    private void OnConnectButtonPressed()
    {
        Locator.TwitchService.Connect();
    }

    private void OnDisconnectButtonPressed()
    {
        Locator.TwitchService.Disconnect();
    }

    private void OnQuitButtonPressed()
    {
        Locator.TwitchService.Disconnect();
        GetTree().Quit();
    }
}
