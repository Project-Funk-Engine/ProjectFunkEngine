using System;
using System.Collections.Generic;
using Godot;
using FileAccess = Godot.FileAccess;

/// <summary>
/// v1 of a drag and drop save system.
/// Does not work with Godot resources for safer handling.
/// </summary>
public partial class Savekeeper : Node
{
    public const string SaveFileDirectory = "user://saved_games";
    public const string SaveFileExtension = ".save";
    public const string DefaultSaveFileName = "game";

    public const char Delimiter = '|';

    public delegate void SavingHandler();
    public static event SavingHandler Saving;

    public delegate void GameSavedHandler();
    public static event GameSavedHandler GameSaved;

    public delegate void LoadedHandler();
    public static event LoadedHandler Loaded;

    public override void _EnterTree()
    {
        DirAccess.MakeDirAbsolute(SaveFileExtension);
    }

    public static Dictionary<String, String> GameSaveObjects = new Dictionary<String, String>();

    public static void RecordSave()
    { //TODO: Refine later
        //Call signal, assume nodes will access dictionary. This means in ready will need to listen to signal.
        //Maybe node groups?
        Saving?.Invoke();
        Callable.From(() => SaveToFile()).CallDeferred();
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
        {
            file.StoreLine(key + Delimiter + value);
        }
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

        GameSaveObjects.Clear();

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

    public static string SanitizeSaveString(string saveString)
    {
        if (string.IsNullOrEmpty(saveString))
            return null;
        saveString = saveString.Trim();
        if (saveString.Length == 0)
            return null;

        if (saveString[0] != Delimiter)
            return null;
        return saveString.Substring(1);
    }

    //TODO: Learn more about C# types.
    //Ex: Position.X|45.8|
    public static string Format(string valName, object value)
    {
        if (value == null)
            return "";
        return valName + Delimiter + value + Delimiter;
    }

    public const string ArrayDelimiter = "~";

    //Ex: ValidIds|[12~1~4~5]|
    public static string FormatArray<T>(string valName, T[] value)
    {
        if (value == null)
            return "";
        String retString = valName + Delimiter + '[';
        foreach (object o in value)
        {
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

        value = value.Replace("[", "");
        value = value.Replace("]", "");
        string[] values = value.Split(ArrayDelimiter);

        List<T> list = new List<T>();
        foreach (String s in values)
        {
            if (string.IsNullOrEmpty(s))
                continue;
            if (handler(s, out var result))
                list.Add(result);
            else
                return new ParseResult<T[]>(
                    list.ToArray(),
                    false,
                    finalIdx,
                    $"Unable to parse from: \"{s}\" to type: {typeof(T)}. Returning any successful values."
                );
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
}
