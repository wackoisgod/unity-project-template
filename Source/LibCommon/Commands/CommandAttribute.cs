using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibCommon.Commands
{
  [AttributeUsage(AttributeTargets.Class)]
  public class CommandGroupAttribute : Attribute
  {
    public string Name { get; private set; }
    public string Help { get; private set; }

    public CommandGroupAttribute(string name, string help)
    {
      this.Name = name.ToLower();
      this.Help = help;
    }
  }

  [AttributeUsage(AttributeTargets.Method)]
  public class CommandAttribute : Attribute
  {

    public string Name { get; private set; }
    public string Help { get; private set; }

    public CommandAttribute(string command, string help)
    {
      this.Name = command.ToLower();
      this.Help = help;
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
