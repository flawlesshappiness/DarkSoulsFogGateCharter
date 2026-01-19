using FlawLizArt.Animation.StateMachine;
using Godot;
using System.Collections;
using System.Collections.Generic;

[GlobalClass]
public partial class AnimationStateMachine : BaseStateMachine
{
    [Export]
    public AnimationPlayer Animator;

    public Dictionary<string, AnimationState> Animations = new();

    private Coroutine cr_variation;

    public override void _Ready()
    {
        base._Ready();
        Animator.AnimationFinished += AnimationFinished;
    }

    public AnimationState CreateAnimation(string animation, bool looping) => CreateAnimation(animation, animation, looping);
    public AnimationState CreateAnimation(string state, string animation, bool looping)
    {
        var animation_state = new AnimationState(animation)
        {
            Looping = looping,
            Node = CreateNode(state)
        };

        Animations.Add(state, animation_state);
        return animation_state;
    }

    public void Connect(AnimationState start, AnimationState end, params Condition[] conditions)
    {
        Connect(start.Node, end.Node, conditions);
    }

    protected virtual void AnimationFinished(StringName animName)
    {
        TryProcessCurrentState(true);
    }

    public override void SetCurrentState(StateNode node)
    {
        base.SetCurrentState(node);

        if (Animations.TryGetValue(node.Name, out var state))
        {
            var animation_name = state.Animation;
            var animation = Animator.GetAnimation(animation_name);
            animation.LoopMode = state.Looping ? Animation.LoopModeEnum.Linear : Animation.LoopModeEnum.None;
            Animator.Play(animation_name);

            StopVariation();
            if (state.HasVariations())
            {
                AnimateVariation(state);
            }
        }
    }

    private void StopVariation()
    {
        Coroutine.Stop(cr_variation);
    }

    private Coroutine AnimateVariation(AnimationState state)
    {
        var animation = Animator.GetAnimation(state.Animation);
        cr_variation = this.StartCoroutine(Cr, "variation");
        return cr_variation;

        IEnumerator Cr()
        {
            while (true)
            {
                yield return new WaitForSeconds(animation.Length);
                var variation = state.GetVariation();
                if (variation != state)
                {
                    SetCurrentState(variation.Node);
                }
            }
        }
    }
}
