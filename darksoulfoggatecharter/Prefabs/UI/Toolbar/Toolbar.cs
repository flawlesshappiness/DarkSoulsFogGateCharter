using FlawLizArt.Log;
using Godot;
using System;
using System.Text.Json;

public partial class Toolbar : MarginContainer
{
    [Export]
    public Button NewButton;

    [Export]
    public Button OpenButton;

    [Export]
    public Button SaveButton;

    [Export]
    public Button NodesButton;

    [Export]
    public Button UndoButton;

    [Export]
    public Button RedoButton;

    [Export]
    public Button ImageMapButton;

    [Export]
    public Button NodeMapButton;

    [Export]
    public FileDialog OpenFileDialog;

    [Export]
    public FileDialog SaveFileDialog;

    [Export]
    public Label UnsavedChangesLabel;

    private string selected_save_path;
    private const string WindowTitle = "Dark Souls Fog Gate Charter";

    public override void _Ready()
    {
        base._Ready();
        NewButton.Pressed += New_Pressed;
        OpenButton.Pressed += Open_Pressed;
        SaveButton.Pressed += Save_Pressed;
        NodesButton.Pressed += Nodes_Pressed;
        UndoButton.Pressed += Undo_Pressed;
        RedoButton.Pressed += Redo_Pressed;
        ImageMapButton.Pressed += ImageMap_Pressed;
        NodeMapButton.Pressed += NodeMap_Pressed;

        OpenFileDialog.FileSelected += OpenFile_Selected;
        SaveFileDialog.FileSelected += SafeFile_Selected;

        NodeController.Instance.OnNodeChanges += Node_Changes;
        NodeController.Instance.OnClear += Node_Clear;
        InputController.Instance.OnShortcutQuicksave += QuickSave;

        SetUnsavedChanges(false);
    }

    private void New_Pressed()
    {
        MainView.Instance.OpenStartSettings();
    }

    private void Open_Pressed()
    {
        OpenFileDialog.Popup();
    }

    private void Save_Pressed()
    {
        SaveFileDialog.Popup();
    }

    private void Nodes_Pressed()
    {
        MainView.Instance.OpenGateList();
    }

    private void Undo_Pressed()
    {
        try
        {
            UndoController.Instance.Undo();
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            Log.Stacktrace(e.StackTrace);
        }
    }

    private void Redo_Pressed()
    {
        try
        {
            UndoController.Instance.Redo();
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            Log.Stacktrace(e.StackTrace);
        }
    }

    private void ImageMap_Pressed()
    {
        UndoController.Instance.StartUndoAction();

        try
        {
            ImageMapButton.Hide();
            NodeMapButton.Show();
            ImageMapController.Instance.SetImageMapModeEnabled(true);
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            Log.Stacktrace(e.StackTrace);
        }

        UndoController.Instance.EndUndoAction();
    }

    private void NodeMap_Pressed()
    {
        UndoController.Instance.StartUndoAction();

        try
        {
            ImageMapButton.Show();
            NodeMapButton.Hide();
            ImageMapController.Instance.SetImageMapModeEnabled(false);
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            Log.Stacktrace(e.StackTrace);
        }

        UndoController.Instance.EndUndoAction();
    }

    private void OpenFile_Selected(string path)
    {
        if (!FileAccess.FileExists(path)) return;

        var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var json = file.GetAsText();

        try
        {
            var data = JsonSerializer.Deserialize<SessionData>(json);
            Load(data);

            selected_save_path = path;
            SetUnsavedChanges(false);
        }
        catch (Exception e)
        {

        }
    }

    private void Load(SessionData data)
    {
        try
        {
            NodeController.Instance.Load(data);
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            Log.Stacktrace(e.StackTrace);
        }
    }

    private void SafeFile_Selected(string path)
    {
        try
        {
            Save(path);
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
            Log.Stacktrace(e.StackTrace);
        }
    }

    private void Save(string path)
    {
        var extension = path.GetExtension();
        if (string.IsNullOrEmpty(extension))
        {
            path += ".data";
        }

        var data = NodeController.Instance.GenerateSaveData();
        var json = JsonSerializer.Serialize(data);
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(json);
            file.Close();

            selected_save_path = path;
        }

        SetUnsavedChanges(false);
    }

    private void QuickSave()
    {
        if (string.IsNullOrEmpty(selected_save_path))
        {
            Save_Pressed();
        }
        else
        {
            SafeFile_Selected(selected_save_path);
        }
    }

    private void Node_Changes()
    {
        SetUnsavedChanges(true);
    }

    private void Node_Clear()
    {
        selected_save_path = null;
    }

    private void SetUnsavedChanges(bool has_unsaved_changes)
    {
        var title = has_unsaved_changes ? $"{WindowTitle} *" : WindowTitle;
        UnsavedChangesLabel.Visible = has_unsaved_changes;
        DisplayServer.WindowSetTitle(title);
    }
}
