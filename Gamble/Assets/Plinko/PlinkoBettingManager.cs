using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.Assertions.Must;
using Unity.VisualScripting;

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
    public static PlinkoBettingManager Instance { get; private set; }

    [Header("Betting Buttons")]
    public Button[] betButtons;
    public float[] betAmounts = { 100f, 250f, 500f, 1000f, 2500f, 5000f };

    [Header("UI Elements")]
    public TMP_Text selectedBetText;
    public GameObject betHistoryPanel;
    public GameObject betHistoryEntryPrefab;
    public Transform betHistoryContent;
    public int maxHistoyrEntries = 10;

    private float currentBet = 100f;
    private bool betPlaced = false;
    private List<BetHistoryEntry> betHistory = new List<BetHistoryEntry>();
    private int currentBetIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        EnsureBetHistoryComponents();
        InitializeButtons();

        // Set initil bet amount
        SelectBetAmount(0);
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
            currentBetIndex = buttonIndex;
            selectedBetText.text = $"Selected Bet: {currentBet}$";

            // Update button viuals based on affordability
            UpdateButtonStates();
        }
    }

    private void UpdateButtonStates()
    {
        for (int i = 0; i < betButtons.Length; i++) {
            bool canAfford = GameManager.Instance.HasSufficientFunds(betAmounts[i]);
            betButtons[i].interactable = canAfford && !betPlaced;
            betButtons[i].GetComponent<Image>().color = (i == currentBetIndex && canAfford) ? Color.green : Color.white;
        }
    }

    public bool TryPlaceBet()
    {
       if(!betPlaced && GameManager.Instance.TrySpendMoney(currentBet))
        {
            betPlaced = true;
            UpdateButtonStates();
            return true;
        }
        return false;
    }

    // call this when a ball reaches a collectin box
    public void ProcessWin(float multiplier)
    {
        if (betPlaced)
        {
            float winAmount = currentBet * multiplier;
            GameManager.Instance.AddMoney(winAmount);

            // Add to history
            AddBetToHistory(currentBet, multiplier, winAmount);

            // Reset betting state
            ResetBetState();
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

            // Proper layout 
            RectTransform rect = historyEntryObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(0.5f, 0);

            TMP_Text entryText = historyEntryObj.GetComponent<TMP_Text>();
            if(entryText != null)
            {
                entryText.text = $"[{entry.timestamp}] Bet: {entry.betAmount}$ | {entry.multiplier}x | Won: {entry.winAmount}$";
                entryText.color = (entry.winAmount > entry.betAmount) ? Color.green : Color.red;
                entryText.alignment = TextAlignmentOptions.Center;
            }

            // Add Layout element for consistent sizing
            LayoutElement layoutElement = historyEntryObj.GetComponent<LayoutElement>();
            if(layoutElement == null)
            {
                layoutElement = historyEntryObj.AddComponent<LayoutElement>();
            }
            layoutElement.minHeight = 30f;
            layoutElement.flexibleWidth = 1f;

            Canvas.ForceUpdateCanvases();
        }
    }

    private void EnsureBetHistoryComponents()
    {
        // Add vertical layout
        if (!betHistoryContent.GetComponent<VerticalLayoutGroup>())
        {
            VerticalLayoutGroup vlg = betHistoryContent.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5f;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
        }

        // Add ContentSizeFitter 
        if (!betHistoryContent.GetComponent<ContentSizeFitter>())
        {
            ContentSizeFitter csf = betHistoryContent.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    private void ResetBetState()
    {
        betPlaced = false;
        UpdateButtonStates();
    }

    public float GetCurrentBet()
    {
        return currentBet;
    }
}
