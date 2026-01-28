using UnityEngine;


// Ensure the Unity.XR.Interaction.Toolkit package is installed in your project via the Unity Package Manager.

public class XRRayOutlineToggle : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;
    private Outline currentOutline;

    void Awake()
    {
        rayInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
    }

    void Update()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            var outline = hit.collider.GetComponent<Outline>();

            if (outline != null)
            {
                if (currentOutline != outline)
                {
                    ClearCurrent();
                    outline.enabled = true;
                    currentOutline = outline;
                }
                return;
            }
        }

        ClearCurrent();
    }

    void ClearCurrent()
    {
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
            currentOutline = null;
        }
    }
}
