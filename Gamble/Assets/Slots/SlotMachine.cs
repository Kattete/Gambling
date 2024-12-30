using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputManagerEntry;

[System.Serializable]
public class SymbolConfig
{
    public Sprite sprite;
    public float weight;
    public float value;
    public string name;
    public int minMatchesRequired = 3;
    public bool isWild = false;
    public bool isFreeSpinSymbol = false;
}

public class SlotMachine : MonoBehaviour
{
    public SymbolConfig[] symbols;
    private float totalWeight;

    // Each reel will be a collumn of 3 symbols
    public Transform[] reels = new Transform[5];
    // Matrix to store current symbols (3 rows X 5 columns)
    private Image[,] symbolMatrix = new Image[3, 5];

    public Button spinButton;

    // Configuration
    public float spinDuration = 2f;
    public float spinSpeed = 10f;
    private bool isSpinning = false;

    // Store Paylines
    public Image paylinePrefab;
    private Image[] paylineImages = new Image[20];

    // Add variables for win calculations
    private float currentBet = 100f;

    // Reel tracking
    private bool[] reelFinished = new bool[5];
    private int spinningReels = 0;

    // Free spins
    private bool isInFreeSpins = false;
    private int remainingFreeSpins = 0;
    private int initialFreeSpins = 8;
    private int retriggerFreeSpins = 5;
    public GameObject freeSpinsPanel;
    public Button freeSpinsButton;
    private bool waitingForFreeSpinsToStart = false;
    public GameObject freeSpinsTextSprite;
    public Sprite[] numberSprites;
    public GameObject numberContainer;
    public float spacingBetweenDigits = 30f;
    private List<Image> digitImages = new List<Image>();

    // Structure to represent a position on the grid
    [System.Serializable]
    public struct GridPosition
    {
        public int row;
        public int col;

        public GridPosition(int r, int c)
        {
            row = r;
            col = c;
        }
    }

    private GridPosition[][] paylines = new GridPosition[20][];

    private void Start()
    {
        SetupNumberDisplay();
        CalculateTotalWeight();
        InitializeMatrix();
        InitializePaylines();
        SetuPaylineSprites();
        spinButton.onClick.AddListener(StartSpin);
        // Set initiale random symbols
        PopulateInitialSymbols();
    }

    //---------------Start Free Spins------------------

