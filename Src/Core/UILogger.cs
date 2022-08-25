using Godot;
using System;

namespace AlphaPaulloz.Core;

public partial class UILogger : Node, ILoggerService
{
    [Export]
    private RichTextLabel? container;

    private Color LogColor => GetParent<Control>().GetThemeColor("readonly_font_color", "Editor");
    private Color ErrorColor => GetParent<Control>().GetThemeColor("error_color", "Editor");

    public override void _EnterTree()
    {
        Locator.Provide(this);
    }

    public override void _ExitTree()
    {
        Locator.Provide<ILoggerService>(null);
    }

    public void Log(string message)
    {
        GD.Print(message);
        PushLine(message, LogColor);
    }

    public void LogErr(string message)
    {
        GD.PrintErr(message);
        PushLine(message, ErrorColor);
    }

    private void PushLine(string message, Color color)
    {
        container!.Text += $"[color=#{color.ToHTML()}][{DateTime.Now:HH:mm:ss}] {message}[/color]\n";
    }
}
