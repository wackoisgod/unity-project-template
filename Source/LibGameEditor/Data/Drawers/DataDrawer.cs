using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LibGameEditor.Data.Drawers
{
  public abstract class DataDrawer
  {
    private static Color _backgroundColor = Color.white;
    private static bool _backgroundColored;

    private static Color BackgroundColor => _backgroundColor;

    private static void SwapBackgroundColor()
    {
      _backgroundColor = _backgroundColored ? Color.white : Color.grey;
      _backgroundColored ^= true;
    }

    public static void ResetColor()
    {
      _backgroundColor = Color.white;
      _backgroundColored = false;
    }

    private class DrawerInfo
    {
      public Type DrawerType;
      public CustomDataDrawerAttribute Attr;
    }

    private static Dictionary<Type, DrawerInfo> _drawerMap;

    private static Type GetDrawerCore(Type fieldType)
    {
      if (_drawerMap == null)
      {
        _drawerMap = new Dictionary<Type, DrawerInfo>();
        IEnumerable<Type> drawers = from assembly in AppDomain.CurrentDomain.GetAssemblies()
          from t in assembly.GetTypes()
          where t.IsSubclassOf(typeof(DataDrawer))
          select t;
        foreach (Type drawer in drawers)
        {
          IEnumerable<CustomDataDrawerAttribute> drawnTypes = from attr in drawer.GetCustomAttributes(typeof(CustomDataDrawerAttribute), false)
            select (attr as CustomDataDrawerAttribute);
          foreach (CustomDataDrawerAttribute drawn in drawnTypes)
          {
            _drawerMap.Add(drawn.Type, new DrawerInfo {DrawerType = drawer, Attr = drawn});
          }
        }
      }
      DrawerInfo output = null;
      Type drawnType = fieldType;
      do
      {
      } while (drawnType != null
               && !(_drawerMap.TryGetValue(drawnType, out output)
                    && ((fieldType == drawnType)
                        || output.Attr.UseForSubclass)
                    || (drawnType = drawnType.BaseType) == null)
        );
      if (output == null
          && fieldType.IsEnum)
      {
        if (fieldType.GetCustomAttributes(typeof(FlagsAttribute), false).Any())
          return typeof(EnumMaskDrawer);
        return typeof(EnumDrawer);
      }

      return output?.DrawerType;
    }

    public static void DrawObject(object obj)
    {
      PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
      foreach (PropertyInfo p in properties)
      {
        DrawProperty(p, obj);
      }
    }

    public static void DrawProperty(PropertyInfo property, object root)
    {
      if (property.GetCustomAttributes(typeof(LibCommon.Data.NonEditable), true).Length == 0)
      {
        if (property.PropertyType.IsArray)
        {
          object value = property.GetValue(root, null);
          Type drawer = GetDrawer(property, value, false, null);
          if (drawer != null)
          {
            DataDrawer drawerInstance = Activator.CreateInstance(drawer) as DataDrawer;
            drawerInstance?.Draw(value, property.Name, property, output => { property.SetValue(root, output, null); });
          }
          else
          {
            EditorGUILayout.LabelField(property.Name);
            if (value == null)
            {
              value = Activator.CreateInstance(property.PropertyType, 0);
            }
            Array arrayValue = value as Array;
            if (arrayValue != null)
            {
              for (int i = 0; i < arrayValue.Length; i++)
              {
                EditorGUI.indentLevel++;
                int index = i;
                GUIStyle style = new GUIStyle(GUI.skin.box);
                SwapBackgroundColor();
                GUI.backgroundColor = BackgroundColor;
                EditorGUILayout.BeginHorizontal(style);

                object element = arrayValue.GetValue(i);

                EditorGUILayout.BeginVertical();
                drawer = GetDrawer(property, element, true, typeof(DefaultDrawer));
                DataDrawer drawerInstance = Activator.CreateInstance(drawer) as DataDrawer;
                drawerInstance?.Draw(element, i.ToString(), property, output => { arrayValue.SetValue(output, index); });
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X"))
                {
                  Array copy = Array.CreateInstance(arrayValue.GetType().GetElementType(), arrayValue.Length - 1);
                  if (i > 0)
                  {
                    Array.Copy(arrayValue, 0, copy, 0, i);
                  }
                  if (i < arrayValue.Length - 1)
                  {
                    Array.Copy(arrayValue, i + 1, copy, i, arrayValue.Length - i - 1);
                  }
                  arrayValue = copy;
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();
                if (i < arrayValue.Length - 1)
                {
                  EditorGUILayout.Separator();
                }
                SwapBackgroundColor();
                EditorGUI.indentLevel--;
              }
              GUI.backgroundColor = Color.green;
              EditorGUILayout.BeginHorizontal();
              GUILayout.Space(EditorGUI.indentLevel*20);
              if (GUILayout.Button("+ " + arrayValue.GetType().GetElementType().Name))
              {
                Array copy = Array.CreateInstance(arrayValue.GetType().GetElementType(), arrayValue.Length + 1);
                if (arrayValue.Length > 0)
                {
                  Array.Copy(arrayValue, copy, arrayValue.Length);
                }
                arrayValue = copy;

                // we create an instance of the element type and assign it to the array ? 
                var objectInArray = Activator.CreateInstance(arrayValue.GetType().GetElementType());
                arrayValue.SetValue(objectInArray, arrayValue.Length - 1);
              }
              EditorGUILayout.EndHorizontal();
              GUI.backgroundColor = Color.white;
            }
            property.SetValue(root, arrayValue, null);
          }
        }
        else
        {
          object value = property.GetValue(root, null);
          Type drawer = GetDrawer(property, value, true, typeof(DefaultDrawer));

          DataDrawer drawerInstance = Activator.CreateInstance(drawer) as DataDrawer;
          drawerInstance?.Draw(value, property.Name, property, output => { property.SetValue(root, output, null); });
        }
      }
    }

    public abstract void Draw(object input, string name, PropertyInfo property, Action<object> setValueCallback);

    private static Type GetDrawer(PropertyInfo property, object value, bool useAttributes, Type defaultDrawer)
    {
      Type drawer = null;
      if (useAttributes)
      {
        object[] drawerAttr = property.GetCustomAttributes(typeof(LibCommon.Data.DataDrawAttribute), true);
        if (drawerAttr.Length > 0)
        {
          drawer = GetDrawerCore(drawerAttr[0].GetType());
        }
      }

      if (drawer == null
          && value != null)
      {
        drawer = GetDrawerCore(value.GetType());
      }
      if (drawer == null)
      {
        drawer = GetDrawerCore(property.PropertyType);
      }
      return drawer ?? defaultDrawer;
    }
  }
}
