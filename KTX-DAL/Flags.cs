namespace KTX_DAL
{
    [Flags]
    public enum QueryFlags
    {
        None = -1,
        Enabled = 0,
        Disabled = 1
    }

    [Flags]
    public enum LoggerFlags
    {
        Error,
        Warning
    }

}