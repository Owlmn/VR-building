using UnityEngine;

public class ObjectMenuPalette : MonoBehaviour
{
    private GameObject currentTarget;

    public void SetTarget(GameObject target)
    {
        currentTarget = target;
    }

    public void ApplyColor(Color color)
    {
        if (currentTarget == null) return;

        Renderer r = currentTarget.GetComponentInChildren<Renderer>();
        var mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);

        // Для URP/HDRP Lit используем _BaseColor
        if (r.sharedMaterial.HasProperty("_BaseColor"))
            mpb.SetColor("_BaseColor", color);
        else
            mpb.SetColor("_Color", color); // стандартный материал

        r.SetPropertyBlock(mpb);

    }
}
