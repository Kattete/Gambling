using UnityEngine;

public class CollectionBoxScript : MonoBehaviour
{
    private float multiplier = 1f;

    public void SetMultiplier(float newMultiplier)
    {
        multiplier = newMultiplier;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            PlinkoBettingManager.Instance.ProcessWin(multiplier);
            Destroy(collision.gameObject);
        }
    }
}
