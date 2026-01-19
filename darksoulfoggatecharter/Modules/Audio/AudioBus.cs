using Godot;
using System.Collections.Generic;
using System.Linq;

public class AudioBus
{
    public string Name { get; set; }
    public int Index { get; set; }
    public int EffectCount { get; set; }
    public List<AudioEffect> Effects { get; set; } = new();
    public static AudioBus Get(string name) => AudioBusController.Instance.GetAudioBus(name);

    public AudioBus(string name, int index)
    {
        Name = name;
        Index = index;
        EffectCount = AudioServer.GetBusEffectCount(Index);
        FindEffects();
    }

    public void SetVolume(float volume) => AudioServer.SetBusVolumeDb(Index, volume);

    public float GetVolume() => AudioServer.GetBusVolumeDb(Index);

    private void FindEffects()
    {
        for (int i = 0; i < EffectCount; i++)
        {
            var effect = AudioServer.GetBusEffect(Index, i);
            Effects.Add(effect);
        }
    }

    public int GetEffectIndex<T>() where T : AudioEffect
    {
        var effect = GetEffect<T>();
        return Effects.IndexOf(effect);
    }

    public T GetEffect<T>() where T : AudioEffect
    {
        return Effects.FirstOrDefault(x => x as T != null) as T;
    }

    public void SetEffectEnabled<T>(bool enabled) where T : AudioEffect
    {
        var idx = GetEffectIndex<T>();
        AudioServer.SetBusEffectEnabled(Index, idx, enabled);
    }

    public void SetMuted(bool muted) => AudioServer.SetBusMute(Index, muted);
}
