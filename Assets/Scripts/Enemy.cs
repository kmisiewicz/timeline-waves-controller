using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float _Velocity;
    [SerializeField] float _MaxVelocityChange;
    [Space]
    [SerializeField] Rigidbody _Rigidbody;

    Transform _target;
    Action _onDeath;

    void Start()
    {
        var player = (PlayerController)FindAnyObjectByType(typeof(PlayerController));
        if (player != null)
            _target = player.GetComponent<Transform>();
    }

    void FixedUpdate()
    {
        if (_target == null)
            return;

        Vector3 direction = _target.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        Vector3 velocityChange = (_Velocity * direction) - _Rigidbody.velocity;
        velocityChange = Vector3.ClampMagnitude(velocityChange, _MaxVelocityChange * Time.deltaTime);
        _Rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        _Rigidbody.velocity = Vector3.ClampMagnitude(_Rigidbody.velocity, _Velocity);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            _onDeath?.Invoke();
            Destroy(gameObject);
        }
    }

    void OnValidate()
    {
        if (_Rigidbody == null)
            _Rigidbody = GetComponent<Rigidbody>();
    }

    public void Init(Transform target, Action onDeath)
    {
        _target = target;
        _onDeath = onDeath;
    }
}
