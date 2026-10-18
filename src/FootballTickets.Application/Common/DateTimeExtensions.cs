namespace FootballTickets.Application.Common;
/*класс для работы с временем, чтобы не выбрасывало исключение каждый раз
 тк Npgsql не принимает DateTime с Kind = Unspecified для столбца timestamptz
 */
public static class DateTimeExtensions
{
    public static DateTime ToUtc(this DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