    private void CheckFreeSpinsSymbols()
    {
        Dictionary<SymbolConfig, int> freeSpinSymbolCount = new Dictionary<SymbolConfig, int>();

        // Count free spin symbols across all positions
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                Sprite currentSprite = symbolMatrix[row, col].sprite;
                SymbolConfig symbolConfig = System.Array.Find(symbols, s => s.sprite == currentSprite);

                if (symbolConfig != null && symbolConfig.isFreeSpinSymbol)
                {
                    if (!freeSpinSymbolCount.ContainsKey(symbolConfig))
                    {
                        freeSpinSymbolCount[symbolConfig] = 0;
                    }
                    freeSpinSymbolCount[symbolConfig]++;
                }
            }
        }

        // Check for free spins activation or retrigger
        foreach (var kvp in freeSpinSymbolCount)
        {
            SymbolConfig symbol = kvp.Key;
            int count = kvp.Value;

            if (!isInFreeSpins && count >= 3)
            {
                StartFreeSpins();
            }
            else if (isInFreeSpins && count >= 2)
            {
                RetriggerFreeSpins();
            }
        }
    }

    private void SetupNumberDisplay()
    {
        // First, clear any existing digit images
        foreach (var digitImage in digitImages)
        {
            if (digitImage != null)
            {
                Destroy(digitImage.gameObject);
            }
        }
        digitImages.Clear();

        // Make sure we have a container
        if (numberContainer == null)
        {
            Debug.LogError("Number container is not assigned!");
            return;
        }
    }

    // This method updates the display with new numbers
    private void UpdateFreeSpinsDisplay(int number)
    {
        // Convert number to digits
        string numberStr = number.ToString();

        // Create or update digit images as needed
        while (digitImages.Count < numberStr.Length)
        {
            // Create new digit holder
            GameObject digitObj = new GameObject($"Digit_{digitImages.Count}");
            digitObj.transform.SetParent(numberContainer.transform, false);

            // Add Image component
            Image digitImage = digitObj.AddComponent<Image>();
            digitImages.Add(digitImage);

            // Set proper UI settings
            RectTransform rectTransform = digitImage.rectTransform;
            rectTransform.sizeDelta = new Vector2(50f, 50f); // Adjust size as needed
        }

        // Remove excess digit images if number is smaller than before
        while (digitImages.Count > numberStr.Length)
        {
            Image lastDigit = digitImages[digitImages.Count - 1];
            digitImages.RemoveAt(digitImages.Count - 1);
            Destroy(lastDigit.gameObject);
        }

        // Position and update sprites for each digit
        for (int i = 0; i < numberStr.Length; i++)
        {
            int digit = int.Parse(numberStr[i].ToString());
            Image digitImage = digitImages[i];

            // Set the sprite for this digit
            digitImage.sprite = numberSprites[digit];

            // Position the digit
            RectTransform rectTransform = digitImage.rectTransform;
            rectTransform.anchoredPosition = new Vector2(i * spacingBetweenDigits, 0);
        }
    }

    private void StartFreeSpins()
    {
        isInFreeSpins = true;
        remainingFreeSpins = initialFreeSpins;
        waitingForFreeSpinsToStart = true;

        UpdateFreeSpinsUI();

        // Play activation animation/sound
        StartCoroutine(PlayFreeSpinsActivation());
    }

    private void RetriggerFreeSpins()
    {
        remainingFreeSpins += retriggerFreeSpins;
        UpdateFreeSpinsUI();

        // Play retrigger animation/sound
        StartCoroutine(PlayRetriggerAnimation());
    }

    private void UpdateFreeSpinsUI()
    {
        // Show/hide the free spins display based on state
        if (freeSpinsTextSprite != null)
        {
            freeSpinsTextSprite.gameObject.SetActive(isInFreeSpins);
        }
        if (numberContainer != null)
        {
            numberContainer.SetActive(isInFreeSpins);
        }

        // Update the number display
        if (isInFreeSpins)
        {
            UpdateFreeSpinsDisplay(remainingFreeSpins);
        }

        // Update panel visibility
        if (freeSpinsPanel != null)
        {
            freeSpinsPanel.SetActive(isInFreeSpins && waitingForFreeSpinsToStart);
        }
    }

    // New coroutine to handle panel fade out
    private IEnumerator HideFreeSpinsPanel()
    {
        if (freeSpinsPanel != null)
        {
            // Get or add CanvasGroup component
            CanvasGroup panelCanvas = freeSpinsPanel.GetComponent<CanvasGroup>();
            if (panelCanvas == null)
            {
                panelCanvas = freeSpinsPanel.AddComponent<CanvasGroup>();
            }

            // Fade out animation
            float elapsed = 0;
            float duration = 0.5f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                panelCanvas.alpha = 1 - (elapsed / duration);
                yield return null;
            }

            // Finally hide the panel
            freeSpinsPanel.SetActive(false);
        }
    }

    private void EndFreeSpins()
    {
        isInFreeSpins = false;
        remainingFreeSpins = 0;
        waitingForFreeSpinsToStart = false;
        UpdateFreeSpinsUI();

        // Play end animation/sound
        StartCoroutine(PlayFreeSpinsEnd());
    }

    // Animation coroutines
    private IEnumerator PlayFreeSpinsActivation()
    {
        // Add your activation animation here
        Debug.Log("Free Spins Activated! 8 Free Spins Awarded!");

        // Enable the panel and text
        if (freeSpinsPanel != null)
        {
            freeSpinsPanel.SetActive(true);

            // Optional: Animate panel appearing
            CanvasGroup panelCanvas = freeSpinsPanel.GetComponent<CanvasGroup>();
            if (panelCanvas != null)
            {
                panelCanvas.alpha = 0;
                float elapsed = 0;
                while (elapsed < 1f)
                {
                    elapsed += Time.deltaTime;
                    panelCanvas.alpha = elapsed;
                    yield return null;
                }
            }
        }

        if (freeSpinsButton != null)
        {
            // Setup the button to start free spins when clicked
            freeSpinsButton.onClick.RemoveAllListeners();
            freeSpinsButton.onClick.AddListener(BeginFreeSpinsSequence);
            freeSpinsButton.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(2f);
    }

    private void BeginFreeSpinsSequence()
    {
        if (waitingForFreeSpinsToStart)
        {
            waitingForFreeSpinsToStart = false;

            StartCoroutine(HideFreeSpinsPanel());

            // Hide the start button
            if (freeSpinsButton != null)
            {
                freeSpinsButton.gameObject.SetActive(false);
            }

            // Start the first spin
            StartCoroutine(AutoSpinSequence());
        }
    }
    private IEnumerator AutoSpinSequence()
    {
        while (remainingFreeSpins > 0 && isInFreeSpins)
        {
            if (!isSpinning)
            {
                remainingFreeSpins--;
                UpdateFreeSpinsUI();

                // Start a spin
                isSpinning = true;
                StartCoroutine(SpinAllReels());

                // Wait for current spin to complete plus a delay
                yield return new WaitUntil(() => !isSpinning);
                yield return new WaitForSeconds(1f); // Delay between spins
            }
            yield return null;
        }

        // When spins are complete
        if (remainingFreeSpins <= 0)
        {
            EndFreeSpins();
        }
    }


    private IEnumerator PlayRetriggerAnimation()
    {
        // Add your retrigger animation here
        Debug.Log("Free Spins Retriggered! +5 Free Spins!");
        yield return new WaitForSeconds(1f);
    }
    private IEnumerator PlayFreeSpinsEnd()
    {
        // Add your end animation here
        Debug.Log("Free Spins Complete!");
        yield return new WaitForSeconds(2f);
    }

    //---------------End Free Spins--------------------

    private IEnumerator SpinAllReels()
    {
        Debug.Log("=== Starting New Spin ===");
        isSpinning = true;

        // Reset completion tracking
        spinningReels = 5; // We have 5 reels total
        for (int i = 0; i < 5; i++)
        {
            reelFinished[i] = false;
        }

        // Start spinning each reel with delay
        for (int reel = 0; reel < 5; reel++)
        {
            StartCoroutine(SpinReel(reel));
            yield return new WaitForSeconds(0.2f); // Delay between starting each reel
        }

        // Wait until ALL reels are finished
        while (spinningReels > 0)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Now that all reels are definitely finished, check for wins
        Debug.Log("All reels have finished spinning - Checking for wins");
        CheckWinningCombinations();
        CheckFreeSpinsSymbols();

        isSpinning = false;
    }

    private void CalculateTotalWeight()
    {
        totalWeight = 0;
        foreach(var symbol in symbols)
        {
            totalWeight += symbol.weight;
        }
    }

    private SymbolConfig GetRandomSymbol()
    {
        float random = Random.Range(0f, totalWeight);
        float weightSum = 0f;

        foreach(var symbol in symbols)
        {
            weightSum += symbol.weight;
            if(random <= weightSum)
            {
                return symbol;
            }
        }

        return symbols[symbols.Length - 1];
    }

    private void InitializeMatrix()
    {
        // Initialize the symbol matrix
        for (int reel = 0; reel < 5; reel++)
        {
            for (int row = 0; row < 3; row++)
            {
                // Get referecne to symbol image component
                symbolMatrix[row, reel] = reels[reel].GetChild(row).GetComponent<Image>();
            }
        }
    }

    private void InitializePaylines()
    {
        // Define all paylines
        paylines[0] = new GridPosition[] { new GridPosition(0,0), new GridPosition(0,1), new GridPosition(0,2), new GridPosition(0,3), new GridPosition(0,4) };
        paylines[1] = new GridPosition[] { new GridPosition(1, 0), new GridPosition(1, 1), new GridPosition(1, 2), new GridPosition(1, 3), new GridPosition(1, 4) };
        paylines[2] = new GridPosition[] { new GridPosition(2, 0), new GridPosition(2, 1), new GridPosition(2, 2), new GridPosition(2, 3), new GridPosition(2, 4) };
        paylines[3] = new GridPosition[] { new GridPosition(0, 0), new GridPosition(1, 1), new GridPosition(2, 2), new GridPosition(1, 3), new GridPosition(0, 4) };
        paylines[4] = new GridPosition[] { new GridPosition(2, 0), new GridPosition(1, 1), new GridPosition(0, 2), new GridPosition(1, 3), new GridPosition(2, 4) };
        paylines[5] = new GridPosition[] { new GridPosition(1, 0), new GridPosition(0, 1), new GridPosition(2, 2), new GridPosition(0, 3), new GridPosition(1, 4) };
        paylines[6] = new GridPosition[] { new GridPosition(1, 0), new GridPosition(2, 1), new GridPosition(0, 2), new GridPosition(2, 3), new GridPosition(1, 4) };
        paylines[7] = new GridPosition[] { new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(1, 2), new GridPosition(2, 3), new GridPosition(2, 4) };
        paylines[8] = new GridPosition[] { new GridPosition(2, 0), new GridPosition(2, 1), new GridPosition(1, 2), new GridPosition(0, 3), new GridPosition(0, 4) };
        paylines[9] = new GridPosition[] { new GridPosition(1, 0), new GridPosition(2, 1), new GridPosition(1, 2), new GridPosition(0, 3), new GridPosition(1, 4) };
        paylines[10] = new GridPosition[] { new GridPosition(1, 0), new GridPosition(0, 1), new GridPosition(1, 2), new GridPosition(2, 3), new GridPosition(1, 4) };
        paylines[11] = new GridPosition[] { new GridPosition(0, 0), new GridPosition(2, 1), new GridPosition(2, 2), new GridPosition(2, 3), new GridPosition(0, 4) };
        paylines[12] = new GridPosition[] { new GridPosition(2, 0), new GridPosition(0, 1), new GridPosition(0, 2), new GridPosition(0, 3), new GridPosition(2, 4) };
        paylines[13] = new GridPosition[] { new GridPosition(0, 0), new GridPosition(2, 1), new GridPosition(0, 2), new GridPosition(2, 3), new GridPosition(0, 4) };
        paylines[14] = new GridPosition[] { new GridPosition(2, 0), new GridPosition(0, 1), new GridPosition(2, 2), new GridPosition(0, 3), new GridPosition(2, 4) };
        paylines[15] = new GridPosition[] { new GridPosition(2, 0), new GridPosition(2, 1), new GridPosition(1, 2), new GridPosition(2, 3), new GridPosition(2, 4) };
        paylines[16] = new GridPosition[] { new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(1, 2), new GridPosition(0, 3), new GridPosition(0, 4) };
        paylines[17] = new GridPosition[] { new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(2, 2), new GridPosition(0, 3), new GridPosition(0, 4) };
        paylines[18] = new GridPosition[] { new GridPosition(2, 0), new GridPosition(2, 1), new GridPosition(0, 2), new GridPosition(2, 3), new GridPosition(2, 4) };
        paylines[19] = new GridPosition[] { new GridPosition(0, 0), new GridPosition(2, 1), new GridPosition(2, 2), new GridPosition(2, 3), new GridPosition(0, 4) };

    }

    private void SetuPaylineSprites()
    {
        // Create parent object for organization
        GameObject paylineParent = new GameObject("Payline_Images");
        paylineParent.transform.SetParent(transform);
        RectTransform parentRect = paylineParent.AddComponent<RectTransform>();

        for (int i = 0; i < paylines.Length; i++)
        {
            GridPosition[] currentPayline = paylines[i];

            // Create segments for each part of the payline
            for (int j = 0; j < currentPayline.Length - 1; j++)
            {
                // Create line segment
                Image lineSegment = Instantiate(paylinePrefab, parentRect);
                lineSegment.gameObject.name = $"Payline_{i}_Segment_{j}";

                // Get start and end positions for this segment
                RectTransform startSymbol = symbolMatrix[currentPayline[j].row, currentPayline[j].col].rectTransform;
                RectTransform endSymbol = symbolMatrix[currentPayline[j + 1].row, currentPayline[j + 1].col].rectTransform;

                // Position the line segment
                RectTransform segmentRect = lineSegment.rectTransform;
                Vector3 startPos = startSymbol.position;
                Vector3 endPos = endSymbol.position;

                // Calculate center point between start and end
                segmentRect.position = Vector3.Lerp(startPos, endPos, 0.5f);

                // Calculate length and rotation
                float distance = Vector3.Distance(startPos, endPos);
                segmentRect.sizeDelta = new Vector2(distance, 10f); // Adjust line thickness as needed

                // Calculate and set rotation
                Vector3 direction = endPos - startPos;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                segmentRect.rotation = Quaternion.Euler(0, 0, angle);

                // Start with line hidden
                lineSegment.enabled = false;

                // Store reference to this segment
                if (paylineImages[i] == null)
                {
                    paylineImages[i] = lineSegment;
                }
                else
                {
                    // If we already have a segment, make this one follow the same enable/disable state
                    int index = i;
                    Image firstSegment = paylineImages[i];
                    // Subscribe to enable/disable events of the first segment
                    StartCoroutine(LinkSegmentToFirst(firstSegment, lineSegment));
                }
            }
        }
    }

    private IEnumerator LinkSegmentToFirst(Image firstSegment, Image segment)
    {
        while (true)
        {
            segment.enabled = firstSegment.enabled;
            segment.color = firstSegment.color;
            yield return null;
        }
    }

    private void PopulateInitialSymbols()
    {
        for (int reel = 0; reel < 5; reel++)
        {
            for (int row = 0; row < 3; row++)
            {
                SymbolConfig randomSymbol = GetRandomSymbol();
                symbolMatrix[row, reel].sprite = randomSymbol.sprite;
            }
        }
    }

    public void StartSpin()
    {
        if (!isSpinning)
        {
            if (isInFreeSpins && remainingFreeSpins > 0)
            {
                remainingFreeSpins--;
                UpdateFreeSpinsUI();
            }
            else if (isInFreeSpins && remainingFreeSpins <= 0)
            {
                // End free spins mode
                EndFreeSpins();
                return;
            }

            isSpinning = true;
            StartCoroutine(SpinAllReels());
        }
    }

    private IEnumerator SpinReel(int reelIndex)
    {
        float elapsedTime = 0f;

        while (elapsedTime < spinDuration)
        {
            // Simulate spinning by changing symbols rapidly
            for (int row = 0; row < 3; row++)
            {
                SymbolConfig randomSymbol = GetRandomSymbol();
                symbolMatrix[row, reelIndex].sprite = randomSymbol.sprite;
            }

            elapsedTime += Time.deltaTime;
            yield return new WaitForSeconds(0.05f); // Control symbol change speed
        }

        // Set final symbols for this reel
        SetFinalReelSymbols(reelIndex);

        // Mark reel as finished
        reelFinished[reelIndex] = true;
        spinningReels--;
        Debug.Log($"Reel {reelIndex} finished. Remaining spinning reels: {spinningReels}");
    }

    private void SetFinalReelSymbols(int reelIndex)
    {
        // In a real slot game, you'd want to weight the probabilities
        // of different symbols appearing
        for (int row = 0; row < 3; row++)
        {
            SymbolConfig selectedSymbol = GetRandomSymbol();
            symbolMatrix[row, reelIndex].sprite = selectedSymbol.sprite;
        }
    }
    private void CheckWinningCombinations()
    {
        // verify done spinning
        if(spinningReels > 0)
        {
            Debug.LogError("Attempting to check wins while reels are still spinning!");
            return;
        }


        Debug.Log("--- Checking for winning combinations ---");
        for (int i = 0; i < paylines.Length; i++)
        {
            // Log the symbols in this payline for debugging
            string paylineSymbols = "Payline " + i + ": ";
            foreach (var pos in paylines[i])
            {
                SymbolConfig symbol = System.Array.Find(symbols,
                    s => s.sprite == symbolMatrix[pos.row, pos.col].sprite);
                paylineSymbols += symbol?.name + " > ";
            }
            Debug.Log(paylineSymbols);

            CheckPayline(i);
        }
    }

    private void CheckPayline(int paylineIndex)
    {
        GridPosition[] currentPayline = paylines[paylineIndex];

        // Get the first symbol and its configuration
        Sprite firstSymbol = symbolMatrix[currentPayline[0].row, currentPayline[0].col].sprite;
        SymbolConfig symbolConfig = System.Array.Find(symbols, s => s.sprite == firstSymbol);

        if (symbolConfig == null)
        {
            Debug.LogError($"Symbol configuration not found for sprite at position {currentPayline[0].row}, {currentPayline[0].col}");
            return;
        }

        int consecutiveMatches = 1; // Start with 1 for the first symbol
        bool checkingWildWin = symbolConfig.isWild;
        SymbolConfig targetSymbol = symbolConfig;

        // Check subsequent positions for matches
        for (int pos = 1; pos < currentPayline.Length; pos++)
        {
            GridPosition currentPos = currentPayline[pos];
            Sprite currentSymbolSprite = symbolMatrix[currentPos.row, currentPos.col].sprite;
            SymbolConfig currentSymbol = System.Array.Find(symbols, s => s.sprite == currentSymbolSprite);

            if (currentSymbol == null) continue;

            bool isMatch = false;
            if (checkingWildWin)
            {
                isMatch = currentSymbol.isWild;
            }
            else
            {
                // Regular symbol matching - count both exact matches and wilds
                isMatch = currentSymbolSprite == firstSymbol || currentSymbol.isWild;

                // If this is a wild, it can substitute for our target symbol
                if (currentSymbol.isWild)
                {
                    // Use the higher value between the wild and the symbol it's substituting for
                    if (currentSymbol.value > targetSymbol.value)
                    {
                        targetSymbol = currentSymbol;
                    }
                }
            }

            if (isMatch)
            {
                consecutiveMatches++;
            }
            else
            {
                // Stop counting when we find a non-matching symbol
                break;
            }
        }

        int requiredMatches = checkingWildWin ? 2 : targetSymbol.minMatchesRequired;

        // Check if we have enough matches based on the symbol's requirement
        if (consecutiveMatches >= requiredMatches)
        {
            Debug.Log($"Win on payline {paylineIndex} with {consecutiveMatches} matches of symbol {symbolConfig.name} (Wild Win: {checkingWildWin})");
            CheckFreeSpinsSymbols();
            StartCoroutine(HighlightWinningPayline(paylineIndex, consecutiveMatches, targetSymbol));

            // Calculate win amount based on the number of matches
            CalculateWinAmount(paylineIndex, consecutiveMatches, targetSymbol);
        }
    }

    private IEnumerator HighlightWinningPayline(int paylineIndex, int matchCount, SymbolConfig targetSymbol)
    {
        // Find all segments for this payline
    Image[] segments = GetPaylineSegments(paylineIndex);

    // Enable all segments
    foreach (var segment in segments)
            {
                segment.enabled = true;
            }

        float elapsed = 0f;
        float duration = 4f;  // Increased duration for better visibility

        while (elapsed < duration)
        {
            float alpha = Mathf.PingPong(elapsed * 2, 1f);
            Color color = Color.yellow;
            color.a = alpha;

            // Update all segments
            foreach (var segment in segments)
            {
                segment.color = color;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Disable all segments
        foreach (var segment in segments)
        {
            segment.enabled = false;
        }

        CalculateWinAmount(paylineIndex, matchCount, targetSymbol);
    }

    private Image[] GetPaylineSegments(int paylineIndex)
    {
        // Find all segments that belong to this payline
        return GameObject.FindObjectsOfType<Image>()
            .Where(img => img.gameObject.name.StartsWith($"Payline_{paylineIndex}_Segment"))
            .ToArray();
    }

    private void CalculateWinAmount(int paylineIndex, int matchCount, SymbolConfig winningSymbol)
    {
        if (winningSymbol != null)
        {
            float winAmount = winningSymbol.value * matchCount * currentBet;

            Debug.Log($"Win on payline {paylineIndex}:" +
                     $"\nSymbol: {winningSymbol.name}" +
                     $"\nMatches: {matchCount}" +
                     $"\nValue: {winningSymbol.value}" +
                     $"\nWin Amount: {winAmount} credits");

            // Here you would update the UI to show the win amount
        }
    }

}
