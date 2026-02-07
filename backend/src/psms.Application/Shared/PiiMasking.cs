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
}
