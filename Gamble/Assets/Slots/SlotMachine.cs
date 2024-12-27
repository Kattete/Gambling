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

    // Store line renderers for visualizing paylines
    public LineRenderer[] paylineRenderers = new LineRenderer[20];

    // Add variables for win calculations
    private float currentBet = 100f;

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
        SetupPaylineRenderers();
        spinButton.onClick.AddListener(StartSpin);
        // Set initiale random symbols
        PopulateInitialSymbols();
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

    private void SetupPaylineRenderers()
    {
        for (int i = 0; i < paylines.Length; i++)
        {
            GameObject lineObj = new GameObject($"Payline_{i}");
            lineObj.transform.SetParent(transform);

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.yellow; // You can customize the color
            lr.endColor = Color.yellow;
            lr.enabled = false;

            // Convert grid positions to world space positions
            Vector3[] positions = new Vector3[5]; // Since each payline has 5 positions
            for (int j = 0; j < 5; j++)
            {
                // Get the RectTransform position of the symbol at this payline position
                GridPosition pos = paylines[i][j];
                RectTransform symbolRect = symbolMatrix[pos.row, pos.col].rectTransform;
                positions[j] = symbolRect.position;
            }

            lr.positionCount = positions.Length;
            lr.SetPositions(positions);

            paylineRenderers[i] = lr;
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

    private IEnumerator SpinAllReels()
    {
        // Spin each reel with a slight delay between them
        for (int reel = 0; reel < 5; reel++)
        {
            StartCoroutine(SpinReel(reel));
            yield return new WaitForSeconds(0.2f); // Delay between reels
        }

        yield return new WaitForSeconds(spinDuration);

        // Check for wins after all reels stop
        CheckWinningCombinations();
        isSpinning = false;
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
        // Check all paylines
        for (int i = 0; i < paylines.Length; i++)
        {
            CheckPayline(i);
        }
    }

    private void CheckPayline(int paylineIndex)
    {
        GridPosition[] currentPayline = paylines[paylineIndex];

        // Get the first symbol in the payline as reference
        Sprite referenceSymbol = symbolMatrix[currentPayline[0].row, currentPayline[0].col].sprite;
        int matchCount = 1;

        // Check subsequent positions
        for (int pos = 1; pos < currentPayline.Length; pos++)
        {
            GridPosition currentPos = currentPayline[pos];
            if (symbolMatrix[currentPos.row, currentPos.col].sprite == referenceSymbol)
            {
                matchCount++;
            }
            else
            {
                break; // Stop checking if we find a non-matching symbol
            }
        }

        if (matchCount >= 3)
        {
            // We have a win on this payline
            StartCoroutine(HighlightWinningPayline(paylineIndex, matchCount));
        }
    }

    private IEnumerator HighlightWinningPayline(int paylineIndex, int matchCount)
    {
        // Get the line renderer for this payline
        LineRenderer lineRenderer = paylineRenderers[paylineIndex];

        // Make the line visible
        lineRenderer.enabled = true;

        // Animate the line
        float elapsed = 0f;
        float duration = 2f;

        while (elapsed < duration)
        {
            // Create a pulsing effect
            float alpha = Mathf.PingPong(elapsed * 2, 1f);
            Color color = lineRenderer.startColor;
            lineRenderer.startColor = new Color(color.r, color.g, color.b, alpha);
            lineRenderer.endColor = new Color(color.r, color.g, color.b, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Hide the line after animation
        lineRenderer.enabled = false;

        // Calculate and display win amount
        CalculateWinAmount(paylineIndex, matchCount);
    }

    private void CalculateWinAmount(int paylineIndex, int matchCount)
    {
        // Get the winning symbol
        GridPosition firstPos = paylines[paylineIndex][0];
        Sprite winningSprite = symbolMatrix[firstPos.row, firstPos.col].sprite;

        SymbolConfig winninSymbol = System.Array.Find(symbols, s => s.sprite == winningSprite);

        if(winninSymbol != null)
        {
            // Calculate win based on match count and current bet
            float winAmount = winninSymbol.value * matchCount * currentBet;

            Debug.Log($"Win on payline {paylineIndex}: {winAmount} credits (Symbol: {winninSymbol.name}, Matches: {matchCount})" + $"Value: {winninSymbol.value}");
            // Here you would update the UI to show the win amount
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
