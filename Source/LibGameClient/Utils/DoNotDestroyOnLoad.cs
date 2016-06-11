using UnityEngine;

namespace LibGameClient.Utils
{
  public class DoNotDestroyOnLoad : MonoBehaviour
  {
    private void Awake()
    {
      DontDestroyOnLoad(gameObject);
    }
  }
}
