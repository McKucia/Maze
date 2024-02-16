using UnityEngine;

public class ExitTile : MonoBehaviour
{
    [SerializeField] InteractionHint _interactionHint;

    public void ShowHideInteractionHint(bool show)
    {
        _interactionHint.SetActive(show);
    }

}
