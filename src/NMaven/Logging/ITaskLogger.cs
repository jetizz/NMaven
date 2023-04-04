namespace NMaven.Logging
{
    public interface ITaskLogger
    {
        void LogMessage(string message);
        void LogImportantMessage(string message);
        void LogWarning(string warning);
        void LogError(string error);
    }
}
