using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public float Money { get; private set; }
    public TMP_Text moneyDisplayText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddMoney(float amount)
    {
        Money += amount;
        Debug.Log("Money Added: " + amount + ". Total Money: " + Money);
        UpdateMoneyDisplay();
    }

    public bool TrySpendMoney(float amount)
    {
        if(Money >= amount)
        {
            Money -= amount;
            UpdateMoneyDisplay();
            Debug.Log("Money Subtracted: " + amount + ". Total Money: " + Money);
            return true;
        }
        return false;
    }

    public bool HasSufficientFunds(float amount)
    {
        return Money >= amount;
    }

    private void UpdateMoneyDisplay()
    {
        if(moneyDisplayText != null)
        {
            moneyDisplayText.text = $"Money: {Money}$";
        }
    }
}
