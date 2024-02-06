using System;
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

    [SerializeField, ReadOnly]
    private State _state = State.Wait;

    private Vector2Int _currentMapPosition = new Vector2Int(3, 3);
    private int _movePower = 3;
    private CalcMoveRange _calcMoveRange;
    private CalcMovePath _calcMovePath;
    private CalcAttackRange _calcAttackRange;

    private int[,] _currentMoveRange;
    private IReadOnlyCollection<Vector2Int> _attackRangeMaps;
    private Vector2Int _currentMoveMapPosition;
    private List<(int mapY, int mapX, int movePower)> _currentMovePath;

    private Action _onClickUnitCallback;
    private Action _changeWaitStateCallback;

    public State UnitState => _state;
    public int[,] CurrentMoveRange => _currentMoveRange;
    public IReadOnlyCollection<Vector2Int> AttackRangeMaps => _attackRangeMaps;
    public IReadOnlyCollection<(int mapY, int mapX, int movePower)> CurrentMovePath => _currentMovePath;

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="mapInfo"></param>
    /// <param name="onClickUnitCallback"></param>
    /// <param name="changeWaitStateCallback"></param>
    public void Initialize(MapInfo mapInfo, Action onClickUnitCallback, Action changeWaitStateCallback)
    {
        _calcMoveRange = new CalcMoveRange(mapInfo);
        _calcMovePath = new CalcMovePath(mapInfo);
        _calcAttackRange = new CalcAttackRange(mapInfo);

        _onClickUnitCallback = onClickUnitCallback;
        _changeWaitStateCallback = changeWaitStateCallback;
    }

    /// <summary>
    /// 引数の座標順で移動処理を開始する
    /// </summary>
    /// <param name="movePathPanels"></param>
    public void StartMoveMapPosition(Vector3[] movePathPanels)
    {
        // 移動先が現在位置と同じ場合は即座に移動終了
        if (_currentMapPosition == _currentMoveMapPosition)
        {
            SetWaitState();
            return;
        }

        _state = State.Moving;
        transform.DOPath(movePathPanels, movePathPanels.Length * 0.25f, PathType.CatmullRom, PathMode.Full3D, 10, Color.red)
                .SetLookAt(0.1f)
                .OnComplete(()=>
                {
                    _currentMapPosition = _currentMoveMapPosition;
                    SetWaitState();
                });
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

                // 攻撃可能範囲の検索
                _attackRangeMaps = _calcAttackRange.GetAttackRangeMaps((1, 1), _currentMoveRange);

                // 最初は現在の自分の位置を移動先に指定しておく
                _currentMoveMapPosition = _currentMapPosition;
                _currentMovePath = _calcMovePath.GetMovePath(_currentMoveMapPosition, _currentMapPosition, _currentMoveRange);

                _onClickUnitCallback();
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
        _state = State.Wait;
        _changeWaitStateCallback();
    }

    /// <summary>
    /// 引数の座標までの経路計算を行う
    /// </summary>
    /// <param name="mapX"></param>
    /// <param name="mapY"></param>
    /// <returns>正常に経路が作成できた場合はtrue, 指定場所までいけない or 既に引数の座標までの計算が済んでいる場合はfalse</returns>
    public bool CalcMovePathToArgPosition(int mapX, int mapY)
    {
        if (_currentMoveMapPosition.x == mapX && _currentMoveMapPosition.y == mapY) return false;

        // 引数の座標までの経路検索を行う
        _currentMoveMapPosition.x = mapX;
        _currentMoveMapPosition.y = mapY;
        List<(int mapY, int mapX, int movePower)> tmpList = _calcMovePath.GetMovePath(_currentMoveMapPosition, _currentMapPosition, _currentMoveRange);
        if (tmpList == null || tmpList.Count <= 0) return false;

        _currentMovePath = tmpList;
        return true;
    }
}
