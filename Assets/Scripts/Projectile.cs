using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] Rigidbody _Rigidbody;

    Vector3 velocity;

    void OnDestroy()
    {
        CancelInvoke();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            KillMe();
    }

    public void Init(Vector3 velocity, float maxTime)
    {
        _Rigidbody.velocity = velocity;
        Invoke(nameof(KillMe), maxTime);
    }

    void KillMe()
    {
        Destroy(gameObject);
    }
}
