using System.IO;
using LibCommon.Manager;
using LibGameClient.Data;
using LibGameClient.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace LibGameClient.UI.Controllers
{
  public class LoadingScreenController : MonoBehaviour
  {
    public Text LoadingText;

    public void OnVisualStateChange(UIController inController, UIController.VisualState inState, bool inValue)
    {
      if (inState == UIController.VisualState.Shown)
      {
        InitLoading();
      }
      else
      {
        gameObject.SetActive(false);
      }
    }

    public void InitLoading()
    {
      gameObject.SetActive(true);

      OfflinePhaseOneLoad();
    }

    private void FinishedLoading(int inErrors)
    {
      Debug.Log("FinishedLoading");

      UIManager.Instance.PopUIController(UIManager.UIControllerID.Loading);
      GameManager.Instance.CurrentApplicationState = GameManager.ApplicationState.MainMenu;
    }

    private void OfflinePhaseOneLoad()
    {
      // lets load it from disk!
      var dataPath = Application.persistentDataPath + "/Data/AssetManifest.xml";
      if (File.Exists(dataPath))
      {
        //var manifestData = File.ReadAllBytes(dataPath);
        //AssetManager.Instance.LoadManifestFile(manifestData);
      }

      PhaseTwoLoad();
    }

    private void PhaseTwoLoad()
    {
      // load the data store with the downloaded data ? 
      var ccDataLoad = new ClientDataLoader();
      ccDataLoad.OnDataLoadComplete += errors => { PhaseThreeLoad(); };
      ccDataLoad.PopulateDataStore();
    }

    private void PhaseThreeLoad()
    {
      PhaseFourLoad();
    }

    private void PhaseFourLoad()
    {
      // we loaded the data and no we should load the MainMenu scene ? 
      var loadMainMenu = new AsyncSceneLoader("GameScene");
      loadMainMenu.OnCompleteLoading += asset =>
      {
        // we now have loaded everything we should have we can now move to the next
        // phase of the game!
        FinishedLoading(0);
      };
      AssetManager.Instance.RequestAssetLoad(loadMainMenu);
    }
  }
}
