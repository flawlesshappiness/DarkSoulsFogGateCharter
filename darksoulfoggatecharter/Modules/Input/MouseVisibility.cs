using Godot;

public partial class MouseVisibility : Node
{
    public static MouseVisibility Instance { get; private set; }

    public readonly MultiLock Lock = new MultiLock();

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        Lock.OnFree += OnFree;
        ProcessMode = ProcessModeEnum.Always;
    }

    private void OnFree()
    {
        HideMouse();
    }

    public override void _Input(InputEvent e)
    {
        base._Input(e);

        if (e is InputEventMouse mouse && mouse != null)
        {
            ShowMouse();
        }
        else if (e is InputEventMouseMotion mmotion && mmotion != null && mmotion.Relative.Length() > 0)
        {
            ShowMouse();
        }
        else if (e is InputEventJoypadButton joypad && joypad != null)
        {
            HideMouse();
        }
        else if (e is InputEventJoypadMotion jmotion && jmotion != null && Mathf.Abs(jmotion.AxisValue) > 0.25f)
        {
            HideMouse();
        }
    }

    private void ShowMouse()
    {
        Input.MouseMode = Lock.IsLocked ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
    }

    private void HideMouse()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public static void Show(string lock_name)
    {
        SetVisible(lock_name, true);
    }

    public static void Hide(string lock_name)
    {
        SetVisible(lock_name, false);
    }

    public static void SetVisible(string id, bool visible)
    {
        Instance.Lock.SetLock(id, visible);
    }
}
