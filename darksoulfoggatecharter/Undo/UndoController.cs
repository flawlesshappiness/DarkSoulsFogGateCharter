using System.Collections.Generic;

public partial class UndoController : SingletonController
{
    public static UndoController Instance => Singleton.Get<UndoController>();
    public override string Directory => "Undo";

    private Stack<UndoActionGroup> undo_actions = new();
    private UndoActionGroup current_group = null;

    private abstract class UndoAction
    {
        public virtual void Undo()
        {

        }
    }

    private class CreateNodeAction : UndoAction
    {
        public string Name { get; set; }

        public override void Undo()
        {
            base.Undo();
            MainScene.Instance.RemoveNode(Name);
        }
    }

    private class CreateConnectionAction : UndoAction
    {
        public string Name { get; set; }

        public override void Undo()
        {
            base.Undo();
            MainScene.Instance.RemoveConnectionObject(Name);
        }
    }

    private class UndoActionGroup
    {
        public Stack<UndoAction> Actions { get; set; } = new();

        public void Undo()
        {
            while (Actions.Count > 0)
            {
                Actions.Pop().Undo();
            }
        }
    }

    public void Clear()
    {
        undo_actions.Clear();
    }

    public void ConfirmUndoAction()
    {
        if (current_group == null) return;
        undo_actions.Push(current_group);
        current_group = null;
    }

    private void AddUndoAction(UndoAction action)
    {
        current_group ??= new UndoActionGroup();
        current_group.Actions.Push(action);
    }

    public void AddCreateNodeAction(string name)
    {
        AddUndoAction(new CreateNodeAction
        {
            Name = name
        });
    }

    public void AddCreateConnectionAction(string name)
    {
        AddUndoAction(new CreateConnectionAction
        {
            Name = name
        });
    }

    public void Undo()
    {
        var action = undo_actions.TryPop(out var result) ? result : null;
        if (action == null) return;

        action.Undo();
    }
}
