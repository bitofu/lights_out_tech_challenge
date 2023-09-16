using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace EATechChallenge
{
  /*
  My approach to the game is that at game start I will randomly select cells to be clicked,
  flipping it and its adjacent neighbours, the cells clicked will be recorded as the solution.
  The player simply needs to click on the cells that was intially clicked to return the field
  to a state where all cells are "dark". Wrong moves will also be recorded to the solution because
  this means that the wrongly clicked cell will now also need to be clicked to revert the mistake.
  */
  public class Game : MonoBehaviour
  {
    // This could be exposed for customizability of playing field but the win condition check
    // would need refactoring to support any grid larger than 5 x 6 or 6 x 5
    public const int Width = 5;
    public const int Height = 5;

    // 1. Going to use UGUI over UItoolkit because I find it faster to set up
    // 2. Furthermore it would be more similar to what is used by Design Home
    [Header("References")]
    [SerializeField] Canvas _gameCanvas = default;
    [SerializeField] CellManager _cellPrefab = default;
    [SerializeField] TextMeshProUGUI _playTimeClock = default;
    [SerializeField] TextMeshProUGUI _moveCountDisplay = default;

    // Exposed for tests
    [field:SerializeField]
    public GameObject WinCanvas { get; private set; } = default;

    [field:Header("Config")]
    [field:SerializeField]
    [field:Range(1, Width * Height)]
    public int MinPuzzleSteps { get; private set; } = 3;

    [field:SerializeField]
    [field:Range(1, Width * Height)]
    public int MaxPuzzleSteps { get; private set; } = 5;

    public float PlayTime { get; private set; }
    public int PuzzleSolution => _puzzleSolution;

    const float MoveCounterHeightOffset = 10f;
    const float PlayTimerHeightOffset = 50f;

    CellManager[,] _cells;
    int _moveCount;
    // Bitwise representation of activated cells to reduce time complexity when checking the win state
    // Time complexity is O(1) vs O(n), the alternative which would be a single loop to check all cells
    int _puzzleSolution;
    bool _isPlaying;

    void OnValidate()
    {
      MaxPuzzleSteps = Mathf.Max(MinPuzzleSteps, MaxPuzzleSteps);
    }

    void Start()
    {
      // Game space could be baked into the scene in editor to improve start time but I opted for
      // runtime instantiation to save myself some time hand placing objects in the scene.
      // Regardless of the above, I would loop over all cells to initialize them with their coordinates
      InitGameSpace(Width, Height, _gameCanvas, _cellPrefab);
      OnPlayAgain();
    }

    void Update()
    {
      // Record elapsed play time
      if (_isPlaying)
      {
        PlayTime += Time.deltaTime;
        // Use StringBuilder to save a little bit of CPU and memory
        StringBuilder playTimeMessage = new();
        playTimeMessage.AppendJoin(string.Empty,
          "Play Time: ",
          ((int)(PlayTime / 60f)).ToString("D2"),
          ':',
          (Mathf.FloorToInt(PlayTime) % 60).ToString("D2"));
        _playTimeClock.text = playTimeMessage.ToString();
      }
    }

    // Utility functions to centralize conversion to and from bitwise index
    // This has to match how the cells are initialized which is whole column by whole column
    #region UtilityFunctions
    public static int CoordinatesToBitwiseIndex(int x, int y)
    {
      return (x * Height) + y + 1;
    }

    public static (int, int) BitwiseIndexToCoordinates(int index)
    {
      int x = Mathf.FloorToInt((index - 1) / Height);
      int y = (index - 1) % Height;
      return (x, y);
    }
    #endregion

    void OnCellClicked(int x, int y)
    {
      FlipCellAndAdjacent(x, y);
      AddToMoveCounterAndUpdateHUD(1);
      CheckForWinState();
    }

    void FlipCellAndAdjacent(int x, int y)
    {
      // Flip horizontal neighbours
      for (int i = -1; i <= 1; i++)
      {
        int xCoord = x + i;
        if (xCoord >= 0 && xCoord < Width)
        {
          _cells[xCoord, y].Flip();
        }
      }
      // Flip vertical neighbours
      for (int i = -1; i <= 1; i++)
      {
        // Skip flipping the clicked cell because it would be flipped in the horizontal loop
        if (i == 0)
        {
          continue;
        }

        int yCoord = y + i;
        if (yCoord >= 0 && yCoord < Height)
        {
          _cells[x, yCoord].Flip();
        }
      }
      // XOR bitwise operator to flip the bit to the opposite value
      _puzzleSolution ^= 1 << CoordinatesToBitwiseIndex(x, y);
    }

    void AddToMoveCounterAndUpdateHUD(int value)
    {
      _moveCount += value;
      _moveCountDisplay.text = $"Moves: {_moveCount}";
    }

    void CheckForWinState()
    {
      // When no cells/bits are activated, ie 1, _puzzleSolution will be equal to 0 as an int
      if (_puzzleSolution == 0)
      {
        // Reveal the win canvas which has a background image to visually dim the game canvas
        // as well as block user inputs on the game canvas' cells
        WinCanvas.SetActive(true);
        // Stop the play timer
        _isPlaying = false;
      }
    }

    void OnPlayAgain()
    {
      // Reset all values to 0, reset the play timer, and re-init a random puzzle solution
      WinCanvas.SetActive(false);
      PlayTime = 0;
      _moveCount = 0;
      _isPlaying = true;
      AddToMoveCounterAndUpdateHUD(0);
      InitPuzzle(MinPuzzleSteps, MaxPuzzleSteps);
    }

    // Flip the cells randomly over 1 loop to keep time complexity to O(n) in the worse case
    void InitPuzzle(int minPuzzleSteps, int maxPuzzleSteps)
    {
      int totalCells = Width * Height;
      int bitsToFlip = Random.Range(minPuzzleSteps, maxPuzzleSteps + 1);
      // Offset the start index of cells flipped because the algorithm uses cumulative probabilty
      // and will favour the latter indices
      int bitwiseIndexOffset = Random.Range(0, totalCells);
      // Loop over each cell where i is the cell's bitwise Index and randomly flip it
      // to be a part of the puzzle solution
      for (int i = 1; i <= totalCells; i++)
      {
        float probabilityToFlip = (float)bitsToFlip / (totalCells + 1 - i);
        float roll = Random.Range(0f, 1f);
        if (roll <= probabilityToFlip)
        {
          // Ensure modulo result is always greater than 0
          int bitwiseIndex = ((i + bitwiseIndexOffset) % totalCells) + 1;
          (int x, int y) = BitwiseIndexToCoordinates(bitwiseIndex);
          FlipCellAndAdjacent(x, y);
          bitsToFlip--;
        }
        // Exit early if there are no more cells to flip for the puzzle
        if (bitsToFlip == 0)
        {
          break;
        }
      }
      // Debug.Log($"{Convert.ToString(_puzzleSolution, toBase: 2)}");
    }

    // May run too many iterations if min/max puzzle steps approaches number of cells
    // and time complexity could be greater than O(n)
    // void InitPuzzle(int minPuzzleSteps, int maxPuzzleSteps)
    // {
    //   int iterations = Random.Range(minPuzzleSteps, maxPuzzleSteps + 1);
    //   for (int i = 0; i < iterations; i++)
    //   {
    //     int solutionBitwiseIndex = UnityEngine.Random.Range(1, Width * Height + 1);
    //     if (((1 << solutionBitwiseIndex) & _puzzleSolution) > 0)
    //     {
    //       // Redo current step of puzzle solution generation
    //       i--;
    //       continue;
    //     }

    //     (int x, int y) = BitwiseIndexToCoordinates(solutionBitwiseIndex);
    //     // Debug.Log(solutionBitwiseIndex + " " + x + " " + y);
    //     FlipCell(x, y);
    //   }
    // }

    void InitGameSpace(int width, int height, Canvas gameCanvas, CellManager cellPrefab)
    {
      _cells = new CellManager[width, height];

      // Initialize clickable cells based on width and height
      RectTransform cellRectTransform = cellPrefab.GetComponent<RectTransform>();
      float cellWidth = cellRectTransform.sizeDelta.x;
      float cellHeight = cellRectTransform.sizeDelta.y;
      float heightOffset = (cellHeight * height) / 2f;
      Vector3 positionOffset = gameCanvas.transform.position
        - new Vector3((cellWidth * width) / 2f, heightOffset);
      for (int i = 0; i < width; i++)
      {
        for (int j = 0; j < height; j++)
        {
          Vector3 position = new Vector3(i * cellWidth, j * cellHeight, 0f) + positionOffset;
          CellManager cell = Instantiate(cellPrefab, position, Quaternion.identity, gameCanvas.transform);
          cell.Init(i, j);
          // Subscribe to cell's button's click event in order to flip the cell and neighbours
          // as well as keep track of progress towards the solution
          cell.OnClickedEvent += OnCellClicked;
          _cells[i, j] = cell;
        }
      }

      // Place timer and move counter to visible location
      _playTimeClock.transform.localPosition = new(0, heightOffset + PlayTimerHeightOffset, 0);
      _moveCountDisplay.transform.localPosition = new(0, heightOffset + MoveCounterHeightOffset, 0);

      // Connect play again button
      Button playAgainButton = WinCanvas.GetComponentInChildren<Button>();
      // Adding the listener via script over the editor to have more visibility in the code
      playAgainButton.onClick.AddListener(OnPlayAgain);
    }
  }
}
