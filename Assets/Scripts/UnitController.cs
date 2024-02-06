using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// マップ上のユニット
/// </summary>
public class UnitController : MonoBehaviour, IPointerClickHandler
{
    /// <summary>
    /// ユニットの状態
    /// </summary>
    public enum State
    {
        Wait,
        SelectMovePath,
        Moving
    }

    [SerializeField]
    private MapObject _mapObject;

    [SerializeField, ReadOnly]
    private State _state = State.Wait;

    private Vector2Int _currentMapPosition = new Vector2Int(3, 3);
    private int _movePower = 3;
    private CalcMoveRange _calcMoveRange = new CalcMoveRange(new MapInfo());
    private CalcMovePath _calcMovePath = new CalcMovePath(new MapInfo());
    private CalcAttackRange _calcAttackRange = new CalcAttackRange(new MapInfo());

    private int[,] _currentMoveRange;
    private IReadOnlyCollection<Vector2Int> _attackRangeMaps;
    private Vector2Int _currentMoveMapPosition;
    private List<(int mapY, int mapX, int movePower)> _currentMovePath;

    void Awake()
    {
        _mapObject.SetOnClickMapPanelCallback(OnClickMapPanel);
    }

    public void Update()
    {
        if (_state != State.SelectMovePath)
        {
            return;
        }

        // 移動経路選択時にEnterキーを押したらそのマスに移動する
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // 移動先が現在位置と同じ場合は即座に移動終了
            if (_currentMapPosition == _currentMoveMapPosition)
            {
                SetWaitState();
                return;
            }

            _state = State.Moving;
            List<Vector3> movePathPanels = _mapObject.GetMovePathPanel(_currentMovePath, transform.position.y);
            // リストの最初の要素は現在自分がいる座標のため、DOPathには不要なので削除しておく
            movePathPanels.RemoveAt(0);
            transform.DOPath(movePathPanels.ToArray(), movePathPanels.Count * 0.25f, PathType.CatmullRom, PathMode.Full3D, 10, Color.red)
                    .SetLookAt(0.1f)
                    .OnComplete(()=>
                    {
                        _currentMapPosition = _currentMoveMapPosition;
                        SetWaitState();
                    });
        }
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
                if (_state != State.Wait) return; 

                _state = State.SelectMovePath;

                // 移動可能範囲の検索
                _currentMoveRange = _calcMoveRange.GetMoveRangeList(_currentMapPosition.x, _currentMapPosition.y, _movePower);
                _mapObject.ShowMoveRangePanel(_currentMoveRange);

                // 攻撃可能範囲の検索
                _attackRangeMaps = _calcAttackRange.GetAttackRangeMaps((1, 1), _currentMoveRange);

                // 最初は現在の自分の位置を移動先に指定しておく
                _currentMoveMapPosition = _currentMapPosition;
                _currentMovePath = _calcMovePath.GetMovePath(_currentMoveMapPosition, _currentMapPosition, _currentMoveRange);
                _mapObject.ShowMovePathPanel(_currentMovePath);

                // 攻撃可能範囲の表示
                _mapObject.ShowAttackRangePanel(_attackRangeMaps);
                break;
            case -2:
                if (_state != State.SelectMovePath) return; 

                SetWaitState();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// ステートを待機状態にする
    /// </summary>
    private void SetWaitState()
    {
        _mapObject.HideMapPanel();
        _state = State.Wait;
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
        _currentMovePath = _calcMovePath.GetMovePath(_currentMoveMapPosition, _currentMapPosition, _currentMoveRange);
        _mapObject.ShowMovePathPanel(_currentMovePath);

        // 攻撃可能範囲マスがリセットされてしまうので再度ここで表示
        _mapObject.ShowAttackRangePanel(_attackRangeMaps);
    }
}
