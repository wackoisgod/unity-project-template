using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LibCommon.Commands
{
  public static class CommandManager
  {
    private static readonly Dictionary<CommandGroupAttribute, CommandGroup> CommandGroups = new Dictionary<CommandGroupAttribute, CommandGroup>();

    static CommandManager()
    {
      RegisterCommandGroups();
    }

    private static void RegisterCommandGroups()
    {
      Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
      foreach (Assembly asm in assemblies)
      {
        foreach (Type type in asm.GetTypes())
        {
          if (!type.IsSubclassOf(typeof(CommandGroup))) continue;

          CommandGroupAttribute[] attributes = (CommandGroupAttribute[])type.GetCustomAttributes(typeof(CommandGroupAttribute), true);
          if (attributes.Length == 0) continue;

          CommandGroupAttribute groupAttribute = attributes[0];

          CommandGroup group = (CommandGroup)Activator.CreateInstance(type);
          group.Register(groupAttribute);
          CommandGroups.Add(groupAttribute, group);
        }
      }
    }

    public static string Parse(string line)
    {
      string output = string.Empty;
      string command;
      string parameters;
      bool found = false;

      if (line == null) return "No valid command";
      if (line.Trim() == string.Empty) return "No valid command";

      if (!ExtractCommandAndParameters(line, out command, out parameters))
      {
        return "Failed to parse Command";
      }

      foreach (KeyValuePair<CommandGroupAttribute, CommandGroup> pair in CommandGroups)
      {
        if (pair.Key.Name != command) continue;
        output = pair.Value.Handle(parameters);
        found = true;
        break;
      }

      if (found == false)
        output = $"Unknown command: {command} {parameters}";

      if (output != string.Empty)
        Console.WriteLine(output);

      return output;
    }

    public static bool ExtractCommandAndParameters(string line, out string command, out string parameters)
    {
      line = line.Trim();
      command = string.Empty;
      parameters = string.Empty;

      if (line == string.Empty)
        return false;

      if (line[0] != '/')
        return false;

      line = line.Substring(1);
      command = line.Split(' ')[0].ToLower();
      parameters = string.Empty;
      if (line.Contains(' ')) parameters = line.Substring(line.IndexOf(' ') + 1).Trim(); // get parameters if any.

      return true;
    }
  }
}
