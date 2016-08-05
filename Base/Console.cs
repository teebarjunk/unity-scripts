using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

/*
    ~           =   Toggle console.
    D           =   Toggle debug render.
    Enter       =   Submit command.
    Up          =   Loop through command history.
    Down        =   Loop through auto correct options. (When available.)
    Left/Right  =   Get out of history/auto complete menus.
*/

public class Console : MonoBehaviour
{
    const string consoleMethodHeader = "Console_";          // Header for console methods. Set to "" to allow all public functions.
    const string renderDebugMethod = "RenderDebug";         // Method called when renderDebug == true. Don't leave this blank or it will call ALL method function every update tick! 

    const string splitOn = " ";                             // String to split commands with.
    const string groupStart = "\"";                         // Used for grouping variables together.
    const string groupEnd = "\"";                           // ^

    const string consoleLogPath = "ConsoleLog.txt";         // Path to save console log to.
    const string bindingsPath = "Bindings.txt";             // Path to save binding data to.
    const KeyCode consoleToggleKey = KeyCode.BackQuote;     // Key that toggles console.
    const KeyCode renderDebugToggleKey = KeyCode.D;         // Key that toggles debug rendering.


    [Tooltip("Call GameObject.SendMessage if no other Console method was found.")]
    public bool allowSendMessage = false;                   // Will call GameObject.SendMessage if no Console_ method was found.

    [Tooltip("Set to false if you want to set bindings from the editor.")]
    public bool loadBindingsOnPlay = true;                 // Set to false if you are setting these in editor mode.

    [Tooltip("Commands tied to numeric keys. Press 0-9 to call.")]
    public string[] bindings = new string[10];              // Commands that will be run if a digit between 0-9 is pressed.



    // Methods by object name.
    Dictionary<string, Dictionary<string, ComponentMethod>> byObject = new Dictionary<string, Dictionary<string, ComponentMethod>>();
    // Methods names available and the objects that have them.
    Dictionary<string, List<ComponentMethod>> byMethod = new Dictionary<string, List<ComponentMethod>>();

    bool consoleIsOpen = false;
    bool renderDebug = true;

    string consoleCommand = "";
    string lastConsoleCommand = "";
    List<string> consoleHistory = new List<string>();
    string consoleHistoryString = "";
    int consoleHistoryIndex = -1;
    Vector2 consoleScroll = new Vector2(0, float.MaxValue);

    List<string> autoComplete = new List<string>();
    string autoCompleteString = "";
    int autoCompleteIndex = -1;

    KeyCode lastKeyCode;
    Dictionary<KeyCode, EventType> lastEvent = new Dictionary<KeyCode, EventType>();

    void Start()
    {
        // Allows methods to ask for a reference to this console.
        MethodHelper.Objects[typeof(Console)] = this;

        Load();
        UpdateMethodList();
        UpdateAutoComplete();
    }

    // Shows console, and loads objects/methods to memory.
    void Open()
    {
        consoleIsOpen = true;
        UpdateMethodList();
        UpdateAutoComplete();
    }

    // Clean memory.
    void Close()
    {
        consoleIsOpen = false;
        autoComplete.Clear();
    }

    void Update()
    {
        // Debug render.
        if (Input.GetKeyUp(renderDebugToggleKey))
            renderDebug = !renderDebug;

        // Render debug data.
        if (renderDebug)
            foreach (GameObject go in FindObjectsOfType<GameObject>())
                go.SendMessage(renderDebugMethod, SendMessageOptions.DontRequireReceiver);

        // Toggle console.
        if (!consoleIsOpen && Input.GetKeyDown(consoleToggleKey))
        {
            Open();
        }
        else
        {
            // Pressing a number from 0-9 will automatically enter a given command.
            // But only if the console is closed.
            for (int i = 0; i < 10; i++)
                if (Input.GetKeyDown((KeyCode)48 + i))
                    Run_Command(bindings[i]);
        }
    }

    // Check for key press.
    KeyCode GetPressed()
    {
        KeyCode keyCode = Event.current.keyCode;
        EventType eventType = Event.current.type;
        bool keyPressed = (eventType == EventType.KeyDown && (!lastEvent.ContainsKey(keyCode) || lastEvent[keyCode] != EventType.KeyDown));
        lastEvent[keyCode] = eventType;

        return keyPressed ? keyCode : KeyCode.None;
    }

