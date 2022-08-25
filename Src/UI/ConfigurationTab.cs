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
        channelNameInput!.Text = Locator.Twitch.ChannelName;
    }

    public override void _Process(float _)
    {
        var twitch = Locator.Twitch;

        channelNameInput!.Editable = !twitch.Connecting && !twitch.Connected;
        connectButton!.Visible = !twitch.Connected;
        connectButton!.Disabled = twitch.Connecting;
        disconnectButton!.Visible = twitch.Connected;
    }

    private void OnChannelNameInputChanged(string text)
    {
        Locator.Twitch.ConfigureChannelName(text);
    }

    private void OnConnectButtonPressed()
    {
        Locator.Twitch.Connect();
    }

    private void OnDisconnectButtonPressed()
    {
        Locator.Twitch.Disconnect();
    }

    private void OnQuitButtonPressed()
    {
        Locator.Twitch.Disconnect();
        GetTree().Quit();
    }
}
