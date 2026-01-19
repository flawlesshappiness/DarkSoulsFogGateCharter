using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SoundInfo : Resource
{
    [Export]
    public Array<AudioStream> AudioStreams;

    [Export]
    public float Volume = 0f;

    [Export]
    public bool Looping;

    [Export]
    public Vector2 PitchRange = new Vector2(1, 1);

    [Export]
    public string Bus = "SFX";

    [Export]
    public SoundDistance Distance = SoundDistance.Default;

    [Export]
    public SoundAttenuation Attenuation = SoundAttenuation.Default;

    public AudioStreamPlayer Play(SoundOverride settings = null) => SoundController.Instance.Play(this, settings);
    public AudioStreamPlayer3D Play(Vector3 position, SoundOverride settings = null) => SoundController.Instance.Play(this, position, settings);
    public AudioStreamPlayer3D Play(Node3D target, SoundOverride settings = null) => SoundController.Instance.Play(this, target, settings);
}

public enum SoundDistance
{
    Default, Near, Far
}

public enum SoundAttenuation
{
    Default, Disabled
}
