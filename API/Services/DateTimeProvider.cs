namespace API.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetDateTimeNow()
    {
        return DateTime.Now;
    }

    public DateTime GetUtcDateTimeNow()
    {
        return DateTime.UtcNow;
    }
}

public interface IDateTimeProvider
{
    DateTime GetDateTimeNow();
    DateTime GetUtcDateTimeNow();
}
