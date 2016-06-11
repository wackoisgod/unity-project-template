using System;
using System.Collections.Generic;
using LibCommon.Manager;
using LibGameClient.UI.Controllers;
using LibGameClient.UI.Utils;
using UnityEngine;

namespace LibGameClient.Manager
{
  public class UIManager : BaseManager
  {
    public enum UILocation
    {
      Invalid,
      Splash,
      Loading,
      MainMenu
    }

    public enum UIControllerID
    {
      None = 0,
      Splash = 1,
      Loading = 2,
      MainMenu = 3
    }

    public static UIManager Instance { get; private set; }

    public UILocation CurrentLocation { get; private set; }
    public GameObject UIRootGameObject;
    public GameObject UICamera;
    public GameManager EventSystem;

    private readonly List<UIController> _controllers = new List<UIController>();

    public override void Init()
    {
      if (Instance == null)
        Instance = this;

      // we initialize to invalid 
      CurrentLocation = UILocation.Invalid;

      // we grab the UI Root and we set it not to destroy so when we load different scenes
      // we can change objects and such :P 
      UIRootGameObject = GameObject.FindGameObjectWithTag("UIRoot");
      UnityEngine.Object.DontDestroyOnLoad(UIRootGameObject);

      var controllers = UIRootGameObject.GetComponentsInChildren<UIController>(true);
      foreach (var item in controllers)
      {
        _controllers.Add(item);
      }
    }

    public override void Begin()
    {
      base.Begin();

      // we hook into this event so we can know when a game state changes. 
      GameManager.Instance.OnApplicationStateChanged += OnApplicationStateChange;
    }

    public void OnApplicationStateChange(GameManager.ApplicationState toState, GameManager.ApplicationState fromState)
    {
      if (toState == fromState) return;

      UILocation oldUILocation = CurrentLocation;
      switch (toState)
      {
        case GameManager.ApplicationState.Invalid:
          break;
        case GameManager.ApplicationState.Splash:
          PushUIController(UIControllerID.Splash);
          break;
        case GameManager.ApplicationState.Loading:
          PushUIController(UIControllerID.Loading);
          break;
        case GameManager.ApplicationState.MainMenu:
          PushUIController(UIControllerID.MainMenu);
          break;
        default:
          break;
      }
    }

    public List<UIController> GetUIControllers()
    {
      return _controllers;
    }

    public UIController GetUIController(UIControllerID inID)
    {
      try
      {
        var controller = _controllers.Find(x => x.ID == inID);
        return controller;
      }
      catch (Exception)
      {
        Debug.LogError("Can't find ID " + inID);
      }

      return null;
    }

    public void PushUIController(UIControllerID inID, bool bringToFront = false)
    {
      var controller = GetUIController(inID);
      if (controller != null)
      {
        controller.Show();

        if (bringToFront)
        {
          // this will brint it to the front of the UI! 
          UIUtils.BringToFront(controller.gameObject);
        }
      }
    }

    public void PopUIController(UIControllerID inID)
    {
      var controller = GetUIController(inID);
      controller?.Hide();
    }
  }
}
