using Godot;
using System.Collections;

public static class AudioStreamPlayerExtensions
{
    public static Coroutine FadeOut(this AudioStreamPlayer3D asp, float duration) =>
        _FadeOut(asp, duration);

    public static Coroutine FadeOut(this AudioStreamPlayer2D asp, float duration) =>
        _FadeOut(asp, duration);

    public static Coroutine FadeOut(this AudioStreamPlayer asp, float duration) =>
        _FadeOut(asp, duration);

    private static Coroutine _FadeOut(Node node, float duration)
    {
        return Coroutine.Start(Cr, "fade_" + node.GetInstanceId(), node);
        IEnumerator Cr()
        {
            var start = AudioMath.DecibelToPercentage(node.Get("volume_db").AsSingle());
            var end = 0f;
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                var t = Mathf.Lerp(start, end, f);
                var db = AudioMath.PercentageToDecibel(t);
                node.Set("volume_db", db);
            });
        }
    }

    public static Coroutine FadeIn(this AudioStreamPlayer3D asp, float duration, float volume) =>
        _FadeIn(asp, duration, volume);

    public static Coroutine FadeIn(this AudioStreamPlayer2D asp, float duration, float volume) =>
        _FadeIn(asp, duration, volume);

    public static Coroutine FadeIn(this AudioStreamPlayer asp, float duration, float volume) =>
        _FadeIn(asp, duration, volume);

    private static Coroutine _FadeIn(Node node, float duration, float volume)
    {
        return Coroutine.Start(Cr, "fade_" + node.GetInstanceId(), node);
        IEnumerator Cr()
        {
            var end = AudioMath.DecibelToPercentage(volume);
            yield return LerpEnumerator.Lerp01(duration, f =>
            {
                var t = Mathf.Lerp(0f, end, f);
                var db = AudioMath.PercentageToDecibel(t);
                node.Set("volume_db", db);
            });
        }
    }

    /// <summary>
    /// Sets AudioStreamPlayer's playing toggle, without requeing
    /// </summary>
    public static void SetPlaying(this AudioStreamPlayer3D asp, bool playing) =>
        _SetPlaying(asp, playing);

    /// <summary>
    /// Sets AudioStreamPlayer's playing toggle, without requeing
    /// </summary>
    public static void SetPlaying(this AudioStreamPlayer2D asp, bool playing) =>
        _SetPlaying(asp, playing);

    /// <summary>
    /// Sets AudioStreamPlayer's playing toggle, without requeing
    /// </summary>
    public static void SetPlaying(this AudioStreamPlayer asp, bool playing) =>
        _SetPlaying(asp, playing);

    private static void _SetPlaying(Node node, bool playing)
    {
        var id = "playing";
        var currently_playing = node.Get(id).AsBool();

        if (playing != currently_playing)
        {
            node.Set(id, playing);
        }
    }
}
