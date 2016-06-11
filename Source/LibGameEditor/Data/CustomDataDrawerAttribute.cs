using System;

namespace LibGameEditor.Data
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public class CustomDataDrawerAttribute : Attribute
  {
    public Type Type;
    public bool UseForSubclass;

    public CustomDataDrawerAttribute(Type targetType) : this(targetType, false)
    {
    }

    public CustomDataDrawerAttribute(Type targetType, bool useForSubclass)
    {
      Type = targetType;
      UseForSubclass = useForSubclass;
    }
  }
}
