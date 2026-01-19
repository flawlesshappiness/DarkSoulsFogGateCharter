using Godot;
using System.Collections;

public static class AnimationPlayerExtensions
{
    public static Coroutine PlayAndWaitForAnimation(this AnimationPlayer player, string animation)
    {
        player.Play(animation);
        player.AnimationFinished += AnimationFinished;

        bool animation_finished = false;
        var id_cr = $"{nameof(PlayAndWaitForAnimation)}_{player.GetInstanceId()}_{GameTime.UnscaledTime}";
        return Coroutine.Start(WaitForAnimationCr(), id_cr, player)
            .SetRunWhilePaused();

        IEnumerator WaitForAnimationCr()
        {
            while (!animation_finished)
            {
                yield return null;
            }
        }

        void AnimationFinished(StringName animation_name)
        {
            if (animation_name.ToString() == animation)
            {
                animation_finished = true;
                player.AnimationFinished -= AnimationFinished;
            }
        }
    }
}
