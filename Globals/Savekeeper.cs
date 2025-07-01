using System;
using System.Collections.Generic;
using Godot;
using FileAccess = Godot.FileAccess;

/// <summary>
/// v1 of a drag and drop save system.<br></br>
/// Does not work with Godot resources for safer handling.<br></br>
/// Assumptions:<br></br>
/// T needs ToString() and T.TryParse(string value, out T result) -> bool<br></br>
/// Objects need to implement their own Serialize and Deserialize<br></br>
/// </summary>
public partial class Savekeeper : Node
{
    private const string SaveFileDirectory = "user://saves";
    private const string SaveFileExtension = ".save";
    private const string DefaultSaveFileName = "MidnightRiff";

    private const char Delimiter = '|';

    public delegate void SavingHandler();

    /// <summary>
    /// Event that a save is underway. Subscribe to serialize data before writing.
    /// </summary>
    public static event SavingHandler Saving;

    public delegate void GameSavedHandler();
    public static event GameSavedHandler GameSaved;

    public delegate void LoadedHandler();
    public static event LoadedHandler Loaded;

    public override void _EnterTree()
    {
        DirAccess.MakeDirAbsolute(SaveFileDirectory);
    }

    public static Dictionary<String, String> GameSaveObjects { get; private set; } =
        new Dictionary<String, String>();

    private static void HandlePrevSave()
    {
        return; //Placeholder to do things, e.g. make a backup
    }

    public static void RecordSave(string savePath = DefaultSaveFileName + SaveFileExtension)
    {
        Saving?.Invoke();
        Callable.From(() => SaveToFile(savePath)).CallDeferred();
    }

    public static bool SaveToFile(string savePath = DefaultSaveFileName + SaveFileExtension)
    {
        if (string.IsNullOrEmpty(savePath))
            return false;
        FileAccess file = FileAccess.Open(
            SaveFileDirectory + "/" + savePath,
            FileAccess.ModeFlags.Write
        );
        if (file == null)
            return false;

        foreach ((string key, string value) in GameSaveObjects)
            file.StoreLine(key + Delimiter + value);
        file.Close();

        GameSaved?.Invoke();
        return true;
    }

    public static bool LoadFromFile(string savePath = DefaultSaveFileName + SaveFileExtension)
    {
        if (string.IsNullOrEmpty(savePath))
            return false;
        FileAccess file = FileAccess.Open(
            SaveFileDirectory + "/" + savePath,
            FileAccess.ModeFlags.Read
        );
        if (file == null)
            return false;

        HandlePrevSave();

        while (file.GetPosition() < file.GetLength())
        {
            string line = file.GetLine().Trim();
            int idx = line.IndexOf(Delimiter);
            if (idx == -1)
                continue;

            string key = line.Substring(0, idx);
            string value = SanitizeSaveString(line.Substring(idx));
            if (value == null)
                continue;

            GameSaveObjects[key] = value;
        }

        Loaded?.Invoke();
        return true;
    }

    private static string SanitizeSaveString(string saveString)
    {
        if (string.IsNullOrEmpty(saveString))
            return null;
        saveString = saveString.Trim();
        if (saveString.Length == 0 || saveString[0] != Delimiter)
            return null;
        return saveString.Substring(1);
    }

    const string InvalidFormatString = "InvalidString";

    //Ex: Position.X|45.8|
    public static string Format(string valName, object value)
    {
        if (value == null)
            return "";
        if (value is string s && !s.IsValidFileName())
            return valName + Delimiter + InvalidFormatString + Delimiter;

        return valName + Delimiter + value + Delimiter;
    }

    private const string ArrayDelimiter = "*";

    //Ex: ValidIds|[12*1*4*5]|
    public static string FormatArray<T>(string valName, T[] value)
    {
        if (value == null)
            return "";
        String retString = valName + Delimiter + '[';

        foreach (object o in value)
        {
            if (o is string s && !s.IsValidFileName())
            {
                retString += InvalidFormatString + ArrayDelimiter;
                continue;
            }
            retString += o.ToString() + ArrayDelimiter;
        }
        retString += "]" + Delimiter;
        return retString;
    }

