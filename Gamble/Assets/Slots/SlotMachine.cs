using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SymbolConfig
{
    public Sprite sprite;
    public float weight;
    public float value;
    public string name;
    public int minMatchesRequired = 3;
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

#if UNITY_EDITOR
        TestSymbolDistribution(10000);
#endif
        CalculateTotalWeight();
        InitializeMatrix();
        InitializePaylines();
        SetuPaylineSprites();
        spinButton.onClick.AddListener(StartSpin);
        // Set initiale random symbols
        PopulateInitialSymbols();
    }

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
            // Instantiate payline image
            Image paylineImage = Instantiate(paylinePrefab, parentRect);
            paylineImage.gameObject.name = $"Payline_Image_{i}";

            // Set the image to cover the path between first and last symbol
            RectTransform imageRect = paylineImage.rectTransform;
            GridPosition startPos = paylines[i][0];
            GridPosition endPos = paylines[i][4];

            // Get positions of start and end symbols
            RectTransform startSymbol = symbolMatrix[startPos.row, startPos.col].rectTransform;
            RectTransform endSymbol = symbolMatrix[endPos.row, endPos.col].rectTransform;

            // Position and size the payline image
            imageRect.position = Vector3.Lerp(startSymbol.position, endSymbol.position, 0.5f);
            float distance = Vector3.Distance(startSymbol.position, endSymbol.position);
            imageRect.sizeDelta = new Vector2(distance, 10f); // Adjust height as needed

            // Calculate rotation to point from start to end
            Vector3 direction = endSymbol.position - startSymbol.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            imageRect.rotation = Quaternion.Euler(0, 0, angle);

            paylineImage.enabled = false;
            paylineImages[i] = paylineImage;
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

        // Check subsequent positions for matches
        for (int pos = 1; pos < currentPayline.Length; pos++)
        {
            GridPosition currentPos = currentPayline[pos];
            Sprite currentSymbol = symbolMatrix[currentPos.row, currentPos.col].sprite;

            if (currentSymbol == firstSymbol)
            {
                consecutiveMatches++;
            }
            else
            {
                // Stop counting when we find a non-matching symbol
                break;
            }
        }

        // Check if we have enough matches based on the symbol's requirement
        if (consecutiveMatches >= symbolConfig.minMatchesRequired)
        {
            Debug.Log($"Win on payline {paylineIndex} with {consecutiveMatches} matches of symbol {symbolConfig.name}");
            StartCoroutine(HighlightWinningPayline(paylineIndex, consecutiveMatches));

            // Calculate win amount based on the number of matches
            CalculateWinAmount(paylineIndex, consecutiveMatches);
        }
    }

    private IEnumerator HighlightWinningPayline(int paylineIndex, int matchCount)
    {
        Image paylineImage = paylineImages[paylineIndex];
        paylineImage.enabled = true;

        float elapsed = 0f;
        float duration = 2f;

        while (elapsed < duration)
        {
            float alpha = Mathf.PingPong(elapsed * 2, 1f);
            Color color = paylineImage.color;
            paylineImage.color = new Color(color.r, color.g, color.b, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        paylineImage.enabled = false;
        CalculateWinAmount(paylineIndex, matchCount);
    }

    private void CalculateWinAmount(int paylineIndex, int matchCount)
    {
        // Get the winning symbol from the first position in the payline
        GridPosition firstPos = paylines[paylineIndex][0];
        Sprite winningSprite = symbolMatrix[firstPos.row, firstPos.col].sprite;

        // Find the corresponding symbol configuration
        SymbolConfig winningSymbol = System.Array.Find(symbols, s => s.sprite == winningSprite);

        if (winningSymbol != null)
        {
            // Only calculate win if we meet the minimum match requirement
            if (matchCount >= winningSymbol.minMatchesRequired)
            {
                // Calculate win based on match count and current bet
                float winAmount = winningSymbol.value * matchCount * currentBet;

                Debug.Log($"Win on payline {paylineIndex}:" +
                         $"\nSymbol: {winningSymbol.name}" +
                         $"\nMatches: {matchCount}/{winningSymbol.minMatchesRequired} required" +
                         $"\nValue: {winningSymbol.value}" +
                         $"\nWin Amount: {winAmount} credits");

                // Here you would update the UI to show the win amount
            }
        }
    }

    // Debug method to test symbol distribution
    public void TestSymbolDistribution(int spins)
    {
        Dictionary<string, int> distribution = new Dictionary<string, int>();

        // Initialize counters
        foreach (var symbol in symbols)
        {
            distribution[symbol.name] = 0;
        }

        // Simulate spins
        for (int i = 0; i < spins; i++)
        {
            SymbolConfig symbol = GetRandomSymbol();
            distribution[symbol.name]++;
        }

        // Log results
        Debug.Log($"Distribution over {spins} spins:");
        foreach (var kvp in distribution)
        {
            float percentage = (float)kvp.Value / spins * 100f;
            Debug.Log($"{kvp.Key}: {percentage:F2}% ({kvp.Value} times)");
        }
    }

}
