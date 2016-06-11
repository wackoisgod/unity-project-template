using System;
using UnityEngine;

namespace LibGameClient.UI.Utils
{
  public static class UIUtils
  {
    public static void BringToFront(GameObject inObject)
    {
      Canvas root = FindInParents<Canvas>(inObject);
      if (root != null)
        inObject.transform.SetParent(root.transform, true);

      inObject.transform.SetAsLastSibling();
    }

    public static T FindInParents<T>(GameObject inObject) where T : Component
    {
      if (inObject == null)
        return null;

      var comp = inObject.GetComponent<T>();
      if (comp != null)
        return comp;

      Transform t = inObject.transform.parent;

      while (t != null && comp == null)
      {
        comp = t.gameObject.GetComponent<T>();
        t = t.parent;
      }

      return comp;
    }

    public static CompType GetChildOfTypeWithName<CompType>(GameObject go, string compName) where CompType : Component
    {
      // if we have a game object
      if (null != go)
      {
        CompType[] subComps = go.GetComponentsInChildren<CompType>(true);
        return Array.Find(subComps, x => x.name == compName);
      }

      return null;
    }
  }
}