    public readonly struct ParseResult<T>(
        T value,
        bool success,
        int nextIdx,
        string message = "Success!"
    )
    {
        public T Value { get; init; } = value;
        public bool Success { get; init; } = success;
        public int NextIdx { get; init; } = nextIdx;
        public string Message { get; init; } = message;
    }

    public delegate bool TryParseHandler<T>(string value, out T result); //https://stackoverflow.com/questions/2961656/generic-tryparse

    public static bool StringParse(string value, out string result)
    {
        result = value;
        return true;
    }

    public static ParseResult<T> Parse<T>(
        string valName,
        string saveString,
        int startIdx,
        TryParseHandler<T> handler
    )
    {
        if (string.IsNullOrEmpty(valName) || string.IsNullOrEmpty(saveString))
            return new ParseResult<T>(
                default,
                false,
                startIdx,
                $"String was empty! {valName} {saveString}"
            );

        ParseResult<string> success = ParseToSubStringValue(saveString, valName, startIdx);
        if (!success.Success)
            return new ParseResult<T>(default, false, startIdx, success.Message);
        string value = success.Value;
        int finalIdx = success.NextIdx;

        if (handler(value, out var result))
            return new ParseResult<T>(result, true, finalIdx);

        return new ParseResult<T>(
            default,
            false,
            startIdx,
            $"Unable to parse from: \"{value}\" to type: {typeof(T)}"
        );
    }

    public static ParseResult<T[]> ParseArray<T>(
        string valName,
        string saveString,
        int startIdx,
        TryParseHandler<T> handler
    )
    {
        if (string.IsNullOrEmpty(valName) || string.IsNullOrEmpty(saveString))
            return new ParseResult<T[]>(
                default,
                false,
                startIdx,
                $"String was empty! {valName} {saveString}"
            );

        ParseResult<string> success = ParseToSubStringValue(saveString, valName, startIdx);
        if (!success.Success)
            return new ParseResult<T[]>(default, false, startIdx, success.Message);
        string value = success.Value;
        int finalIdx = success.NextIdx;

        string[] values = value.Replace("[", "").Replace("]", "").Split(ArrayDelimiter);
        List<T> list = new List<T>();
        foreach (String s in values)
        {
            if (string.IsNullOrEmpty(s))
                continue;
            if (!handler(s, out var result))
                return new ParseResult<T[]>(
                    list.ToArray(),
                    false,
                    finalIdx,
                    $"Unable to parse from: \"{s}\" to type: {typeof(T)}. Returning any successful values."
                );

            list.Add(result);
        }

        return new ParseResult<T[]>(list.ToArray(), true, finalIdx);
    }

    private static ParseResult<string> ParseToSubStringValue(
        string saveString,
        string valName,
        int startIdx
    )
    {
        int nextIdx = saveString.IndexOf(valName, startIdx, StringComparison.Ordinal);
        if (nextIdx == -1)
            return new ParseResult<string>(default, false, startIdx, $"Name not found! {valName}");

        nextIdx += valName.Length + 1;
        int finalIdx = saveString.IndexOf(Delimiter, nextIdx);
        if (finalIdx == -1)
            return new ParseResult<string>(
                default,
                false,
                startIdx,
                $"No final delimiter found around value! \n String received: {saveString}, from position {nextIdx}"
            );
        string value = saveString.Substring(nextIdx, finalIdx - nextIdx);

        return new ParseResult<string>(value, true, finalIdx);
    }

    #region Project Specific

    public const string DefaultRunSaveHeader = "CurrentGame";

    public static void ClearRun()
    {
        if (GameSaveObjects.ContainsKey(DefaultRunSaveHeader))
        {
            GameSaveObjects.Remove(DefaultRunSaveHeader);
            SaveToFile();
        }
    }

    #endregion
}
