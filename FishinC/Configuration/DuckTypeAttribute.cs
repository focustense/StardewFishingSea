namespace FishinC.Configuration;

/// <summary>
/// Marks a type for conversion using StardewUI's duck-type converter.
/// </summary>
/// <remarks>
/// This is required for some two-way bindings in order to allow conversion back to our own type.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class DuckTypeAttribute : Attribute { }
