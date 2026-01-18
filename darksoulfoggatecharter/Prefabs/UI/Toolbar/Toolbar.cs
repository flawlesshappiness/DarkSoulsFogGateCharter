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
    public FileDialog OpenFileDialog;

    [Export]
    public FileDialog SaveFileDialog;

    public override void _Ready()
    {
        base._Ready();
        NewButton.Pressed += New_Pressed;
        OpenButton.Pressed += Open_Pressed;
        SaveButton.Pressed += Save_Pressed;

        OpenFileDialog.FileSelected += OpenFile_Selected;
        SaveFileDialog.FileSelected += SafeFile_Selected;
    }

    private void New_Pressed()
    {
        MainScene.Instance.Clear();
        MainView.Instance.OpenStartSearch();
    }

    private void Open_Pressed()
    {
        OpenFileDialog.Popup();
    }

    private void Save_Pressed()
    {
        SaveFileDialog.Popup();
    }

    private void OpenFile_Selected(string path)
    {
        if (!FileAccess.FileExists(path)) return;

        var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var json = file.GetAsText();

        try
        {
            var data = JsonSerializer.Deserialize<SaveData>(json);
            MainScene.Instance.Load(data);
        }
        catch (Exception e)
        {

        }
    }

    private void SafeFile_Selected(string path)
    {
        var extension = path.GetExtension();
        if (string.IsNullOrEmpty(extension))
        {
            path += ".data";
        }

        var data = MainScene.Instance.GenerateSaveData();
        var json = JsonSerializer.Serialize(data);
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(json);
            file.Close();
        }
    }
}
