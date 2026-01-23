using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class UndoController : SingletonController
{
    public static UndoController Instance => Singleton.Get<UndoController>();
    public override string Directory => "Undo";

    private Stack<UndoActionGroup> undo_actions = new();
    private Stack<UndoActionGroup> redo_actions = new();
    private UndoActionGroup current_group = null;

    private abstract class UndoAction
    {
        public virtual string UndoString => GetType().ToString();
        public virtual string RedoString => GetType().ToString();
        public abstract void Undo();
        public abstract void Redo();
    }

    private class CreateNodeAction : UndoAction
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }

        public override string UndoString => $"Remove: {Name}";
        public override string RedoString => $"Create: {Name}, {Position}";

        public override void Redo()
        {
            NodeController.Instance.CreateNode(Name, Position);
        }

        public override void Undo()
        {
            NodeController.Instance.RemoveNode(Name);
        }
    }

    private class CreateConnectionAction : UndoAction
    {
        public string Name { get; set; }

        public override string UndoString => $"Disconnect: {Name.Split(',')[0]} x {Name.Split(',')[1]}";
        public override string RedoString => $"Connect: {Name.Split(',')[0]} > {Name.Split(',')[1]}";

        public override void Redo()
        {
            NodeController.Instance.ConnectNodes(Name);
        }

        public override void Undo()
        {
            NodeController.Instance.RemoveConnectionObject(Name);
        }
    }

    private class MoveNodeAction : UndoAction
    {
        public string Name { get; set; }
        public Vector3 From { get; set; }
        public Vector3 To { get; set; }

        public override string UndoString => $"Move: {Name} {From}";
        public override string RedoString => $"Move: {Name} {To}";

        public override void Redo()
        {
            NodeController.Instance.MoveNode(Name, To);
        }

        public override void Undo()
        {
            NodeController.Instance.MoveNode(Name, From);
        }
    }

    private class SelectNodeAction : UndoAction
    {
        public string Name { get; set; }
        public bool Selected { get; set; }

        public override string UndoString => $"{(Selected ? "Deselect" : "Select")}: {Name}";
        public override string RedoString => $"{(Selected ? "Select" : "Deselect")}: {Name}";

        public override void Redo()
        {
            SelectionController.Instance.SelectNode(Name, Selected);
        }

        public override void Undo()
        {
            SelectionController.Instance.SelectNode(Name, !Selected);
        }
    }

    private class UndoActionGroup
    {
        public Stack<UndoAction> Actions { get; set; } = new();

        public List<UndoAction> GetRedoActions()
        {
            var actions = Actions.ToList();
            actions.Reverse();
            return actions;
        }

        public List<UndoAction> GetUndoList()
        {
            var actions = Actions.ToList();
            actions.Reverse();
            return actions;
        }

        public void Redo()
        {
            GetRedoActions().ForEach(x => x.Redo());
        }

        public void Undo()
        {
            GetUndoList().ForEach(x => x.Undo());
        }
    }

    public override void _Ready()
    {
        base._Ready();
        RegisterDebugActions();
    }

    private void RegisterDebugActions()
    {
        var category = "UNDO / REDO";

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Show undo actions",
            Action = ShowUndoActions
        });

        void ShowUndoActions(DebugView v)
        {
            v.SetContent_List();

            var group = undo_actions.TryPeek(out var result) ? result : new UndoActionGroup();
            foreach (var action in group.GetUndoList())
            {
                v.ContentList.AddText(action.UndoString);
            }
        }

        Debug.RegisterAction(new DebugAction
        {
            Category = category,
            Text = "Show redo actions",
            Action = ShowRedoActions
        });

        void ShowRedoActions(DebugView v)
        {
            v.SetContent_List();

            var group = redo_actions.TryPeek(out var result) ? result : new UndoActionGroup();
            foreach (var action in group.GetUndoList())
            {
                v.ContentList.AddText(action.RedoString);
            }
        }
    }

    public void Clear()
    {
        undo_actions.Clear();
        redo_actions.Clear();
        current_group = null;
    }

    public void StartUndoAction()
    {
        current_group = new UndoActionGroup();
    }

    public void EndUndoAction()
    {
        undo_actions.Push(current_group);
        redo_actions.Clear();
        current_group = null;
    }

    private void AddUndoAction(UndoAction action)
    {
        current_group ??= new UndoActionGroup();
        current_group.Actions.Push(action);
    }

    public void AddCreateNodeAction(string name, Vector3 position)
    {
        AddUndoAction(new CreateNodeAction
        {
            Name = name,
            Position = position
        });
    }

    public void AddCreateConnectionAction(string name)
    {
        AddUndoAction(new CreateConnectionAction
        {
            Name = name
        });
    }

    public void AddMoveNodeAction(string name, Vector3 from, Vector3 to)
    {
        AddUndoAction(new MoveNodeAction
        {
            Name = name,
            From = from,
            To = to
        });
    }

    public void AddSelectNodeAction(string name, bool selected)
    {
        AddUndoAction(new SelectNodeAction
        {
            Name = name,
            Selected = selected
        });
    }

    public void Undo()
    {
        var group = undo_actions.TryPop(out var result) ? result : null;
        if (group == null) return;

        redo_actions.Push(group);
        group.Undo();
    }

    public void Redo()
    {
        var group = redo_actions.TryPop(out var result) ? result : null;
        if (group == null) return;

        undo_actions.Push(group);
        group.Redo();
    }
}
