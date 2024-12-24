using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachine : MonoBehaviour
{
    // Each reel will be a collumn of 3 symbols
    public Transform[] reels = new Transform[5];
    // Store All possible symbols
    public Sprite[] symbolSprites;
    // Matrix to store current symbols (3 rows X 5 columns)
    private Image[,] symbolMatrix = new Image[3, 5];

    public Button spinButton;

    // Configuration
    public float spinDuration = 2f;
    public float spinSpeed = 10f;
    private bool isSpinning = false;

    private void Start()
    {
        // Initialize the symbol matrix
        for (int reel = 0; reel < 5; reel++)
        {
            for(int row = 0; row < 3; row++)
            {
                // Get referecne to symbol image component
                symbolMatrix[row,reel] = reels[reel].GetChild(row).GetComponent<Image>();
            }
        }
        spinButton.onClick.AddListener(StartSpin);
        // Set initiale random symbols
        PopulateInitialSymbols();
    }

    private void PopulateInitialSymbols()
    {
        for (int reel = 0; reel < 5; reel++)
        {
            for (int row = 0; row < 3; row++)
            {
                int randomSymbol = Random.Range(0, symbolSprites.Length);
                symbolMatrix[row, reel].sprite = symbolSprites[randomSymbol];
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
                int randomSymbol = Random.Range(0, symbolSprites.Length);
                symbolMatrix[row, reelIndex].sprite = symbolSprites[randomSymbol];
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
            int randomSymbol = Random.Range(0, symbolSprites.Length);
            symbolMatrix[row, reelIndex].sprite = symbolSprites[randomSymbol];
        }
    }

    private void CheckWinningCombinations()
    {
        // Start with checking horizontal lines
        for (int row = 0; row < 3; row++)
        {
            CheckHorizontalLine(row);
        }
    }

    private void CheckHorizontalLine(int row)
    {
        // Get the first symbol in the row as reference
        Sprite firstSymbol = symbolMatrix[row, 0].sprite;
        int matchCount = 1;

        // Check subsequent symbols in the row
        for (int reel = 1; reel < 5; reel++)
        {
            if (symbolMatrix[row, reel].sprite == firstSymbol)
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
            Debug.Log($"Win on row {row} with {matchCount} matches!");
            // Here you would trigger win animations and calculate payout
        }
    }

}
