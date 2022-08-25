using AlphaPaulloz.Core;
using AlphaPaulloz.Twitch;
using Godot;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;

namespace AlphaPaulloz;

public partial class TwitchBot : Node, ITwitchService
{
    private const float CONNECTION_TIMEOUT = 8.0f;

    private readonly Config config;

    private TwitchClient client = new();
    private float connectionTimeout = 0.0f;
    private string channelName;

    public bool Connected => client.IsConnected && !Connecting;
    public bool Connecting => connectionTimeout > 0.0f;
    public string ChannelName => channelName;

    public TwitchBot()
    {
        config = Config.ReadFromFile("user://config.json");
        channelName = config.DefaultChannel;
    }

    public override void _EnterTree()
    {
        SetupClient();

        Locator.Provide(this);
    }

    public override void _ExitTree()
    {
        Locator.Provide<ITwitchService>(null);

        TeardownClient();
    }

    public override void _Process(float delta)
    {
        if (!Connecting) return;

        connectionTimeout -= delta;
        if (connectionTimeout <= 0.0f)
            Disconnect();
    }

    public void Connect()
    {
        if (Connected || Connecting) return;

        connectionTimeout = CONNECTION_TIMEOUT;
        Task.Run(() => client.Connect());
    }

    public void Disconnect()
    {
        if (!Connected && !Connecting) return;

        connectionTimeout = 0.0f;
        client.Disconnect();

        SetupClient();
    }

    public void ConfigureChannelName(string newChannelName)
    {
        channelName = newChannelName;
    }

    public void SendMessage(string message)
    {
        if (!Connected) return;

        client.SendMessage(client.GetJoinedChannel(channelName), message);
    }

    private void SetupClient()
    {
        client = new();
        client.Initialize(new ConnectionCredentials(config.Username, config.AccessToken));

        client.OnError += OnClientError;
        client.OnConnected += OnClientConnected;
        client.OnJoinedChannel += OnClientJoinedChannel;
        client.OnChatCommandReceived += OnClientChatCommandReceived;
    }

    private void TeardownClient()
    {
        client.Disconnect();

        client.OnError -= OnClientError;
        client.OnConnected -= OnClientConnected;
        client.OnJoinedChannel -= OnClientJoinedChannel;
        client.OnChatCommandReceived -= OnClientChatCommandReceived;
    }

    private void OnClientError(object? sender, OnErrorEventArgs args)
    {
        Locator.Logger.LogErr(args.Exception.Message);
    }

    private void OnClientConnected(object? sender, OnConnectedArgs args)
    {
        client.JoinChannel(channelName);
    }

    private void OnClientJoinedChannel(object? sender, OnJoinedChannelArgs args)
    {
        connectionTimeout = 0.0f;

        if (args.Channel != channelName || client.JoinedChannels.Count > 1)
            Disconnect();
    }

    private void OnClientChatCommandReceived(object? sender, OnChatCommandReceivedArgs args)
    {
    }

    private struct Config
    {
        public string DefaultChannel { get; init; }
        public string Username { get; init; }
        public string AccessToken { get; init; }

        public static Config ReadFromFile(string path)
        {
            File configFile = new();
            if (configFile.Open(path, File.ModeFlags.Read) is Error err && err != Error.Ok)
                throw new Exception(Enum.GetName(typeof(Error), err));
            Config config = JsonSerializer.Deserialize<Config>(configFile.GetAsText());
            configFile.Close();
            return config;
        }
    }
}
