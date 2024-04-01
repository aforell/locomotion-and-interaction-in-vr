using UnityEngine;

[System.Serializable]
public class ButtonState
{
  public Material [] materials = {};
  public string [] stateIDs = {};
  public int stateIndex = 0;

  public bool hasStates ()
  {
    return materials.Length > 0 && stateIDs.Length > 0;
  }

  public Material getNextMaterial ()
  {
    if (stateIndex >= materials.Length)
    {
      stateIndex = 0;
    }

    Material result = materials[stateIndex];
    stateIndex++;
    return result;
  }
}