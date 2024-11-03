namespace LunaHost.HTTP.Interface
{
    public enum UrlType
    {
        None = 0,   // No special matching behavior
        Match = 1,  // Exact match (e.g., "/something" matches "/something")
        After = 2   // Partial match (e.g., "/something" matches "/something/else")
    }


}
