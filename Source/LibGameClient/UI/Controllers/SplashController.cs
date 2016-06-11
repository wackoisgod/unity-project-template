using System.Collections;
using LibGameClient.Manager;
using UnityEngine;

namespace LibGameClient.UI.Controllers
{
  public class SplashScreenController : MonoBehaviour
  {
    public GameObject PublisherSplash;
    public GameObject CompanySplash;

/*
    private bool _publisherPlayed = false;
*/
/*
    private bool _companyPlayed = false;
*/

    private void Awake()
    {
      PublisherSplash?.SetActive(false);
      CompanySplash?.SetActive(false);
    }

    public void OnVisualStateChange(UIController inController, UIController.VisualState inState, bool inValue)
    {
      if (inState == UIController.VisualState.Shown)
      {
        InitSplashScreen();
      }
      else
      {
        gameObject.SetActive(false);
      }
    }

    public void InitSplashScreen()
    {
      gameObject.SetActive(true);

      CompanySplash.SetActive(true);
      StartCoroutine(PumpStateFunction());
    }

    public IEnumerator PumpStateFunction()
    {
      yield return new WaitForSeconds(4);

      // We should figure out how to make this work a little better ? 
      UIManager.Instance.PopUIController(UIManager.UIControllerID.Splash);
      GameManager.Instance.CurrentApplicationState = GameManager.ApplicationState.Loading;
    }
  }
}
