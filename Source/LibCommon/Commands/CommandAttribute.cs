using System;

namespace LibCommon.Commands
{
  [AttributeUsage(AttributeTargets.Class)]
  public class CommandGroupAttribute : Attribute
  {
    public string Name { get; private set; }
    public string Help { get; private set; }

    public CommandGroupAttribute(string name, string help)
    {
      Name = name.ToLower();
      Help = help;
    }
  }

  [AttributeUsage(AttributeTargets.Method)]
  public class CommandAttribute : Attribute
  {
    public string Name { get; private set; }
    public string Help { get; private set; }

    public CommandAttribute(string command, string help)
    {
      Name = command.ToLower();
      Help = help;
    }
  }

  [AttributeUsage(AttributeTargets.Method)]
  public class DefaultCommand : CommandAttribute
  {
    public static DefaultCommand Instance = new DefaultCommand();

    public DefaultCommand()
      : base("", "")
    {
    }
  }
}
