using Acidmanic.Utilities.NamingConventions;

namespace Cubism.Shared.Exceptions;

public class ExceptionBase(string id, string message) : Exception(message)
{
    public string Id { get; } = id;
}

public class AutoMessageExceptionBase<T>(string id) : ExceptionBase(id, GetMessage())
{
    private static readonly ConventionDescriptor prettyMessageDescription = new()
    {
        Delimiter = " ",
        Name = "Pretty Message",
        PreFix = string.Empty,
        SegmentCase = i => i == 0 ? Case.Upper : Case.Lower,
        Separation = Separation.ByDelimiter
    };

    private static string GetMessage()
    {
        var message = typeof(T).Name;

        if (message.EndsWith("Exception", StringComparison.OrdinalIgnoreCase))
        {
            message = message.Substring(0, message.Length - 9);
        }

        var nc = new NamingConvention();

        message = nc.Convert(message, prettyMessageDescription);

        return message;
    }
}