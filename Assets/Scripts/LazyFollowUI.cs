using UnityEngine;

public class LazyFollowUI : MonoBehaviour
{
    [Header("Target")]
    public Transform head;   // XR Camera

    [Header("Position")]
    public float distance = 1.5f;
    public float fixedHeight = 1.3f;

    [Header("Smoothing")]
    public float positionLerp = 5f;
    public float rotationLerp = 5f;

    private void LateUpdate()
    {
        if (!head) return;

        // 1️⃣ Берём forward головы
        Vector3 forward = head.forward;

        // 2️⃣ УБИРАЕМ наклон вверх/вниз
        forward.y = 0f;

        // защита от нулевого вектора
        if (forward.sqrMagnitude < 0.001f)
            forward = head.forward;

        forward.Normalize();

        // 3️⃣ Целевая позиция
        Vector3 targetPos =
            head.position +
            forward * distance;

        // 4️⃣ ФИКСИРУЕМ высоту
        targetPos.y = fixedHeight;

        // 5️⃣ ПЛАВНО двигаем
        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * positionLerp
        );

        // 6️⃣ Поворот ТОЛЬКО по Y
        Quaternion targetRot = Quaternion.LookRotation(forward);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * rotationLerp
        );
    }
}
    