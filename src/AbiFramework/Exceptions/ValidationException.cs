namespace AbiFramework.Exceptions;

public class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : this()
    {
        Errors = errors;
    }

    public ValidationException(IEnumerable<KeyValuePair<string, string[]>> errors)
        : this()
    {
        Errors = errors.ToDictionary(e => e.Key, e => e.Value);
    }

    public ValidationException(string propertyName, string errorMessage)
        : this()
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }

    public IDictionary<string, string[]> Errors { get; }
}