    void OnGUI()
    {
        if (!consoleIsOpen)
            return;

        GUI.FocusControl("Console");

        bool moveToEndOfLine = false;

        KeyCode key = GetPressed();

        // Toggle.
        if (key == consoleToggleKey)
        {
            Close();
            return;
        }
        else if (key == KeyCode.Return)
        {
            // Set selected auto complete to console command.
            if (autoCompleteIndex != -1)
            {
                consoleCommand += autoComplete[autoCompleteIndex] + splitOn;
                autoCompleteIndex = -1;
                moveToEndOfLine = true;
            }
            // Run console command.
            else if (consoleCommand.Trim() != "")
            {
                Run_Command(consoleCommand);
                consoleCommand = "";
            }
        }
        else if (key == KeyCode.LeftArrow || key == KeyCode.RightArrow)
        {
            autoCompleteIndex = -1;
            consoleHistoryIndex = -1;
        }
        // Console history.
        else if (key == KeyCode.UpArrow)
        {
            if (autoCompleteIndex != -1)
                autoCompleteIndex = -1;
            else if (consoleHistory.Count > 0)
            {
                consoleHistoryIndex = (consoleHistoryIndex + 1) % consoleHistory.Count;
                consoleCommand = consoleHistory[consoleHistory.Count - 1 - consoleHistoryIndex];
            }

            moveToEndOfLine = true;
        }
        // Auto complete history.
        else if (key == KeyCode.DownArrow && autoComplete.Count > 0)
        {
            if (consoleHistoryIndex != -1)
                consoleHistoryIndex = -1;
            else
                autoCompleteIndex = (autoCompleteIndex + 1) % autoComplete.Count;
        }

        // Show history.
        GUI.Box(new Rect(0, 0, 800, 100), "");
        consoleScroll = GUILayout.BeginScrollView(consoleScroll, GUILayout.Width(800), GUILayout.Height(100));
        GUILayout.Label(consoleHistoryString);
        GUILayout.EndScrollView();

        // Show input field.
        GUI.SetNextControlName("Console");
        consoleCommand = GUI.TextField(new Rect(0, 100, 800, 20), consoleCommand);

        // Show Auto complete.
        if (autoComplete.Count > 0)
        {
            GUIStyle s = new GUIStyle();
            s.border.Remove(new Rect());
            float h = GUI.skin.label.lineHeight;
            Rect r = new Rect(0, 120, 200, h * autoComplete.Count);

            // Backing.
            GUI.Box(r, "");

            // Selection.
            if (autoCompleteIndex != -1)
                GUI.Box(new Rect(0, 120 + autoCompleteIndex * h, 200, h), "");

            // List.
            GUIStyle style = new GUIStyle();
            style.richText = true;
            GUI.Label(r, autoCompleteString, style);
        }

        // 
        if (lastConsoleCommand != consoleCommand)
        {
            lastConsoleCommand = consoleCommand;

            // Update auto complete.
            UpdateAutoComplete();

            // Move cursor to end of line.
            if (moveToEndOfLine)
            {
                TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                editor.MoveCursorToPosition(Vector2.one * float.MaxValue);
            }
        }
    }

    void UpdateAutoComplete()
    {
        autoComplete.Clear();
        autoCompleteString = "";

        string[] parts = consoleCommand.Split(new string[] { splitOn }, StringSplitOptions.None);
        string last = parts[parts.Length - 1];
        
        if (parts.Length > 2)
        {
            autoComplete.Add("Ass");
            autoCompleteString += "Ass\n";
            return;
        }

        if (parts.Length > 1)
        {
            string objectName = parts[parts.Length - 2];

            // First parameter was object, so list it's methods.
            if (byObject.ContainsKey(objectName))
            {
                foreach (string go in byObject[objectName].Keys)
                    if (go.Contains(last))
                    {
                        autoComplete.Add(go);
                        autoCompleteString += go + "\n";
                    }

                return;
            }
        }

        // List all objects.
        foreach (string go in byObject.Keys)
            if (go.Contains(last))
            {
                autoComplete.Add(go);
                autoCompleteString += go + "\n";
            }

        // List all methods.
        foreach (string m in byMethod.Keys)
            if (m.Contains(last))
            {
                autoComplete.Add(m);
                autoCompleteString += m + "\n";
            }
    }

