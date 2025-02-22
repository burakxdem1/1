using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 3;
    [SerializeField] private GameObject interactionText;
    private InteractObject currentInteractable;

    [SerializeField] private LayerMask interactableLayer;

    private void Start()
    {
        interactionText.SetActive(false); // Oyun baþlarken interaction text'i kapat
    }

    private void Update()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer)) {
            InteractObject interactableObject = hit.collider.GetComponent<InteractObject>();

            if (interactableObject != null) {
                if (interactableObject != currentInteractable) {
                    currentInteractable = interactableObject;
                    interactionText.SetActive(true);

                    TextMeshProUGUI textComponent = interactionText.GetComponent<TextMeshProUGUI>();
                    if (textComponent != null) {
                        textComponent.text = currentInteractable.GetInteractionText();
                    }
                }
            } else {
                ClearInteraction();
            }
        } else {
            ClearInteraction();
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            currentInteractable?.Interact();
        }
    }

    private void ClearInteraction()
    {
        if (currentInteractable != null) {
            currentInteractable = null;
            interactionText.SetActive(false);
        }
    }
}
