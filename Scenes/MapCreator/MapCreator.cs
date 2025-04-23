using System;
using FunkEngine.Classes.BeatDetector;
using Godot;

public partial class MapCreator : Node
{
    [Export]
    public Button AudioSelectButton;

    [Export]
    public Label SelectedSongLabel;

    [Export]
    public RichTextLabel DetectedOnsetsLabel;

    private FileDialog _fileDialog;

    private AudioFileAnalyzer _audioFileAnalyzer;

    public override void _Ready()
    {
        _audioFileAnalyzer = new AudioFileAnalyzer();

        _fileDialog = new FileDialog();
        _fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        _fileDialog.Access = FileDialog.AccessEnum.Filesystem;
        _fileDialog.UseNativeDialog = true;
        _fileDialog.Filters = ["*.wav", "*.mp3"];
        AddChild(_fileDialog);

        _fileDialog.FileSelected += (filePath) =>
        {
            SelectedSongLabel.Text = "Selected Song: " + filePath;
            ProcessSongFile(filePath);
        };

        AudioSelectButton.Pressed += () => _fileDialog.PopupCentered();
    }

    private void ProcessSongFile(string filePath)
    {
        _audioFileAnalyzer.LoadAudioFromFile(filePath);
        if (_audioFileAnalyzer.PCMStream != null)
        {
            _audioFileAnalyzer.DetectOnsets();
            var onsets = _audioFileAnalyzer.OnsetsFound;
            DetectedOnsetsLabel.Text = "Detected Onsets: " + string.Join(", ", onsets);
        }
        else
        {
            DetectedOnsetsLabel.Text = "Failed to load audio file.";
        }
    }
}
