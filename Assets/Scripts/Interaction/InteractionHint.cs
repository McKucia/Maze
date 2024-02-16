using UnityEngine;

public class InteractionHint : MonoBehaviour
{
    void Update()
    {
        if (!GameManager.Instance.Initialized) return;

        transform.LookAt(transform.position + 
            GameManager.Instance.MainCamera.rotation * Vector3.back, 
            GameManager.Instance.MainCamera.rotation * Vector3.up);
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
