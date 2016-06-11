using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace LibCommon.Data
{
  public class Serializer
  {
    public static void Serialize<T>(T data, string path)
    {
      Serialize(typeof(T), data, path);
    }

    public static void Serialize(Type dataType, object data, string path)
    {
      List<Type> processedTypes = new List<Type>();
      List<Type> polymorphicTypes = GetPolymorphicTypes(dataType, processedTypes);
      Serialize(dataType, data, path, polymorphicTypes.ToArray());
    }

    public static void Serialize(Type dataType, object data, string path, Type[] extraTypes)
    {
      XmlSerializer serializer = new XmlSerializer(dataType, extraTypes);
      using (XmlTextWriter writer = new XmlTextWriter(path, Encoding.UTF8))
      {
        writer.Formatting = Formatting.Indented;
        if (writer.Settings != null)
        {
          writer.Settings.Indent = true;
          writer.Settings.NewLineChars = "/n";
        }
        serializer.Serialize(writer, data);
      }
    }

    public static T Deserialize<T>(string path)
    {
      return (T) Deserialize(typeof(T), path);
    }

    private static object Deserialize(Type dataType, Stream stream, Type[] extraTypes)
    {
      XmlSerializer serialzier = new XmlSerializer(dataType, extraTypes);
      using (XmlTextReader reader = new XmlTextReader(stream))
      {
        return serialzier.Deserialize(reader);
      }
    }

    public static object Deserialize(Type dataType, string filePath)
    {
      List<Type> processedTyeps = new List<Type>();
      List<Type> polymorphicTypes = GetPolymorphicTypes(dataType, processedTyeps);
      return Deserialize(dataType, filePath, polymorphicTypes.ToArray());
    }

    public static object Deserialize(Type dataType, string filePath, Type[] additionalTypes)
    {
      using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
      {
        return Deserialize(dataType, fs, additionalTypes);
      }
    }

    public static T Deserialize<T>(byte[] bytes)
    {
      return (T) Deserialize(typeof(T), bytes);
    }

    public static object Deserialize(Type dataType, byte[] bytes)
    {
      List<Type> processedTypes = new List<Type>();
      List<Type> polymorphicTypes = GetPolymorphicTypes(dataType, processedTypes);
      return Deserialize(dataType, bytes, polymorphicTypes.ToArray());
    }

    public static object Deserialize(Type dataType, byte[] bytes, Type[] additionalTypes)
    {
      using (MemoryStream ms = new MemoryStream(bytes))
      {
        return Deserialize(dataType, ms, additionalTypes);
      }
    }

    public static bool CanDeserialize<T>(string path)
    {
      return CanDeserialize(typeof(T), path);
    }

    public static bool CanDeserialize(Type dataType, string path)
    {
      List<Type> processedTypes = new List<Type>();
      return CanDeserialize(dataType, path, GetPolymorphicTypes(dataType, processedTypes).ToArray());
    }

    public static bool CanDeserialize(Type dataType, string path, Type[] extraTypes)
    {
      XmlSerializer serializer = new XmlSerializer(dataType, extraTypes);
      using (XmlTextReader reader = new XmlTextReader(path))
      {
        return serializer.CanDeserialize(reader);
      }
    }

    private static List<Type> GetPolymorphicTypes(object obj)
    {
      List<Type> processedTypes = new List<Type>();
      List<Type> polymorphicTypes = GetPolymorphicTypes(obj.GetType(), processedTypes);
      PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
      foreach (PropertyInfo p in properties)
      {
        object subObj = p.GetValue(obj, null);
        Array arrayObj = subObj as Array;
        if (arrayObj != null)
        {
          for (int i = 0; i < arrayObj.Length; i++)
          {
            object element = arrayObj.GetValue(i);
            polymorphicTypes.AddRange(GetPolymorphicTypes(element));
          }
        }
        else if (subObj != null
                 && !subObj.GetType().IsPrimitive
                 && subObj.GetType() != typeof(string))
        {
          polymorphicTypes.AddRange(GetPolymorphicTypes(subObj));
        }
      }

      return polymorphicTypes;
    }

    private static List<Type> GetPolymorphicTypes(Type t, List<Type> processedTypes)
    {
      List<Type> polymorphicTypes = new List<Type>();
      if (!processedTypes.Contains(t))
      {
        processedTypes.Add(t);
        PropertyInfo[] properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo p in properties)
        {
          if (!(p.PropertyType.IsAssignableFrom(t)
                || t.IsAssignableFrom(p.PropertyType)))
          {
            object[] polymorphicAttr = p.GetCustomAttributes(typeof(PolymorphicAttribute), true);
            if (polymorphicAttr.Length > 0)
            {
              Type[] subclasses =
                DataUtils.GetSubclasses(p.PropertyType.HasElementType ? p.PropertyType.GetElementType() : p.PropertyType);
              foreach (Type subType in subclasses)
              {
                if (!polymorphicTypes.Contains(subType))
                {
                  polymorphicTypes.AddRange(GetPolymorphicTypes(subType, processedTypes));
                }
              }
              polymorphicTypes.AddRange(subclasses);
            }

            if (p.PropertyType.HasElementType)
            {
              polymorphicTypes.AddRange(GetPolymorphicTypes(p.PropertyType.GetElementType(), processedTypes));
            }
            else if (!p.PropertyType.IsPrimitive
                     && p.PropertyType != typeof(string)
                     && !polymorphicTypes.Contains(p.PropertyType))
            {
              GetPolymorphicTypes(p.PropertyType, processedTypes);
            }
          }
        }
      }
      return polymorphicTypes;
    }
  }
}
