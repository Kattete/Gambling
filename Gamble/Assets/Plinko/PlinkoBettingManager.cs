using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.Assertions.Must;
using Unity.VisualScripting;
using System.Collections;

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

    [Header("Double Button References")]
    [SerializeField] private Button doubleButton;
    [SerializeField] private Image doubleButtonImage;
    [SerializeField] private TMP_Text doubleButtonText;
    public PlinkoGameManagerScript gameManagerScript;

    [Header("Glow Effect Settings")]
    [SerializeField] private Color normalColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color glowColor = new Color(1f, 0.8f, 0f);
    [SerializeField] private float glowSpeed = 2f;
    [SerializeField] private float glowIntensity = 1.2f;

    private Coroutine glowCoroutine;

    private float currentBet = 100f;
    private bool betPlaced = false;
    private List<BetHistoryEntry> betHistory = new List<BetHistoryEntry>();
    private int currentBetIndex = 0;

    private int pendingBalls = 0;
    public System.Action onBetAmountChanged;
    public System.Action<float, float, float> onBetResolved;

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
        InitializeDoubleBetButton();
        // Find the game manager if not assigned
        if (gameManagerScript == null)
        {
            gameManagerScript = FindFirstObjectByType<PlinkoGameManagerScript>();
        }
        // Set initil bet amount
        SelectBetAmount(0);
    }

    private void InitializeDoubleBetButton()
    {
        // Hide button initialy
        if(doubleButton != null)
        {
            doubleButton.gameObject.SetActive(false);
            doubleButton.onClick.AddListener(OnDoubleBetClicked);
        }
    }

    public void ShowDoubleBetButton()
    {
        if (doubleButton != null)
        {
            // calculate the doubled bet amount
            float doubledBet = currentBet * 2;

            // Only show if player can afford the doubled bet
            if (GameManager.Instance.HasSufficientFunds(doubledBet))
            {
                doubleButton.gameObject.SetActive(true);
                doubleButtonText.text = $"Double Bet\n({doubledBet}$)";
                StartGlowEffect();
            }
        }
    }

    private void OnDoubleBetClicked()
    {
        // Double the current bet
        int currentIndex = Array.IndexOf(betAmounts, currentBet);
        if(currentIndex < betAmounts.Length - 1)
        {
            // Find the next highest bet amunt thats closest to double
            float targetAmount = currentBet * 2;
            for (int i = currentIndex + 1; i < betAmounts.Length; i++)
            {
                if (betAmounts[i] >= targetAmount)
                {
                    SelectBetAmount(i);
                    break;
                }
            }
        }

        // Force ball count to 1 for double bet
        if(gameManagerScript != null && gameManagerScript.ballCountSlider != null)
        {
            gameManagerScript.ballCountSlider.value = 1;
        }

        // Try place bet and launch ball
        if (TryPlaceBet(1))
        {
            if(gameManagerScript != null)
            {
                StartCoroutine(gameManagerScript.LaunchMultipleBalls());
            }
        }

        // Hide the button after use
        HideDoubleBetButton();
    }

    private void HideDoubleBetButton()
    {
        if (doubleButton != null)
        {
            doubleButton.gameObject.SetActive(false);
            if(glowCoroutine != null)
            {
                StopCoroutine(glowCoroutine);
                glowCoroutine = null;
            }
        }
    }

    private void StartGlowEffect()
    {
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
        }
        glowCoroutine = StartCoroutine(GlowPulseEffect());
    }

    private IEnumerator GlowPulseEffect()
    {
        while (doubleButton.gameObject.activeInHierarchy)
        {
            // create smooth pulsing effect with a sine wave
            float pulseValue = (Mathf.Sin(Time.time * glowSpeed) + 1f) / 2f;

            // interpolate between normal and glow color
            doubleButtonImage.color = Color.Lerp(normalColor, glowColor, pulseValue * glowIntensity);

            yield return null;
        }
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

            // Notify listeners that bet amount changed
            onBetAmountChanged?.Invoke();
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

    public bool TryPlaceBet(int ballCount)
    {
        float totalBetAmount = currentBet * ballCount;
       if(!betPlaced && GameManager.Instance.TrySpendMoney(totalBetAmount))
        {
            betPlaced = true;
            pendingBalls = ballCount;
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

            onBetResolved?.Invoke(currentBet, multiplier, winAmount);

            // Add to history
            AddBetToHistory(currentBet, multiplier, winAmount);

            pendingBalls--;

            if(pendingBalls <= 0)
            {
                // Show Double bet button if it was a loss
                if(winAmount < currentBet)
                {
                    ShowDoubleBetButton();
                }
                // Reset betting state  
                ResetBetState();
            }
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
