using AMS.Domain.Entities;

namespace AMS.Domain.Entities.Cfg;

/// <summary>
/// Represents a system configuration setting stored in the database.
/// Allows dynamic configuration without code deployment.
/// </summary>
public class AppSetting : BaseEntity
{
    /// <summary>
    /// Unique key for the setting.
    /// </summary>
    public string SettingKey { get; set; } = string.Empty;

    /// <summary>
    /// Value of the setting.
    /// </summary>
    public string SettingValue { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this setting controls.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category for grouping settings (e.g., "Tax", "Invoice", "General").
    /// </summary>
    public string Category { get; set; } = "General";

    /// <summary>
    /// Indicates if the setting value should be encrypted.
    /// </summary>
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// Date from which this setting is effective.
    /// </summary>
    public DateTime? EffectiveFrom { get; set; }

    /// <summary>
    /// Date until which this setting is effective.
    /// </summary>
    public DateTime? EffectiveTo { get; set; }
}