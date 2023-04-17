namespace WebApi.Logger
{
    public interface ILoggingService
    {
        public void LogInformation(string message, params object[] parameters);
    }
}
