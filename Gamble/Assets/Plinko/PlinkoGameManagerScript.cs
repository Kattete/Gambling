using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlinkoGameManagerScript : MonoBehaviour
{
    public GameObject ballPrefab;
    public Transform[] ballDropPoint;

    public Slider ballCountSlider;
    public TMP_Text ballCountText;
    public TMP_Text totalBetText;

    private int currentBallCount = 1;
    private PlinkoBettingManager bettingManager;

    private void Start()
    {
        SetupBallCountSlider();
        bettingManager = PlinkoBettingManager.Instance;
        if (bettingManager != null)
        {
            bettingManager.onBetAmountChanged += UpdateTotalBetDisplay;
        }
        // Initial update
        UpdateTotalBetDisplay();
    }

    private void OnDestroy()
    {
        // unsubscribe to prevent memory leaks
        if(bettingManager != null)
        {
            bettingManager.onBetAmountChanged -= UpdateTotalBetDisplay;
        }
    }

    private void SetupBallCountSlider()
    {
        if(ballCountSlider != null)
        {
            ballCountSlider.minValue = 1;
            ballCountSlider.maxValue = 10;
            ballCountSlider.wholeNumbers = true;
            ballCountSlider.value = 1;

            ballCountSlider.onValueChanged.AddListener(OnBallCountChanged);

            UpdateBallCountDisplay(1);
        }
    }

    private void OnBallCountChanged(float value)
    {
        currentBallCount = (int)value;
        UpdateBallCountDisplay(currentBallCount);
        UpdateTotalBetDisplay();
    }

    private void UpdateBallCountDisplay(int count)
    {
        if (ballCountText != null)
        {
            ballCountText.text = $"Balls: {count}";
        }
    }

    private void UpdateTotalBetDisplay()
    {
        if (totalBetText != null && bettingManager != null)
        {
            float totalBet = currentBallCount * bettingManager.GetCurrentBet();
            totalBetText.text = $"Total Bet: {totalBet}$";

            // Check if player can afford this bet
            bool canAfford = GameManager.Instance.HasSufficientFunds(totalBet);
            totalBetText.color = canAfford ? Color.white : Color.red;

            // Adjust ball count if can't afford
            if (!canAfford)
            {
                float maxAffordableBalls = Mathf.Floor(GameManager.Instance.Money / bettingManager.GetCurrentBet());
                if (ballCountSlider.value > maxAffordableBalls)
                {
                    ballCountSlider.value = Mathf.Max(1, maxAffordableBalls);
                }
            }
        }
    }
    public void LaunchBall()
    {
        float totalBetAmount = currentBallCount * PlinkoBettingManager.Instance.GetCurrentBet();
        // first check if we can place a bet before launching a ball
        if (PlinkoBettingManager.Instance.TryPlaceBet(currentBallCount))
        {
            // launch multiple balls
            StartCoroutine(LaunchMultipleBalls());
        }
    }

    public System.Collections.IEnumerator LaunchMultipleBalls()
    {
        for (int i =0; i < currentBallCount; i++)
        {
            GameObject newBall = Instantiate(ballPrefab, ballDropPoint[Random.Range(0, ballDropPoint.Length)].position, Quaternion.identity);
            PlinkoBallController ballController = newBall.GetComponent<PlinkoBallController>();
            Rigidbody2D ballRigidbody = newBall.GetComponent<Rigidbody2D>();
            ballController.DropBall(ballDropPoint, ballRigidbody);

            yield return new WaitForSeconds(0.2f);
        }    
    }
}
