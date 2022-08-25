using AlphaPaulloz.Core;
using AlphaPaulloz.Twitch;
using Godot;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;

namespace AlphaPaulloz;

public partial class TwitchBot : Node, ITwitchService
{
    private const string CONFIGURATION_FILE_PATH = "user://config.json";
    private const float CONNECTION_TIMEOUT = 10.0f;

    private Config config;

    private readonly TwitchAPI api = new();
    private TwitchClient client = new();
    private float connectionTimeout = 0.0f;
    private string channelName;

    public bool Connected => client.IsConnected && !Connecting;
    public bool Connecting => connectionTimeout > 0.0f;
    public string ChannelName => channelName;

    public TwitchBot()
    {
        config = Config.ReadFromFile(CONFIGURATION_FILE_PATH);
        channelName = config.DefaultChannel;
        api.Settings.ClientId = config.ClientID;
        api.Settings.Secret = config.ClientSecret;
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
        Task.Run(() => ValidateRefreshAndConnect());
    }

    public void Disconnect()
    {
        if (!Connected && !Connecting) return;

        connectionTimeout = 0.0f;
        client.Disconnect();

        Locator.Logger.Log("Disconnected.");

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

    private async void ValidateRefreshAndConnect()
    {
        if (await ValidateAccessToken())
            client.Connect();
        else
        {
            try
            {
                await RefreshAccessToken();
                config.SaveToFile(CONFIGURATION_FILE_PATH);
            }
            catch (Exception exception)
            {
                OnClientError(client, new OnErrorEventArgs { Exception = exception });
                Disconnect();
                return;
            }

            if (await ValidateAccessToken())
                client.Connect();
            else
                Disconnect();
        }
    }

    private async Task<bool> ValidateAccessToken()
    {
        return (await api.Auth.ValidateAccessTokenAsync(config.AccessToken))?.ExpiresIn > 0;
    }

    private async Task RefreshAccessToken()
    {
        var refresh = await api.Auth.RefreshAuthTokenAsync(config.RefreshToken, config.ClientSecret, config.ClientID);
        if (refresh == null)
            throw new Exception("Could not refresh token.");

        config = config with
        {
            AccessToken = refresh.AccessToken,
            RefreshToken = refresh.RefreshToken,
        };

        TeardownClient();
        SetupClient();
    }

    private void SetupClient()
    {
        client = new();
        client.Initialize(new ConnectionCredentials(config.Username, config.AccessToken));

        client.OnError += OnClientError;
        client.OnConnected += OnClientConnected;
        client.OnConnectionError += OnClientConnectionError;
        client.OnIncorrectLogin += OnClientIncorrectLogin;
        client.OnJoinedChannel += OnClientJoinedChannel;
        client.OnChatCommandReceived += OnClientChatCommandReceived;
    }

    private void TeardownClient()
    {
        client.Disconnect();

        client.OnError -= OnClientError;
        client.OnConnected -= OnClientConnected;
        client.OnConnectionError -= OnClientConnectionError;
        client.OnIncorrectLogin -= OnClientIncorrectLogin;
        client.OnJoinedChannel -= OnClientJoinedChannel;
        client.OnChatCommandReceived -= OnClientChatCommandReceived;
    }

    private void OnClientError(object? sender, OnErrorEventArgs args)
    {
        Locator.Logger.LogErr(args.Exception.Message);
    }

    private void OnClientConnectionError(object? sender, OnConnectionErrorArgs args)
    {
        Locator.Logger.LogErr(args.Error.Message);
        Disconnect();
    }

    private void OnClientIncorrectLogin(object? sender, OnIncorrectLoginArgs args)
    {
        Locator.Logger.LogErr(args.Exception.Message);
        Disconnect();
    }

    private void OnClientConnected(object? sender, OnConnectedArgs args)
    {
        client.JoinChannel(channelName);

        Locator.Logger.Log($"Connected as: {config.Username}.");
    }

    private void OnClientJoinedChannel(object? sender, OnJoinedChannelArgs args)
    {
        connectionTimeout = 0.0f;

        if (args.Channel != channelName || client.JoinedChannels.Count > 1)
            Disconnect();

        Locator.Logger.Log($"Joined channel: {channelName}.");
    }

    private void OnClientChatCommandReceived(object? sender, OnChatCommandReceivedArgs args)
    {
    }

    private struct Config
    {
        public string ClientID { get; init; }
        public string ClientSecret { get; init; }

        public string Username { get; init; }
        public string DefaultChannel { get; init; }

        public string AccessToken { get; init; }
        public string RefreshToken { get; init; }

        public void SaveToFile(string path)
        {
            File configFile = new();
            if (configFile.Open(path, File.ModeFlags.Write) is Error err && err != Error.Ok)
                throw new Exception(Enum.GetName(typeof(Error), err));
            configFile.StoreString(JsonSerializer.Serialize(this, typeof(Config),
                                                            new JsonSerializerOptions { WriteIndented = true }));
            configFile.Close();
        }

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
