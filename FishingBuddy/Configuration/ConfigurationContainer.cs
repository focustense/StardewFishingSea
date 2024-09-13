namespace FishingBuddy.Configuration;

/// <summary>
/// Container for the mod configuration; used for GMCM, etc.
/// </summary>
/// <typeparam name="T">Type of configuration data.</typeparam>
public interface IConfigurationContainer<T>
{
    /// <summary>
    /// The current configuration.
    /// </summary>
    T Config { get; }

    /// <summary>
    /// Gets a new instance of the configuration data holding the default settings.
    /// </summary>
    T GetDefault();

    /// <summary>
    /// Saves the current <see cref="Config"/> to disk.
    /// </summary>
    void Save();
}

/// <summary>
/// Standard configuration container using SMAPI's mod helper.
/// </summary>
/// <typeparam name="T">Type of configuration data.</typeparam>
/// <param name="helper">SMAPI mod helper provided to Entry class.</param>
public class ConfigurationContainer<T>(IModHelper helper) : IConfigurationContainer<T>
    where T : class, new()
{
    public T Config { get; private set; } = helper.ReadConfig<T>();

    public T GetDefault()
    {
        return new();
    }

    public void Save()
    {
        helper.WriteConfig(Config);
    }
}
