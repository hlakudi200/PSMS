using psms.Debugging;

namespace psms;

public class psmsConsts
{
    public const string LocalizationSourceName = "psms";

    public const string ConnectionStringName = "Default";

    public const bool MultiTenancyEnabled = true;


    /// <summary>
    /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
    /// </summary>
    public static readonly string DefaultPassPhrase =
        DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "dbb1851252eb4b19b246c97f98c05ed3";
}
