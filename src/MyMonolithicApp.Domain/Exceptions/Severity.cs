namespace MyMonolithicApp.Domain.Exceptions
{
    public enum Severity : byte
    {
        Correctable,
        Unrecoverable,
        Unexpected,
    }
}
