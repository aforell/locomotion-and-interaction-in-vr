using UnityEngine;

public class SwitchActiveState : MonoBehaviour
{
    public void SwitchEnabledGO()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
