using UnityEngine;
using UnityEngine.Events;

public class InteractObject : MonoBehaviour
{
    [SerializeField] private string interactionText = "Press E to Interact";
    [SerializeField] private UnityEvent onInteract;

    public string GetInteractionText()
    {
        return interactionText;
    }

    public void Interact()
    {
        onInteract.Invoke();
    }
}
