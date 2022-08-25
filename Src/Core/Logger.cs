using Godot;

namespace AlphaPaulloz.Core;

public class DefaultLogger : ILoggerService
{
    public void Log(string message)
    {
        GD.Print(message);
    }

    public void LogErr(string message)
    {
        GD.PrintErr(message);
    }
}
