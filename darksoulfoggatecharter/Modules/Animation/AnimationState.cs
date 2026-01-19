using FlawLizArt.Animation.StateMachine;

public class AnimationState
{
    public string Animation { get; private set; } = string.Empty;
    public StateNode Node { get; set; }
    public bool Looping { get; set; }

    private WeightedRandom<AnimationState> Variations = new();

    public AnimationState(string animation)
    {
        Animation = animation;
        AddVariation(this, 1.0f);
    }

    public void AddVariation(AnimationState state, float weight)
    {
        Variations.AddElement(state, weight);
    }

    public AnimationState GetVariation()
    {
        return Variations.Random();
    }

    public bool HasVariations()
    {
        return Variations.Count > 0;
    }
}
