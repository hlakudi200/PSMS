namespace psms.Shared;

/// <summary>
/// Helper for masking personally identifiable information in API responses.
/// Shows only the last 4 characters, replacing the rest with asterisks.
/// </summary>
public static class PiiMasking
{
    /// <summary>
    /// Masks a value showing only the last 4 characters.
    /// Example: "9501015800085" → "*********0085"
    /// </summary>
    public static string Mask(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        if (value.Length <= 4)
            return new string('*', value.Length);

        return new string('*', value.Length - 4) + value[^4..];
    }

    /// <summary>
    /// Whether a value is one of these masks rather than a real identifier.
    /// <para>
    /// A mask makes a round trip whenever a screen loads a record, shows it in
    /// a field and saves it again — and on the way back it is indistinguishable
    /// from a deliberate edit, so it overwrites the real number with asterisks.
    /// No ID number or passport contains one, so a value that does was never
    /// typed by a person and must never be stored.
    /// </para>
    /// </summary>
    public static bool LooksMasked(string value)
    {
        return !string.IsNullOrEmpty(value) && value.Contains('*');
    }
}
