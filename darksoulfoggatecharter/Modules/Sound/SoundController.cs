using Godot;
using System;
using System.Collections;
using System.Linq;

public partial class SoundController : ResourceController<SoundCollection, SoundInfo>
{
    public override string Directory => $"{Paths.Modules}/Sound";
    protected override string CustomResourceCollectionDirectory => "Sounds";
    public static SoundController Instance => Singleton.Get<SoundController>();

    public AudioStreamPlayer Play(SoundInfo info, SoundOverride settings = null) => Play(info.ResourcePath, settings);
    public AudioStreamPlayer Play(string name, SoundOverride settings = null)
    {
        var asp = CreateAudioStreamPlayer();
        Play(asp, Collection.GetEntry(name), settings);
        return asp;
    }

    public AudioStreamPlayer3D Play(SoundInfo info, Vector3 position, SoundOverride settings = null) => Play(info.ResourcePath, position, settings);
    public AudioStreamPlayer3D Play(string name, Vector3 position, SoundOverride settings = null)
    {
        var asp = CreateAudioStreamPlayer(position);
        Play(asp, Collection.GetEntry(name), settings);
        return asp;
    }

    public AudioStreamPlayer3D Play(SoundInfo info, Node3D target, SoundOverride settings = null) => Play(info.ResourcePath, target, settings);
    public AudioStreamPlayer3D Play(string name, Node3D target, SoundOverride settings = null)
    {
        var asp = CreateAudioStreamPlayer(target);
        Play(asp, Collection.GetEntry(name), settings);
        return asp;
    }

    private AudioStreamPlayer CreateAudioStreamPlayer()
    {
        var asp = new AudioStreamPlayer();
        asp.SetParent(Scene.Root);
        return asp;
    }

    private AudioStreamPlayer3D CreateAudioStreamPlayer(Vector3 position)
    {
        var asp = new AudioStreamPlayer3D();
        asp.SetParent(Scene.Root);
        asp.GlobalPosition = position;
        return asp;
    }

    private AudioStreamPlayer3D CreateAudioStreamPlayer(Node3D target)
    {
        var asp = new AudioStreamPlayer3D();
        asp.SetParent(target);
        asp.Position = Vector3.Zero;
        return asp;
    }

    private void Play(GodotObject go, SoundEntry entry, SoundOverride settings = null)
    {
        if (!IsInstanceValid(go)) return;
        if (entry == null) return;

        SetupAudioStreamPlayer(go, entry, settings);
        go.Call("play", settings?.PlaybackPosition ?? 0);
    }

    private void DestroyDelay(GodotObject go, float delay)
    {
        var node = go as Node;

        Coroutine.Start(Cr);
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);

            if (IsInstanceValid(node) && !node.IsQueuedForDeletion())
            {
                node.QueueFree();
            }
        }
    }

    private void SetupAudioStreamPlayer(GodotObject go, SoundEntry entry, SoundOverride settings = null)
    {
        if (!IsInstanceValid(go)) return;

        // Base
        var info = entry.Info;
        var rng = new RandomNumberGenerator();
        var bus = (settings?.Bus ?? info.Bus).ToString();
        var pitch_range = settings?.PitchRange ?? info.PitchRange;
        var pitch_scale = rng.RandfRange(pitch_range.X, pitch_range.Y);
        var volume = settings?.Volume ?? info.Volume;
        var stream = GetRandomAudioStream(entry);

        if (stream == null)
        {
            Debug.LogError($"SetupAudioStreamPlayer for {info.ResourcePath}: Stream was null");
            return;
        }

        // 3D
        var distance = settings?.Distance ?? info.Distance;
        var max_distance = distance switch
        {
            SoundDistance.Near => 8f,
            SoundDistance.Far => 32f,
            _ => 16f
        };
        var unit_size = distance switch
        {
            SoundDistance.Near => 8f,
            SoundDistance.Far => 20,
            _ => 10f
        };
        var attenuation = settings?.Attenuation ?? info.Attenuation;
        var attenuation_filter_cutoff_hz = attenuation switch
        {
            SoundAttenuation.Disabled => 20500f,
            _ => 7000f
        };

        // Set values
        go.Set("bus", bus);
        go.Set("pitch_scale", pitch_scale);
        go.Set("volume_db", volume);
        go.Set("stream", stream);
        go.Set("max_distance", max_distance);
        go.Set("unit_size", unit_size);
        go.Set("attenuation_filter_cutoff_hz", attenuation_filter_cutoff_hz);
        go.Set("max_db", 10); // Subject to change

        // Destroy if not looping
        if (!info.Looping)
        {
            var duration = Convert.ToSingle(stream.GetLength());
            DestroyDelay(go, duration);
        }
    }

    private AudioStream GetRandomAudioStream(SoundEntry entry)
    {
        var rng = new RandomNumberGenerator();
        var allow_duplicate = entry.Info.AudioStreams.Count == 1;
        var stream = entry.Info.AudioStreams
            .Where(x => allow_duplicate || x != entry.LastPlayedStream)
            .ToList()
            .Random();
        entry.LastPlayedStream = stream;
        return stream;
    }
}

public class SoundOverride
{
    public string? Bus { get; set; }
    public float? Volume { get; set; }
    public Vector2? PitchRange { get; set; } = new Vector2(1, 1);
    public float? PlaybackPosition { get; set; }

    // 3D
    public SoundDistance? Distance { get; set; }
    public SoundAttenuation? Attenuation { get; set; }
}