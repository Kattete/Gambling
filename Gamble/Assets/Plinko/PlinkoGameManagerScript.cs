using UnityEngine;

public class PlinkoGameManagerScript : MonoBehaviour
{
    public GameObject ballPrefab;
    public Transform[] ballDropPoint;

    public void LaunchBall()
    {
        GameObject newBall = Instantiate(ballPrefab, ballDropPoint[Random.Range(0, ballDropPoint.Length)].position, Quaternion.identity);
        PlinkoBallController ballController = newBall.GetComponent<PlinkoBallController>();
        Rigidbody2D ballRigidbody = newBall.GetComponent<Rigidbody2D>();
        ballController.DropBall(ballDropPoint, ballRigidbody);
    }
}
