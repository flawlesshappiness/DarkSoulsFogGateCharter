using Godot;

public static class ScrollContainerExtensions
{
    public static void ScrollVerticalToBottom(this ScrollContainer container) => container.ScrollVertical = int.MaxValue;
    public static void ScrollVerticalToTop(this ScrollContainer container) => container.ScrollVertical = 0;
    public static void ScrollHorizontalToBottom(this ScrollContainer container) => container.ScrollHorizontal = int.MaxValue;
    public static void ScrollHorizontalToTop(this ScrollContainer container) => container.ScrollHorizontal = 0;
}
