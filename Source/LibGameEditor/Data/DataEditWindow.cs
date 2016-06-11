using System.Collections.Generic;
using System.Reflection;
using LibCommon.Data;
using LibGameEditor.Data.Drawers;
using UnityEditor;
using UnityEngine;

namespace LibGameEditor.Data
{
  public class DataEditWindow : EditorWindow
  {
    private Vector2 _scrollPosition;

    private class Tab
    {
      public BaseData Data { get; set; }
      public readonly string Path;

      public Tab(BaseData inputData, string savePath)
      {
        Data = inputData;
        Path = savePath;
      }
    }

    private readonly List<Tab> _tabs = new List<Tab>();

    private int _currentTab;

    public static void EditData(BaseData data, string path)
    {
      DataEditWindow window = GetWindow<DataEditWindow>();
      for (int i = 0; i < window._tabs.Count; i++)
      {
        if (path == window._tabs[i].Path)
        {
          window._currentTab = i;
          return;
        }
      }
      Tab addedTab = new Tab(data, path);
      window._tabs.Add(addedTab);
      window._currentTab = window._tabs.Count - 1;
      window.Show();
    }

    public void OnGUI()
    {
      if (_tabs == null)
      {
        Close();
        return;
      }

      GUILayout.BeginHorizontal();
      for (int i = 0; i < _tabs.Count; i++)
      {
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        if (_currentTab != i)
        {
          buttonStyle.normal = buttonStyle.active;
        }
        if (GUILayout.Button(_tabs[i].Data.Name, buttonStyle))
        {
          _currentTab = i;
        }

        if (!GUILayout.Button("X", buttonStyle, GUILayout.ExpandWidth(false))) continue;

        _tabs.RemoveAt(i);
        if (i <= _currentTab)
        {
          _currentTab--;
        }
      }
      GUILayout.EndHorizontal();

      if (_tabs.Count == 0)
      {
        Close();
        return;
      }
      _currentTab = Mathf.Clamp(_currentTab, 0, _tabs.Count - 1);
      if (GUILayout.Button("Save"))
      {
        Serializer.Serialize(_tabs[_currentTab].Data.GetType(), _tabs[_currentTab].Data,
          Application.dataPath + _tabs[_currentTab].Path);
        AssetDatabase.Refresh();
      }
      _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
      PropertyInfo property = typeof(Tab).GetProperty("Data", BindingFlags.Public | BindingFlags.Instance);
      DataDrawer.ResetColor();
      DataDrawer.DrawProperty(property, _tabs[_currentTab]);
      EditorGUILayout.EndScrollView();
      Repaint();
    }
  }
}
