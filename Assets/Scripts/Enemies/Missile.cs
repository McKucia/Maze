using UnityEngine;

public class Missile : MonoBehaviour
{
    [HideInInspector] public Transform Target;
    [SerializeField] float _howHigh = 10f;
    [SerializeField] float _gravity = -18;

    Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        Launch();
    }

    void Update()
    {
        CheckCollide();
    }

    void Launch()
    {
        Physics.gravity = Vector3.up * _gravity;
        _rb.velocity = CalculateLaunchData();
    }

    Vector3 CalculateLaunchData()
    {
        float displacementY = Target.position.y - transform.position.y;
        Vector3 displacementXZ = new Vector3(Target.position.x - transform.position.x, 0, Target.position.z - transform.position.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * _gravity * _howHigh);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * _howHigh / _gravity) + Mathf.Sqrt(2 * (displacementY - _howHigh) / _gravity));

        return velocityXZ + velocityY;
    }

    void CheckCollide()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.1f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.tag == "Player")
                Target.gameObject.GetComponent<PlayerMovement>().Hit();
            Destroy(this.gameObject);
        }
    }
}