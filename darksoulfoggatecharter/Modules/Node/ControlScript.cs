using Godot;
using System;
using System.Collections;

public partial class ControlScript : Control
{
    private bool _initialized;
    private bool _visible;

    public Coroutine StartCoroutine(Func<IEnumerator> enumerator, string id = null)
    {
        id = (id ?? string.Empty) + GetInstanceId();
        return Coroutine.Start(enumerator, id, this);
    }

    public override void _Ready()
    {
        base._Ready();
        VisibilityChanged += OnVisibilityChanged;
    }

    protected virtual void Initialize()
    {
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Process_Initialize();
    }

    private void Process_Initialize()
    {
        if (!_initialized)
        {
            _initialized = true;
            Initialize();
        }
    }

    protected virtual void OnShow()
    {

    }

    protected virtual void OnHide()
    {

    }

    protected void ReleaseCurrentFocus()
    {
        GetViewport().GuiReleaseFocus();
    }

    protected virtual void OnVisibilityChanged()
    {
        if (!_initialized) return;
        if (Visible)
        {
            OnShow();
        }
        else
        {
            OnHide();
        }
    }
}
