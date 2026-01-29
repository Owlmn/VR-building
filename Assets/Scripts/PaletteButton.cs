using UnityEngine;
using UnityEngine.UI;

public class PaletteButton : MonoBehaviour
{
    [SerializeField] private ObjectMenuPalette palette;
    [SerializeField] private Color color;

    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (palette != null)
            palette.ApplyColor(color);
        else
        {
            Debug.Log("pallette is null");
        }
    }
}
