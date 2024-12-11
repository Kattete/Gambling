using UnityEngine;

public class PlinkoBallGridGenerator : MonoBehaviour
{
    [Header("Grid Configuration")]
    public GameObject ballPrefab;
    public GameObject collectionBoxPrefab;
    public int rows = 10;
    public Color gridColor = Color.red;

    [Header("Spacing Parameters")]
    public float horizontalOffset = 1f;
    public float verticalOffset = 1f;
    public float staggerOffset = 0;
    public float topPadding = 1f;
    public float bottomYPosition = 2f;
    public float bottomBinSpacing = 1f;


    void Start()
    {
        GeneratePlinkoGrid();
    }

    void GeneratePlinkoGrid()
    {
        // Clear any existing grid
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        int maxBallsInRow = 3 + (rows - 1);
        float gridWidth = (maxBallsInRow -1) * horizontalOffset;
        float startX = -gridWidth / 2f;

        // Generate Plinko staggered grid
        for(int row = 0; row < rows; row++)
        {
            // Determine number of balls in this row
            int ballsInRow = 3 + row;

            // Calculate starting X position to center the row
            float rowStartX = startX + ((maxBallsInRow - ballsInRow) * horizontalOffset / 2f);

            for(int col = 0; col < ballsInRow; col++)
            {
                // Calculate position with horizontal and vertical offset
                Vector3 ballPosition = new Vector3(rowStartX + (col * horizontalOffset) + (row % 2 == 1 ? staggerOffset : 0), -row * verticalOffset - topPadding, 0);

                // Instantiate the ball
                GameObject newBall = Instantiate(ballPrefab, ballPosition, Quaternion.identity, transform);


                // Set color
                Renderer ballRenderer = newBall.GetComponent<Renderer>();
                if(ballRenderer != null)
                {
                    ballRenderer.material.color = gridColor;
                }
            }
        }
        GenerateCollectionBox(maxBallsInRow - 1);
    }

    void GenerateCollectionBox(int numBins)
    {
        float binSpacing = bottomBinSpacing;
        float startX = -(numBins) * binSpacing / 2f;
        float yOffset = -rows * 0.5f;
        for (int i = 0; i < numBins; i++)
        {
            Vector3 BinPosition = new Vector3(startX + (i * binSpacing), bottomYPosition + yOffset, 0);
            GameObject bin = Instantiate(collectionBoxPrefab, BinPosition, Quaternion.identity, transform);
        }
    }

    // Method to dynamically adjust grid parameters
    public void ConfigureGrid(int newRowCount)
    {
        rows = newRowCount;

        GeneratePlinkoGrid();
    }
}