    public void Console_Ass(string str, bool b, int[] intarray, Console meep = null)
    {
        meep.Log(str + " " + b.ToString() + " " + StringHelper.ArrayToString(intarray));
    }

    void Run_Command(string command)
    {
        // Clean whitespace.
        command = command.Trim();

        // Ignore if blank.
        if (command == "")
            return;

        // Add to log.
        consoleHistory.Add(command);
        consoleHistoryIndex = consoleHistory.Count;
        consoleHistoryString += command + "\n";
        // Scroll to bottom.
        consoleScroll.y = float.MaxValue;

        // Save log.
        SaveConsoleHistory();

        // Run.
        string[] parts = MethodHelper.CommandSplit(command, splitOn, groupStart, groupEnd);
        string first = parts[0];

        // Bind a command to a keyboard.
        if (first == "bind")
        {
            // Set binding,
            bindings[int.Parse(parts[1])] = StringHelper.ArrayToString(parts, splitOn, 2);
            // Save bindings.
            SaveBindings();
            return;
        }

        // On single object that has method.
        if (parts.Length > 1 && byObject.ContainsKey(first))
        {
            string method = parts[1];

            // Check that method exists.
            if (!byObject[first].ContainsKey(method))
            {
                Log("No method " + method + " in object " + first);
                return;
            }

            byObject[first][method].Invoke(ArrayHelper.SubArray(parts, 2));
        }
        // On all objects with method.
        else if (byMethod.ContainsKey(first))
        {
            string[] data = ArrayHelper.SubArray(parts, 1);
            foreach (ComponentMethod cm in byMethod[first])
                cm.Invoke(data);
        }
        // Call the command on all game objects.
        else if (allowSendMessage)
        {
            command = StringHelper.ArrayToString(parts, splitOn, 1);

            // Otherwise try the built in SendMessage on all objects.
            foreach (GameObject go in FindObjectsOfType<GameObject>())
                go.SendMessage(parts[0], command, SendMessageOptions.DontRequireReceiver);
        }
        else
            Log("Command " + first + " not recognized");
    }

    public void Log(string data, string color = "white")
    {
        Debug.Log(data);
    }

    void UpdateMethodList()
    {
        byObject.Clear();
        byMethod.Clear();

        foreach (GameObject go in FindObjectsOfType(typeof(GameObject)) as GameObject[])
            foreach (Component c in go.GetComponents<Component>())
                foreach (MethodInfo m in c.GetType().GetMethods())
                    if (m.Name.StartsWith("Console_"))
                    {
                        //methodInfo.Invoke(component, AsParameters(data));
                        ComponentMethod cm = new ComponentMethod(c, m);
                        string mod_name = m.Name.Replace("Console_", "");

                        // Add method to objects list of methods.
                        if (!byObject.ContainsKey(go.name))
                            byObject.Add(go.name, new Dictionary<string, ComponentMethod>());

                        if (!byObject[go.name].ContainsKey(mod_name))
                            byObject[go.name].Add(mod_name, cm);

                        // Add object to methods list of objects.
                        if (!byMethod.ContainsKey(mod_name))
                            byMethod.Add(mod_name, new List<ComponentMethod>());

                        byMethod[mod_name].Add(cm);

                        break;
                    }
    }

    void Save()
    {
        SaveConsoleHistory();
        SaveBindings();
    }

    void SaveConsoleHistory()
    {
        // Limit logging to last 100 commands.
        int start = Math.Max(0, consoleHistory.Count - 101);

        StreamWriter file = File.CreateText(consoleLogPath);
        for (int i = start; i < consoleHistory.Count; i++)
            file.WriteLine(consoleHistory[i]);
        file.Close();
    }

    void SaveBindings()
    {
        StreamWriter file = File.CreateText(bindingsPath);
        for (int i = 0; i < 10; i++)
            file.WriteLine(bindings[i]);
        file.Close();
    }

    void Load()
    {
        // Load bindings.
        if (loadBindingsOnPlay)
        {
            int i = 0;
            foreach (string line in FileHelper.LoadFile(bindingsPath))
                bindings[i++] = line;
        }
        
        // Load history.
        foreach (string line in FileHelper.LoadFile(consoleLogPath))
            consoleHistory.Add(line);

        // Move history to end.
        consoleHistoryIndex = consoleHistory.Count;
    }
}
