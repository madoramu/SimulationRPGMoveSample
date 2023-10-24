using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// マップ上のユニット
/// </summary>
public class UnitController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private MapObject _mapObject;

    private Vector2Int _currentMapPosition = new Vector2Int(3, 3);
    private int _movePower = 3;
    private CalcMoveRange _calcMoveRange = new CalcMoveRange(new MapInfo());
    private CalcMovePath _calcMovePath = new CalcMovePath(new MapInfo());

    private int[,] _currentMoveRange;
    private Vector2Int _currentMoveMapPosition;

    // Start is called before the first frame update
    void Awake()
    {
        _mapObject.SetOnClickMapPanelCallback(OnClickMapPanel);
    }

    /// <summary>
    /// ユニットクリック時の処理
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.pointerId)
        {
            case -1:
                // 移動可能範囲の検索
                _currentMoveRange = _calcMoveRange.GetMoveRangeList(_currentMapPosition.x, _currentMapPosition.y, _movePower);
                _mapObject.ShowMoveRangePanel(_currentMoveRange);

                // 最初は現在の自分の位置を移動先に指定しておく
                _currentMoveMapPosition = _currentMapPosition;
                var movePath = _calcMovePath.GetMovePath(_currentMoveMapPosition, _currentMapPosition, _currentMoveRange);
                _mapObject.ShowMovePathPanel(movePath);
                break;
            case -2:
                _mapObject.HideMapPanel();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 移動可能範囲表示時に移動可能範囲のパネルをクリックした時の処理
    /// </summary>
    /// <param name="mapX"></param>
    /// <param name="mapY"></param>
    private void OnClickMapPanel(int mapX, int mapY)
    {
        if (_currentMoveMapPosition.x == mapX && _currentMoveMapPosition.y == mapY) return;

        // 引数の座標までの経路検索を行う
        _currentMoveMapPosition.x = mapX;
        _currentMoveMapPosition.y = mapY;
        var movePath = _calcMovePath.GetMovePath(_currentMoveMapPosition, _currentMapPosition, _currentMoveRange);
        _mapObject.ShowMovePathPanel(movePath);
    }
}
