using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 画面上にあるオブジェクト類全般をプレイヤーが操作するためのコントローラークラス
/// </summary>
public class PlayerController : MonoBehaviour
{
    ///<summary> マップ情報 </summary>
    private MapInfo _mapInfo;

    ///<summary> マップオブジェクト </summary>
    [SerializeField]
    private MapObject _mapObject;

    ///<summary> 操作ユニット </summary>
    [SerializeField]
    private UnitController _unitController;

    void Awake()
    {
        _mapInfo = new MapInfo();

        _mapObject.SetupMap(_mapInfo, OnClickMapPanel);

        _unitController.Initialize(_mapInfo, OnClickUnitControllerCallback, ChangeUnitControllerWaitStateCallback);
    }

    void Update()
    {
        if (_unitController.UnitState != UnitController.State.SelectMovePath)
        {
            return;
        }

        // 移動経路選択時に右クリックしたら待機状態にする
        if (Input.GetMouseButtonDown(1))
        {
            _unitController.SetWaitState();
        }
    }

    /// <summary>
    /// ユニットクリック時のコールバック
    /// </summary>
    private void OnClickUnitControllerCallback()
    {
        _mapObject.ShowMoveRangePanel(_unitController.CurrentMoveRange);
        _mapObject.ShowMovePathPanel(_unitController.CurrentMovePath);
        _mapObject.ShowAttackRangePanel(_unitController.AttackRangeMaps);
    }

    /// <summary>
    /// ユニットのステートが待機状態になった時のコールバック
    /// </summary>
    private void ChangeUnitControllerWaitStateCallback()
    {
        _mapObject.HideMapPanel();
    }

    /// <summary>
    /// 移動可能範囲表示時に移動可能範囲のパネルをクリックした時の処理
    /// </summary>
    /// <param name="mapX"></param>
    /// <param name="mapY"></param>
    private void OnClickMapPanel(int mapX, int mapY)
    {
        // 移動先と同じマスを再度クリックした場合、そのマスに移動する
        if (_unitController.HasMovePathToArgPosition(mapX, mapY))
        {
            List<Vector3> movePathPanels = _mapObject.GetMovePathPanel(_unitController.CurrentMovePath, _unitController.transform.position.y);
            // リストの最初の要素は現在自分がいる座標のため、DOPathには不要なので削除しておく
            movePathPanels.RemoveAt(0);
            _unitController.StartMoveMapPosition(movePathPanels.ToArray());
            return;
        }

        // 引数の位置に向かって経路作成を行う。異常値の場合は抜ける
        if (!_unitController.CalcMovePathToArgPosition(mapX, mapY))
        {
            return;
        }

        _mapObject.ShowMovePathPanel(_unitController.CurrentMovePath);
        // 攻撃可能範囲マスがリセットされてしまうので再度ここで表示
        _mapObject.ShowAttackRangePanel(_unitController.AttackRangeMaps);
    }
}
