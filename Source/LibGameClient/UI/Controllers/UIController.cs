using System;
using LibGameClient.Manager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LibGameClient.UI.Controllers
{
  [DisallowMultipleComponent, ExecuteInEditMode]
  public class UIController : MonoBehaviour
  {
    public enum VisualState
    {
      Shown,
      Hidden
    }

    [SerializeField] private UIManager.UIControllerID _controllerId = UIManager.UIControllerID.None;

    public UIManager.UIControllerID ID => _controllerId;

    [Serializable]
    public class VisualStateEvent : UnityEvent<UIController, VisualState, bool>
    {
    }

    public VisualStateEvent OnVisualStateChange = new VisualStateEvent();

    private VisualState _currentVisualState = VisualState.Hidden;

    protected virtual bool IsActive()
    {
      return (enabled && gameObject.activeInHierarchy);
    }

    public void OnSelect(BaseEventData eventData)
    {
      //throw new NotImplementedException();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
      //throw new NotImplementedException();
    }

    public virtual void Show()
    {
      if (!IsActive())
        return;

      if (_currentVisualState == VisualState.Shown)
        return;

      _currentVisualState = VisualState.Shown;
      OnVisualStateChange.Invoke(this, _currentVisualState, true);
    }

    public virtual void Hide()
    {
      if (!IsActive())
        return;

      if (_currentVisualState == VisualState.Hidden)
        return;

      _currentVisualState = VisualState.Hidden;
      OnVisualStateChange.Invoke(this, _currentVisualState, false);
    }
  }
}
