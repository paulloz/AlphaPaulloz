namespace AlphaPaulloz.Core;

public interface ILoggerService : IService
{
    public void Log(string message);
    public void LogErr(string message);
}
