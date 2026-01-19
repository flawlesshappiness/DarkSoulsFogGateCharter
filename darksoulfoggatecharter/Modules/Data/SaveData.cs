using System;
using System.Text.Json.Serialization;

public abstract class SaveData
{
    public string Version { get; set; } = string.Empty;
    public bool IsRelease { get; set; } = false;
    public string TimeUpdated { get; set; } = string.Empty;
    public int? Profile { get; set; } = null;
    public bool Deleted { get; set; } = false;

    public void UpdateVersion() => Version = ApplicationInfo.Instance.Version;
    public void UpdateRelease() => IsRelease = ApplicationInfo.Instance.Release;
    public void UpdateTimestamp() => TimeUpdated = DateTime.UtcNow.ToString();

    [JsonIgnore]
    public DateTime DateTimeUpdated => DateTime.TryParse(TimeUpdated, out var result) ? result : new DateTime();

    public event Action OnBeforeSave;
    public event Action OnAfterSave;

    public void Update()
    {
        UpdateVersion();
        UpdateRelease();
        UpdateTimestamp();
    }

    public void Save()
    {
        OnBeforeSave?.Invoke();
        SaveDataController.Instance.Save(this);
        OnAfterSave?.Invoke();
    }
}
