namespace WebExecutor
{
    public interface IDebugConsole
    {
        void AppendFolded(string message);
        void AppendMessage(MessageKind kind, string message);
        void Clear();
    }

    public enum MessageKind
    {
        Info,
        Debug,
        Error,
    }
}