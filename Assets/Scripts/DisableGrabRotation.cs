using UnityEngine;


public class DisableGrabRotation : MonoBehaviour
{
    void Awake()
    {
        var grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grab != null)
        {
            grab.trackRotation = false;  // выключаем вращение
        }
    }
}
