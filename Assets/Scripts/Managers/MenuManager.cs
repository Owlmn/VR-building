using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Button firstButton; // Перетащите сюда первую кнопку (например, "Start")

    void OnEnable()
    {
        firstButton = GetComponent<Button>();
        if (firstButton != null)
        {
            firstButton.Select(); // Устанавливает фокус на первую кнопку
            firstButton.OnSelect(null); // Вызывает событие выделения
        }
    }
}
