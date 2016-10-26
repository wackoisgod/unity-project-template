using UnityEngine;

namespace LibGameClient.Utils
{
  public class DoNotDestroyOnLoad : MonoBehaviour
  {
    // ReSharper disable once UnusedMember.Local
    private void Awake()
    {
      DontDestroyOnLoad(gameObject);
    }
  }
}
