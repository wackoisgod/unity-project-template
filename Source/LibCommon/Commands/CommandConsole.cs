using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static System.String;

namespace LibCommon.Commands
{
  public class CommandConsole
  {
    private static readonly Dictionary<string, CommandGroup> CommandGroups
      = new Dictionary<string, CommandGroup>();

    public static readonly CommandConsole Instance = new CommandConsole();
    public Action<string> MessageLogged;

    [CommandGroup("console", "console operations")]
    private class ConsoleCommandGroup : CommandGroup
    {
      [DefaultCommand]
      [Command("help", "display help")]
      public static string ShowHelp(params string[] args)
      {
        string output = "";
        foreach (KeyValuePair<string, CommandGroup> pair in CommandGroups)
        {
          foreach (CommandAttribute command in pair.Value.CommandAttributes)
            output += $"{pair.Key} {command.Name}: {command.Help}\n";
        }
        return output;
      }

      [Command("echo", "Displays a message in the console log")]
      public static string Echo(params string[] args)
      {
        return Join(" ", args);
      }
    }

    public string CommandPrefix { get; protected set; }

    public void Init(string prefix)
    {
      CommandPrefix = prefix;
      Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
      foreach (Assembly asm in assemblies)
        RegisterAssembly(asm);
    }

    public void Shutdown()
    {
    }

    public void RegisterAssembly(Assembly asm)
    {
      foreach (Type type in asm.GetTypes())
        RegisterType(type);
    }

    public bool RegisterType(Type type)
    {
      if (!type.IsSubclassOf(typeof(CommandGroup))) return false;

      CommandGroupAttribute[] attributes =
        (CommandGroupAttribute[]) type.GetCustomAttributes(typeof(CommandGroupAttribute), true);
      if (attributes.Length == 0) return false;

      CommandGroupAttribute groupAttribute = attributes[0];
      CommandGroup commandGroup = (CommandGroup) Activator.CreateInstance(type);
      commandGroup.Register(groupAttribute);
      return RegisterCommandGroup(groupAttribute.Name, commandGroup);
    }

    public bool RegisterCommandGroup(string name, CommandGroup commandGroup)
    {
      if (CommandGroups.ContainsKey(name)) return false;
      CommandGroups.Add(name, commandGroup);
      return true;
    }

    public void Dispatch(string line)
    {
      string command;
      string parameters;
      string output = Empty;
      bool found = false;

      if (IsNullOrEmpty(line)) return;
      if (ExtractCommandAndParameters(line, out command, out parameters))
      {
        foreach (KeyValuePair<string, CommandGroup> pair in CommandGroups)
        {
          if (pair.Key != command) continue;
          output = pair.Value.Handle(parameters);
          found = true;
          break;
        }
      }

      if (!found)
      {
        Log($"Unknown command: {line}");
        Log($"type '{CommandPrefix}console help' for a list of available commands");
      }

      if (!IsNullOrEmpty(output)) Log(output);
    }

    public bool ExtractCommandAndParameters(string line, out string command, out string parameters)
    {
      line = line.Trim();
      command = Empty;
      parameters = Empty;

      if (line == Empty)
        return false;
      if (!line.StartsWith(CommandPrefix))
        return false;

      command = line.Substring(CommandPrefix.Length).Split(' ')[0].ToLower();
      parameters = Empty;
      if (line.Contains(' ')) parameters = line.Substring(line.IndexOf(' ') + 1).Trim(); // get parameters if any.
      return true;
    }

    public void Log(string message)
    {
      MessageLogged?.Invoke(message);
    }
  }
}