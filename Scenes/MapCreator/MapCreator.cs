using System;
using Godot;

public partial class MapCreator : Node
{
    [Export]
    public Button AudioSelectButton;

    [Export]
    public Label SelectedSongLabel;

    [Export]
    public RichTextLabel DetectedOnsetsLabel;

    private string _selectedSongPath;
    private FileDialog _fileDialog;

    public override void _Ready()
    {
        _fileDialog = new FileDialog();
        _fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        _fileDialog.Access = FileDialog.AccessEnum.Filesystem;
        _fileDialog.UseNativeDialog = true;
        _fileDialog.Filters = ["*.wav", "*.mp3"];
        AddChild(_fileDialog);

        _fileDialog.FileSelected += (filePath) =>
        {
            _selectedSongPath = filePath;
            SelectedSongLabel.Text = "Selected Song: " + filePath;
        };

        AudioSelectButton.Pressed += () => _fileDialog.PopupCentered();
    }
}
