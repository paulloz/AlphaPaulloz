using AlphaPaulloz.Core;
using Godot;

namespace AlphaPaulloz.UI;

public partial class MessageTab : VBoxContainer
{
    [Export]
    private TextEdit? messageInput;

    private void OnSendButtonPressed()
    {
        var message = messageInput!.Text;
        messageInput!.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(message)) return;

        Locator.TwitchService.SendMessage(message);
    }

    private void OnMessageInputGUIInput(InputEvent input)
    {
        if (input is not InputEventKey keyInput) return;
        if (!keyInput.Pressed) return;
        if (keyInput.Keycode != Key.Enter || !keyInput.CtrlPressed) return;

        AcceptEvent();
        OnSendButtonPressed();
    }
}
