namespace Financist.Application.Common.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException(string message)
        : this(new Dictionary<string, string[]>
        {
            ["request"] = [message]
        })
    {
    }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
