using UnityEngine;
using UnityEngine.UI;

namespace EATechChallenge
{
  public class CellManager : MonoBehaviour
  {
    public delegate void CoordinatesEvent(int x, int y);

    public event CoordinatesEvent OnClickedEvent;

    #region ExposedForTests
    public int x => _x;
    public int y => _y;
    public bool IsLit => _isLit;
    #endregion

    Button _button;
    int _x;
    int _y;
    bool _isLit;

    public void Init(int x, int y)
    {
      _x = x;
      _y = y;
      _button = GetComponent<Button>();
      // Adding the listener via script over the editor to have more visibility in the code
      _button.onClick.AddListener(OnClicked);
    }

    public void Flip()
    {
      _isLit = !_isLit;

      ColorBlock colours = _button.colors;
      colours.normalColor = _isLit ? Color.yellow : Color.white;
      _button.colors = colours;
    }

    // Exposed for tests
    public void OnClicked() => OnClickedEvent?.Invoke(_x, _y);
  }
}
