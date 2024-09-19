namespace FishinC.UI;

/// <summary>
/// Holds quotes used for flavor text.
/// </summary>
internal static class Quotes
{
    private static readonly List<string> keys = [];

    // Set in Init.
    private static ITranslationHelper translations = null!;

    /// <summary>
    /// Initializes the quote list from the current translation keys.
    /// </summary>
    /// <param name="translations">Mod translation helper.</param>
    public static void Init(ITranslationHelper translations)
    {
        Quotes.translations = translations;
        keys.AddRange(
            translations
                .GetTranslations()
                .Select(t => t.Key)
                .Where(key => key.StartsWith("FlavorQuote."))
        );
    }

    /// <summary>
    /// Gets a random quote.
    /// </summary>
    /// <returns>A random selection from the <c>FlavorQuote</c> entries, or an empty string if the
    /// list is empty.</returns>
    public static string GetRandomQuote()
    {
        if (keys.Count == 0)
        {
            return "";
        }
        var index = Game1.random.Next(10);
        string rawQuote = translations.Get(keys[index]);
        var separatorIndex = rawQuote.IndexOf('|');
        return separatorIndex >= 0
            ? FormatQuote(rawQuote[(separatorIndex + 1)..], rawQuote[0..separatorIndex])
            : FormatQuote(rawQuote);
    }

    private static string FormatQuote(string quote, string? person = null)
    {
        var quotedQuote = "\"" + quote + "\"";
        return !string.IsNullOrEmpty(person) ? quotedQuote + "\n  --" + person : quotedQuote;
    }
}
