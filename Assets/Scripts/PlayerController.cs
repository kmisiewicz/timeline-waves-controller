using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float _MaxVelocity = 3f;
    [SerializeField] float _MaxVelocityChange = 10f;

    [Header("Combat")]
    [SerializeField] Projectile _ProjectilePrefab;
    [SerializeField] Transform _ProjectileParent;
    [SerializeField] float _ProjectileSpeed;
    [SerializeField, Min(0f)] float _AttackInterval = 0.5f;
    [SerializeField, Min(2f)] float _AttackDistance = 10f;
    [SerializeField] LayerMask _EnemyLayerMask;

    Rigidbody _rigidbody;
    Vector3 _input;
    float _attackTimer;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _attackTimer = _AttackInterval;
    }

    void Update()
    {
        _input = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            _input.z += 1;
        if (Input.GetKey(KeyCode.S))
            _input.z -= 1;
        if (Input.GetKey(KeyCode.A))
            _input.x -= 1;
        if (Input.GetKey(KeyCode.D))
            _input.x += 1;
        _input.Normalize();

        _attackTimer -= Time.deltaTime;
        if (_attackTimer <= 0)
        {
            Vector3 position = transform.position;
            Collider[] enemies = Physics.OverlapSphere(transform.position, _AttackDistance, _EnemyLayerMask, QueryTriggerInteraction.Ignore);
            Vector3 velocity = _input * _ProjectileSpeed;
            if (enemies.Length > 0)
            {
                Collider closest = enemies.Aggregate((currentClosest, other) => 
                    (currentClosest.transform.position - position).sqrMagnitude < (other.transform.position - position).sqrMagnitude ? currentClosest : other);
                velocity = (closest.transform.position - position).normalized * _ProjectileSpeed;
            }
            position.y = _ProjectileParent.position.y;
            Instantiate(_ProjectilePrefab, position, Quaternion.identity, _ProjectileParent).Init(velocity * _ProjectileSpeed, 5f);
            _attackTimer = _AttackInterval;
        }
    }

    void FixedUpdate()
    {
        Vector3 targetVelocity = _input * _MaxVelocity;
        Vector3 velocityChange = Vector3.ClampMagnitude(targetVelocity - _rigidbody.velocity, _MaxVelocityChange * Time.deltaTime);
        _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
    }
}
