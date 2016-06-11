using LibCommon.Assets;
using UnityEditor;

namespace LibGameEditor.Data
{
  [InitializeOnLoad]
  public class EditorDataLoaderSetup
  {
    static EditorDataLoaderSetup()
    {
      AssetStore.SetInstance(new Assets.EditorAssetStore());
    }
  }
}
