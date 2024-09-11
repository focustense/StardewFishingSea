﻿namespace FishingBuddy;

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
    /// Resets the configuration to default settings.
    /// </summary>
    void Reset();

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

    public void Reset()
    {
        Config = new();
    }

    public void Save()
    {
        helper.WriteConfig(Config);
    }
}
