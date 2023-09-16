using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EATechChallenge
{
  public class TestUtils
  {
    public static List<bool> GetCellAndNeighbourLitState(CellManager[,] cells, int x, int y)
    {
      List<bool> states = new();
      for (int i = -1; i <= 1; i++)
      {
        int xCoord = x + i;
        if (xCoord >= 0 && xCoord < Game.Width)
        {
          states.Add(cells[xCoord, y].IsLit);
        }
      }
      for (int i = -1; i <= 1; i++)
      {
        if (i == 0)
        {
          continue;
        }

        int yCoord = y + i;
        if (yCoord >= 0 && yCoord < Game.Height)
        {
          states.Add(cells[yCoord, y].IsLit);
        }
      }
      return states;
    }

    // This would need reconsideration if play field is very large
    // FindObjectsByType would not be performant
    public static CellManager[,] FindCellsOrdered()
    {
      CellManager[] cells = GameObject.FindObjectsByType<CellManager>(FindObjectsSortMode.None);
      CellManager[,] orderedCells = new CellManager[Game.Width, Game.Height];

      foreach (CellManager cell in cells)
      {
        orderedCells[cell.x, cell.y] = cell;
      }
      return orderedCells;
    }
  }
}
