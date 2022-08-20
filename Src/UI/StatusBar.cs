using AlphaPaulloz.Core;
using Godot;

namespace AlphaPaulloz.UI;

public partial class StatusBar : Panel
{
    [Export]
    private Label? statusLabel;

    public override void _Process(float delta)
    {
        (string text, Color color) = EvaluateStatus();

        statusLabel!.Text = text;
        statusLabel!.AddThemeColorOverride("font_color", color);
    }

    private (string, Color) EvaluateStatus() => Locator.TwitchService.Connected switch
    {
        true => ("Connected", GetThemeColor("success_color", "Editor")),
        false when Locator.TwitchService.Connecting => ("Connecting", GetThemeColor("warning_color", "Editor")),
        false => ("Not connected", GetThemeColor("error_color", "Editor")),
    };
}
