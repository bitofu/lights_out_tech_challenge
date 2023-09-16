using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EATechChallenge
{
  public class TestGame
  {
    [UnityTest]
    public IEnumerator GameComponentsExistsGivenWidthAndHeight()
    {
      SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
      yield return new WaitForSeconds(0.1f);
      Game game = GameObject.FindFirstObjectByType<Game>();
      CellManager[] cells = GameObject.FindObjectsByType<CellManager>(FindObjectsSortMode.None);

      Assert.NotNull(game);
      Assert.NotNull(game.WinCanvas);
      Assert.True(!game.WinCanvas.activeInHierarchy);
      Assert.AreEqual(Game.Width * Game.Height, cells.Length);
    }

    [UnityTest]
    public IEnumerator CellsCanFlipAndFlipAdjacentNeighbours()
    {
      SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
      yield return new WaitForSeconds(0.1f);
      CellManager[,] orderedCells = TestUtils.FindCellsOrdered();

      int xInput = 1;
      int yInput = 1;
      List<bool> previousStates = TestUtils.GetCellAndNeighbourLitState(orderedCells, xInput, yInput);
      orderedCells[xInput, yInput].OnClicked();
      List<bool> nextStates = TestUtils.GetCellAndNeighbourLitState(orderedCells, xInput, yInput);
      CollectionAssert.AreNotEqual(previousStates, nextStates);
    }

    [UnityTest]
    public IEnumerator GameIsWinnable()
    {
      SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
      yield return new WaitForSeconds(0.1f);
      Game game = GameObject.FindFirstObjectByType<Game>();
      CellManager[,] orderedCells = TestUtils.FindCellsOrdered();

      int totalCells = Game.Width * Game.Height;
      for (int i = 1; i <= totalCells; i++)
      {
        if (((1 << i) & game.PuzzleSolution) > 0)
        {
          (int x, int y) = Game.BitwiseIndexToCoordinates(i);
          orderedCells[x, y].OnClicked();
        }
      }

      Assert.True(game.WinCanvas.activeInHierarchy);
    }

    [UnityTest]
    public IEnumerator GameIsWinnableAfterRandomInputs()
    {
      SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
      yield return new WaitForSeconds(0.1f);
      Game game = GameObject.FindFirstObjectByType<Game>();
      CellManager[,] orderedCells = TestUtils.FindCellsOrdered();

      int totalCells = Game.Width * Game.Height;
      // Randomly flip cells
      for (int i = 1; i <= totalCells; i++)
      {
        int roll = UnityEngine.Random.Range(0, 2);
        if (roll > 0)
        {
          (int x, int y) = Game.BitwiseIndexToCoordinates(i);
          orderedCells[x, y].OnClicked();
        }
      }

      // Flip cells according to solution given state of the playing field
      for (int i = 1; i <= totalCells; i++)
      {
        if (((1 << i) & game.PuzzleSolution) > 0)
        {
          (int x, int y) = Game.BitwiseIndexToCoordinates(i);
          orderedCells[x, y].OnClicked();
        }
      }

      Assert.True(game.WinCanvas.activeInHierarchy);
    }
  }
}
