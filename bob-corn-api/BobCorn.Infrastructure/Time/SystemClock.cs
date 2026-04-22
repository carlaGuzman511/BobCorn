using BobCorn.Application.Abstractions.Time;

namespace BobCorn.Infrastructure.Time
{
    public class SystemClock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
