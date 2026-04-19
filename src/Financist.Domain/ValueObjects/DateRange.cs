using Financist.Domain.Common;

namespace Financist.Domain.ValueObjects;

public sealed class DateRange : IEquatable<DateRange>
{
    private DateRange()
    {
    }

    public DateRange(DateOnly start, DateOnly end)
    {
        if (end < start)
        {
            throw new DomainException("Date range end cannot be earlier than the start.");
        }

        Start = start;
        End = end;
    }

    public DateOnly Start { get; private set; }

    public DateOnly End { get; private set; }

    public bool Contains(DateOnly date)
    {
        return date >= Start && date <= End;
    }

    public bool Equals(DateRange? other)
    {
        return other is not null && Start == other.Start && End == other.End;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as DateRange);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }
}
