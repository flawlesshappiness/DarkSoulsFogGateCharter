using Godot;
using System.Collections.Generic;

public class WeightedRandom<T>
{
    private List<Element> elements = new List<Element>();
    private float max_weight = 0f;

    private RandomNumberGenerator rng;

    public int Count { get { return elements.Count; } }

    public class Element
    {
        public T value;
        public float weight;

        public Element(T value, float weight)
        {
            this.value = value;
            this.weight = weight;
        }
    }

    public WeightedRandom(List<Element> elements = null, RandomNumberGenerator rng = null)
    {
        this.rng = rng ?? new RandomNumberGenerator();
        elements ??= new List<Element>();

        foreach (var element in elements)
        {
            AddElement(element);
        }
    }

    public void AddElement(Element element)
    {
        elements.Add(element);
        max_weight += element.weight;
    }

    public void AddElement(T value, float weight)
    {
        AddElement(new Element(value, weight));
    }

    public T Random()
    {
        float r_weight = rng.RandfRange(0f, max_weight);
        float temp_weight = 0f;
        foreach (var element in elements)
        {
            temp_weight += element.weight;
            if (r_weight < temp_weight) return element.value;
        }

        return elements[elements.Count - 1].value;
    }
}
