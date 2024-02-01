using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Missile : MonoBehaviour
{
    [HideInInspector] public Transform Target;
    [SerializeField] float _speed = 6f;

    void Start()
    {
        StartCoroutine(SendHoming());
    }

    IEnumerator SendHoming()
    {
        while(Vector3.Distance(Target.position, transform.position) > 0.3f)
        {
            transform.position += (Target.position - transform.position).normalized * _speed * Time.deltaTime;
            transform.LookAt(Target);

            if (Physics.OverlapSphere(transform.position, 0.1f).Length > 0) Destroy(gameObject);
            yield return null;
        }
        Destroy(gameObject);
    }
}
