using UnityEngine;

public class ObjectMenuUI : MonoBehaviour
{
    [Header("Offset from target")]
    public Vector3 offset = new Vector3(0.2f, 0.2f, 0f); // смещение относительно объекта

    private GameObject currentTarget;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Show(GameObject target)
    {
        currentTarget = target;
        UpdatePosition();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        currentTarget = null;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (currentTarget != null)
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        if (currentTarget == null) return;

        // Позиция рядом с объектом
        transform.position = currentTarget.transform.position + offset;

        // Смотрим на камеру
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0); // фронт панели к камере
    }

    public void Delete()
    {
        if (currentTarget == null) return;
        Destroy(currentTarget);
        Hide();
    }

    public bool IsTarget(GameObject obj)
    {
        return obj == currentTarget;
    }
}
