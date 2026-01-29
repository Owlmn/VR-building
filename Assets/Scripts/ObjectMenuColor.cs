using UnityEngine;

public class ObjectMenuColor : MonoBehaviour
{
    private GameObject currentTarget; // текущий выбранный объект

    // Вызываем при открытии меню
    public void SetTarget(GameObject target)
    {
        currentTarget = target;
    }

    // Кнопка "Change Color"
    public void ChangeColor()
    {
        if (currentTarget == null) return;

        // Получаем все Renderer у объекта и дочерних объектов
        foreach (var r in currentTarget.GetComponentsInChildren<Renderer>())
        {
            r.material.color = Random.ColorHSV(); // случайный цвет
        }
    }
}
