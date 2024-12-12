using UnityEngine;

public class PlinkoBallController : MonoBehaviour
{
    [Header("Ball physics settings")]
    public float initialDropForce = 10f;
    public float horizontalRandomness = 0.1f;
    public float bounceCoefficient = 0.7f;
    public float minBounceVelocity = 0.5f;

    [Header("Collision Detection")]
    public LayerMask pegLayerMask;
    public LayerMask bottomLayerMask;

    private Rigidbody2D rb;
    private bool isDropped = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void DropBall(Transform[] dropPoint, Rigidbody2D rb)
    {
        transform.position = dropPoint[Random.Range(0, dropPoint.Length)].position;


        // Add initial drop force with slight horizontal randomness
        Vector2 dropForce = Vector2.down * initialDropForce;
        dropForce.x += Random.Range(-horizontalRandomness, horizontalRandomness);

        rb.AddForce(dropForce, ForceMode2D.Impulse);

        isDropped = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & pegLayerMask) != 0) {
            HandlePegCollision(collision);
        }

        if (((1 << collision.gameObject.layer) & bottomLayerMask) != 0) {
            HandleBottomCollision();
        }
    }

    void HandlePegCollision(Collision2D collision)
    {
        // Add slight randomness to ball trajectory after peg hit
        Vector2 randomDeviation = Random.insideUnitCircle * 0.5f;
        rb.AddForce(randomDeviation, ForceMode2D.Impulse);

        // Reduce velocity based on bounce coefficient
        rb.linearVelocity *= bounceCoefficient;
    }

    void HandleBottomCollision()
    {
        Debug.Log("Ball reached bottom!");
        Destroy(gameObject);
        GameManager.Instance.AddMoney(100);

    }

    private void FixedUpdate()
    {
        if(isDropped && rb.linearVelocity.magnitude < minBounceVelocity)
        {
            rb.linearVelocity = Vector2.zero;
            isDropped = false;
        }
    }

    public void ResetBall()
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        isDropped = false;
    }
}
