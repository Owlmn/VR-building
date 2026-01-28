using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRRayInteractor))]
public class XRRayObjectSelector : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference triggerAction;

    [Header("UI")]
    [SerializeField] private ObjectMenuUI objectMenu;

    [Header("Layers")]
    [SerializeField] private LayerMask selectableLayers; // только эти слои можно выбирать

    private XRRayInteractor rayInteractor;
    private bool clickPressed = false;

    private void Awake()
    {
        rayInteractor = GetComponent<XRRayInteractor>();
    }

    private void OnEnable()
    {
        triggerAction.action.performed += OnTriggerPerformed;
        triggerAction.action.canceled += OnTriggerCanceled;
        triggerAction.action.Enable();
    }

    private void OnDisable()
    {
        triggerAction.action.performed -= OnTriggerPerformed;
        triggerAction.action.canceled -= OnTriggerCanceled;
        triggerAction.action.Disable();
    }

    private void OnTriggerPerformed(InputAction.CallbackContext ctx)
    {
        if (clickPressed) return;
        clickPressed = true;

        // Сначала проверяем попадание по объекту
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            // Игнорируем объекты вне selectableLayers
            if (((1 << hit.collider.gameObject.layer) & selectableLayers) == 0)
                return;

            // Показать HUD меню
            objectMenu.Show(hit.collider.gameObject);
        }
        else
        {
            // Клик в пустоту → скрыть меню
            objectMenu.Hide();
        }
    }

    private void OnTriggerCanceled(InputAction.CallbackContext ctx)
    {
        clickPressed = false;
    }
}
