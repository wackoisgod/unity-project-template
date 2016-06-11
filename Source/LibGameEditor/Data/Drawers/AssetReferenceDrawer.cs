using System;
using System.Reflection;
using LibCommon.Data;
using UnityEditor;
using UnityEngine;

namespace LibGameEditor.Data.Drawers
{
  [CustomDataDrawer(typeof(AssetReferenceAttribute))]
  public class AssetReferenceDrawer : DataDrawer
  {
    public override void Draw(object input, string name, PropertyInfo property, Action<object> setValueCallback)
    {
      string value = (string) input ?? "";
      string path = AssetDatabase.GUIDToAssetPath(value);
      UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(path);

      object[] attr = property.GetCustomAttributes(typeof(AssetReferenceAttribute), true);
      AssetReferenceAttribute drawerAttr = attr[0] as AssetReferenceAttribute;
      if (drawerAttr != null)
      {
        switch (drawerAttr.AssetType)
        {
          case AssetReferenceAttribute.AssetReferenceType.Material:
            asset = EditorGUILayout.ObjectField(name, asset, typeof(Material), false);
            break;
          case AssetReferenceAttribute.AssetReferenceType.AudioClip:
            asset = EditorGUILayout.ObjectField(name, asset, typeof(AudioClip), false);
            break;
          case AssetReferenceAttribute.AssetReferenceType.GameObject:
            asset = EditorGUILayout.ObjectField(name, asset, typeof(GameObject), false);
            break;
          case AssetReferenceAttribute.AssetReferenceType.Texture:
            asset = EditorGUILayout.ObjectField(name, asset, typeof(Texture), false);
            break;
          case AssetReferenceAttribute.AssetReferenceType.Animation:
            asset = EditorGUILayout.ObjectField(name, asset, typeof(AnimationClip), false);
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
      }

      path = AssetDatabase.GetAssetPath(asset);
      value = AssetDatabase.AssetPathToGUID(path);
      setValueCallback(value);
    }
  }
}
