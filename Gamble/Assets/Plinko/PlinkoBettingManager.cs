using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.Assertions.Must;

[System.Serializable]
public class BetHistoryEntry
{
    public float betAmount;
    public float multiplier;
    public float winAmount;
    public string timestamp;

    public BetHistoryEntry(float bet, float mult, float win)
    {
        betAmount = bet;
        multiplier = mult;
        winAmount = win;
        timestamp = DateTime.Now.ToString("HH:mm:ss");
    }
}

public class PlinkoBettingManager : MonoBehaviour
{
    [Header("Betting Buttons")]
    public Button[] betButtons;
    public float[] betAmounts = { 100f, 250f, 500f, 1000f, 2500f, 5000f };

    [Header("UI Elements")]
    public TMP_Text selectedBetText;
    public Button placeBetButton;
    public GameObject betHistoryPanel;
    public GameObject betHistoryEntryPrefab;
    public Transform betHistoryContent;
    public int maxHistoyrEntries = 10;

    private float currentBet = 100f;
    private bool betPlaced = false;
    private List<BetHistoryEntry> betHistory = new List<BetHistoryEntry>();

    private void Start()
    {
        InitializeButtons();
        placeBetButton.onClick.AddListener(PlaceBet);
    }

    void InitializeButtons()
    {
        // Set up bet amount buttons
        for (int i = 0; i < betButtons.Length && i < betAmounts.Length; i++)
        {
            float betAmount = betAmounts[i];
            betButtons[i].GetComponentInChildren<TMP_Text>().text = $"{betAmount}$";

            int index = i;
            betButtons[i].onClick.AddListener(() => SelectBetAmount(index));
        }
    }

    void SelectBetAmount(int buttonIndex)
    {
        if (!betPlaced && buttonIndex < betAmounts.Length)
        {
            currentBet = betAmounts[buttonIndex];
            selectedBetText.text = $"Selected Bet: {currentBet}$";

            // Enable place bet button if player has enough money
            placeBetButton.interactable = GameManager.Instance.HasSufficientFunds(currentBet);
            // Highlight selected button and unhighlight others
            for (int i = 0; i < betButtons.Length; i++)
            {
                betButtons[i].GetComponent<Image>().color = (i == buttonIndex ? Color.green : Color.white);
            }
        }
    }

    public void PlaceBet()
    {
        if (currentBet > 0 && GameManager.Instance.TrySpendMoney(currentBet))
        {
            betPlaced = true;
            DisableBetButtons();
        }
    }

    private void DisableBetButtons()
    {
        foreach (Button button in betButtons)
        {
            button.interactable = false;
        }
        placeBetButton.interactable = false;
    }

    private void EnableBetButtons()
    {
        foreach(Button button in betButtons)
        {
            button.interactable = true;
            button.GetComponent<Image>().color = Color.white;
        }
    }

    public void ProcessWin(float multiplier)
    {
        if (betPlaced)
        {
            float winAmount = currentBet * multiplier;
            GameManager.Instance.AddMoney(winAmount);

            // Add to history
            AddBetToHistory(currentBet, multiplier, winAmount);

            // Reset betting state
            ResetBet();
        }
    }

    private void AddBetToHistory(float betAmount, float multiplier, float winAmount)
    {
        // Create new history entry
        BetHistoryEntry entry = new BetHistoryEntry(betAmount, multiplier, winAmount);
        betHistory.Insert(0, entry);

        // Limit history size
        if(betHistory.Count > maxHistoyrEntries)
        {
            betHistory.RemoveAt(betHistory.Count - 1);
        }

        UpdateHistoryDisplay();
    }

    private void UpdateHistoryDisplay() {
        // clear existing history
        foreach(Transform child in betHistoryContent)
        {
            Destroy(child.gameObject);
        }

        // create new history entries
        foreach(BetHistoryEntry entry in betHistory)
        {
            GameObject historyEntryObj = Instantiate(betHistoryEntryPrefab, betHistoryContent);
            TMP_Text entryText = historyEntryObj.GetComponent<TMP_Text>();
            entryText.text = $"[{entry.timestamp}] Bet: {entry.betAmount}$ | {entry.multiplier}x | Won: {entry.winAmount}$";

            // color code based on win/loss
            entryText.color = (entry.winAmount > entry.betAmount) ? Color.green : Color.red;
        }
    }

    private void ResetBet()
    {
        betPlaced = false;
        currentBet = 100f;
        selectedBetText.text = "Selected Bet: 100$";
        EnableBetButtons();
        placeBetButton.interactable = false;
    }
}
