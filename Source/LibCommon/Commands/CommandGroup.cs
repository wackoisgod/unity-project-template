using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LibCommon.Commands
{
  public abstract class CommandGroup
  {
    public CommandGroupAttribute Attributes { get; private set; }

    private readonly Dictionary<CommandAttribute, MethodInfo> _commands = new Dictionary<CommandAttribute, MethodInfo>();

    public IEnumerable<CommandAttribute> CommandAttributes => _commands.Keys.AsEnumerable();

    public void Register(CommandGroupAttribute attributes)
    {
      Attributes = attributes;
      RegisterDefaultCommand();
      RegisterCommands();
    }

    protected void RegisterCommands()
    {
      foreach (MethodInfo method in GetType().GetMethods())
      {
        object[] attributes = method.GetCustomAttributes(typeof(CommandAttribute), true);
        if (attributes.Length == 0) continue;

        CommandAttribute attribute = (CommandAttribute)attributes[0];
        if (attribute is DefaultCommand) continue;

        if (!_commands.ContainsKey(attribute))
          _commands.Add(attribute, method);
      }
    }

    private void RegisterDefaultCommand()
    {
      foreach (MethodInfo method in GetType().GetMethods())
      {
        object[] attributes = method.GetCustomAttributes(typeof(DefaultCommand), true);
        if (attributes.Length == 0) continue;
        if (method.Name.ToLower() == "fallback") continue;

        _commands.Add(new DefaultCommand(), method);
        return;
      }

      _commands.Add(new DefaultCommand(), GetType().GetMethod("Fallback"));
    }

    public virtual string Handle(string parameters)
    {
      string[] @params = null;
      CommandAttribute target;

      if (parameters == string.Empty)
        target = GetDefaultSubcommand();
      else
      {
        @params = parameters.Split(' ');
        target = GetSubcommand(@params[0]) ?? GetDefaultSubcommand();

        if (!Equals(target, GetDefaultSubcommand()))
          @params = @params.Skip(1).ToArray();
      }

      MethodInfo methodInfo;
      if (!_commands.TryGetValue(target, out methodInfo) || methodInfo == null) return "Unknown Command\n";

      try
      {
        return (string)methodInfo.Invoke(this, new object[] { @params });
      }
      catch (Exception e)
      {
        return e.ToString();
      }
    }

    protected CommandAttribute GetDefaultSubcommand()
    {
      return DefaultCommand.Instance;
    }

    protected CommandAttribute GetSubcommand(string name)
    {
      return _commands.Keys.FirstOrDefault(command => command.Name == name);
    }
  }
}
