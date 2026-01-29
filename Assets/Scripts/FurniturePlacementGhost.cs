using UnityEngine;
using System.Collections.Generic;

public class FurniturePlacementGhost : MonoBehaviour
{
    [Header("Follow")]
    public Transform head;
    public float distance = 5f;
    public float maxRaycastDistance = 50f;

    [Header("Floor Snapping")]
    public LayerMask floorMask; // Слой пола
    public float heightOffset = 0f; // Смещение от пола 
    public bool snapToFloor = true;

    [Header("Placement")]
    public LayerMask blockingMask;

    [Header("Colors")]
    public Color validColor = new Color(0f, 1f, 0f, 0.5f);
    public Color invalidColor = new Color(1f, 0f, 0f, 0.5f);
    

    Renderer[] renderers;
    Collider[] colliders;

    bool canPlace;
    public bool CanPlace => canPlace;

    void Awake()
    {
        // Удаляем MeshCollider, чтобы избежать concave trigger
        foreach (var mc in GetComponentsInChildren<MeshCollider>())
            Destroy(mc);

        // Добавляем BoxCollider к каждому Renderer
        var newColliders = new List<Collider>();
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            BoxCollider box = r.gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            newColliders.Add(box); // собираем только новые BoxCollider
        }

        colliders = newColliders.ToArray();
        renderers = GetComponentsInChildren<Renderer>();
    }


    void LateUpdate()
    {
        UpdatePosition();
        CheckCollisions();
        UpdateColor();
    }

    void UpdatePosition()
    {
        Vector3 forward = head.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 targetPos = head.position + forward * distance;
        transform.position = targetPos;

        transform.rotation = Quaternion.Euler(0f, head.eulerAngles.y, 0f);

        if (snapToFloor)
        {
            // Raycast вниз от целевой позиции
            Ray ray = new Ray(targetPos + Vector3.up * 10f, Vector3.down);
            
            if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, floorMask))
            {
                // Привязываем к полу
                targetPos.y = hit.point.y + heightOffset;
            }
            else
            {
                // Если пол не найден, пробуем raycast от головы вниз
                ray = new Ray(head.position, Vector3.down);
                if (Physics.Raycast(ray, out hit, maxRaycastDistance, floorMask))
                {
                    targetPos.y = hit.point.y + heightOffset;
                }
            }
        }

        transform.position = targetPos;
    }

    void CheckCollisions()
    {
        canPlace = true;

        foreach (var col in colliders)
        {
            Bounds b = col.bounds;

            Collider[] hits = Physics.OverlapBox(
                b.center,
                b.extents,
                transform.rotation,
                blockingMask
            );

            foreach (var h in hits)
            {
                if (!h.isTrigger && h.transform != transform)
                {
                    canPlace = false;
                    return;
                }
            }
        }
    }

    void UpdateColor()
    {
        Color target = canPlace ? validColor : invalidColor;

        foreach (var r in renderers)
        {
            foreach (var m in r.materials)
                m.color = target;
        }
    }

}