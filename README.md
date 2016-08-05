# unity-scripts v.1
some script to make stuff easier.

#### Console.cs
* Features
* How to use.
* Examples.
* Advanced usage.
 * History & auto complete.
 * Binding commands to keys.
 * Returning a reference to an object.
* Render Debug
* TODO


# Console.cs
### Features
* Toggleable console for play mode.
* Console history for quickly entering same commands over again.
* Auto complete for quicker input.
* Bind commands to 0-9 numerical keys.
* Console history and key bindings save to file, so you can have them next play test.
* Very easy to implement.


### How to use.
* Add Console.cs to any GameOject.
* Inside any component create a method that start with Console_ (i.e. Console_ResetHealth(int newHealth = 100))
* In play mode hit ~ to toggle Console.
* Type: "ResetHealth" to call Console_ResetHealth on all objects, with the default parameter. (i.e. 100)
* Type: "Player ResetHealth" to only call it on the object named Player.
* Type: "Player ResetHealth 10" and it will set 10 as the parameter.
It's best if every method has default parameters set, so you don't get any problems.


### Examples
```c#
// Methods can take any number of parameters.
public void Console_GiveItem(string item = "Sword", int quantity = 1) { }
// Parameters can be int, float, bool, string, int[], float[], bool[], or string[]
public void Console_SpawnEnemies(string[] enemies = null) { }
public void Console_SpawnEnemiesWithItem(string withItem = "Sword", int quantity = 1, string[] enemies = null) { }
public void Console_SpawnEnemiesWithItems(string[] items = null, int quantity = 1, string[] enemies = null) { }
// Automatic type casting, so you don't have to deal with strings.
public void Console_SetSettings(bool[] settings) {}
// They can even be references to objects. Scroll down to Advanced Usage.
public void Console_LogHealth(Console console = null) { console.Log(myHealth); }
```
Then in console...
```c#
GiveItem
//>> Console_GiveItem(Sword, 1)

// Arrays are automatically created.
SpawnEnemies Orc Orc Orc
//>> Console_SpawnEnemies([Orc, Orc, Orc])

// But explicitly surrounding them with quotations and using commas will allow spaces in their names.
SpawnEnemies "Orc,Orc,Orc Leader"
//>> Console_SpawnEnemies([Orc, Orc, Orc Leader])

// If you don't explicitly type an array, make sure it is the last parameter.
SpawnEnemiesWithItem Sword 1 Orc Orc Orc
//>> Console_SpawnEnemiesWithItem(Sword, 1, [Orc, Orc, Orc])

// The last array doesn't need quotes if you don't feel like it.
// Commas in arrays are only necessary if you want spaces between strings.
SpawnEnemiesWithItems "Sword Bow Axe" "1 2 3" Orc Orc Orc
//>> Console_SpawnEnemiesWithItems([Sword, Bow, Axe], [1, 2, 3], [Orc, Orc, Orc])

// Booleans converted from strings or ints.
SetSettings t false 1 0 TRUE False
//>> Console_SetSettings([true, false, true, false, true, false])
```


### Advanced Usage
#### History & auto complete.
Up arrow = moves through history.

Down arrow = moves through auto complete. (when available.)

Enter = select auto complete.

Left/Right = back to input field.

#### Binding commands to keys.
You can bind a command to a number key for faster play testing.
```C#
// bind number command
bind 0 Player SetHealth 0
bind 1 Player SetHealth 100
bind 2 SetHealth 10
bind 3 SpawnEnemies Orc Orc Orc
bind 4 SpawnEnemies "Orc Master,Orc,Orc,Orc Slave"
```

#### Returning a reference to an object.
If you want methods to recieve a reference to an object:
```c#
public void Console_TeleportPlayerTo(string target, Player player) {}

// Set MethodInfo.Objects[ObjectType] to an object
MethodInfo.Objects[typeof(Player)] = myGame.player;

// Now if you entered
TeleportPlayerTo Volcanoe
// it will call Console_TeleportPlayerTo(Volcanoe, myGame.player);
```
It automatically has a reference to the console if you want to use it. Though there isn't much point atm.
#### Getting ALL public methods.
In Console.cs you can change
```c#
const string consoleMethodHeader = "Console_";  
// to
const string consoleMethodHeader = "";
```
And it will show ALL public methods in ALL components in ALL objects.
It's not designed for this, so use with caution.

### Render Debug
On any component you want to show debug info for add the method.
```c#
public void RenderDebug() {}
```
Then tap D in game to toggle renderDebug. It will be updated every tick. This isn't fully implemented though.

### TODO
* set parameters by their name
* call methods on objects by type
* calling multiple methods in a single command
