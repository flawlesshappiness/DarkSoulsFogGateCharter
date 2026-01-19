using Godot;
using System.Collections.Generic;

public partial class AudioBusController : SingletonController
{
    public override string Directory => $"{Paths.Modules}/Audio";
    public static AudioBusController Instance => Singleton.Get<AudioBusController>();

    private Dictionary<string, AudioBus> audio_busses = new();

    public AudioBus GetAudioBus(string name)
    {
        if (!audio_busses.ContainsKey(name))
        {
            var idx = AudioServer.GetBusIndex(name);
            if (idx >= 0)
            {
                audio_busses.Add(name, new AudioBus(Name, idx));
            }
        }

        return audio_busses[name];
    }
}
