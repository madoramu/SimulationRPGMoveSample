using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// マップオブジェクト
/// </summary>
public class MapObject : MonoBehaviour
{
    [SerializeField, Header("移動範囲表示用パネル")]
    private MapPanel _mapPanelObject;

    [SerializeField, Header("移動範囲マテリアル")]
    private Material _moveRangeMaterial;

    [SerializeField, Header("移動経路マテリアル")]
    private Material _movePathMaterial;

    [SerializeField, Header("攻撃可能範囲マテリアル")]
    private Material _attackRangeMaterial;

    [SerializeField]
    private Transform _mapObjectParent;

    [SerializeField]
    private GameObject _forestObject;

    [SerializeField]
    private GameObject _wallObject;

    private MapInfo _mapInfo;

    private Dictionary<(int mapY, int mapX), MapPanel> _mapPanelInfos;

    private Action<int, int> _onClickMapPanelCallback;

    /// <summary>
    /// マップのセットアップ
    /// </summary>
    /// <param name="mapInfo">マップ情報</param>
    /// <param name="mapInfo">マップのパネルをクリックしたときのコールバック</param>
    public void SetupMap(MapInfo mapInfo, Action<int, int> onClickMapPanelCallback)
    {
        _mapPanelObject.gameObject.SetActive(false);

        _mapInfo = mapInfo;
        _onClickMapPanelCallback = onClickMapPanelCallback;
        _mapPanelInfos = new Dictionary<(int mapX, int mapY), MapPanel>();

        // マップ情報をもとに各マスのオブジェクトを作成
        // マップ左上のマスは(-x, z)としておく
        Vector2Int leftUpPanelPosition = new Vector2Int(-MapInfo.MAP_WIDTH / 2, MapInfo.MAP_HEIGHT / 2);
        Vector2Int panelSize = Vector2Int.one;
        Vector3 instantiatePosition = _mapPanelObject.transform.localPosition;
        Quaternion instantiateQuaternion = _mapPanelObject.transform.localRotation;
        for (int y = 0; y < MapInfo.MAP_HEIGHT; y++)
        {
            for (int x = 0; x < MapInfo.MAP_WIDTH; x++)
            {
                // 左上が(-x, z)なので、そこに対してX座標は加算、Z座標は減算させてマスを埋めていく
                instantiatePosition.x = leftUpPanelPosition.x + panelSize.x * x;
                instantiatePosition.z = leftUpPanelPosition.y - panelSize.y * y;
                MapPanel mapPanel = Instantiate<MapPanel>(_mapPanelObject, instantiatePosition, instantiateQuaternion, transform);
                mapPanel.Initialize(x, y, _onClickMapPanelCallback);
                _mapPanelInfos.Add((y, x), mapPanel);

                // 森の場合はその地点にオブジェクトを生成
                if (_mapInfo.MAP[y, x] == -2)
                {
                    Instantiate(_forestObject, new Vector3(instantiatePosition.x, _forestObject.transform.position.y, instantiatePosition.z), Quaternion.identity, _mapObjectParent).SetActive(true);
                }
                // 壁の場合はその地点にオブジェクトを生成
                if (_mapInfo.MAP[y, x] == -10)
                {
                    Instantiate(_wallObject, new Vector3(instantiatePosition.x, _wallObject.transform.position.y, instantiatePosition.z), Quaternion.identity, _mapObjectParent).SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// 引数の移動可能範囲に該当するパネルをアクティブにする
    /// </summary>
    /// <param name="moveRange"></param>
    public void ShowMoveRangePanel(int[,] moveRange)
    {
        int moveRangeHeight = moveRange.GetLength(0);
        int moveRangeWidth = moveRange.GetLength(1);
        if (moveRangeHeight != MapInfo.MAP_HEIGHT || moveRangeWidth != MapInfo.MAP_WIDTH)
        {
            Debug.LogError($"移動可能範囲の配列要素数がマップと一致しません:縦{moveRangeHeight}, 横{moveRangeHeight}");
            return;
        }

        for (int y = 0; y < moveRangeHeight; y++)
        {
            for (int x = 0; x < moveRangeWidth; x++)
            {
                if (moveRange[y, x] >= 0)
                {
                    _mapPanelInfos[(y, x)].SetMaterial(_moveRangeMaterial);
                    _mapPanelInfos[(y, x)].SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// 引数の移動経路に該当するパネルをアクティブにしてパネルの色を変更する
    /// </summary>
    /// <param name="movePath"></param>
    public void ShowMovePathPanel(IReadOnlyCollection<(int mapY, int mapX, int movePower)> movePath)
    {
        // 一度移動可能範囲の全パネルのマテリアルをリセットする
        foreach (var mapPanel in _mapPanelInfos.Values)
        {
            if (mapPanel.ActiveSelf)
            {
                mapPanel.SetMaterial(_moveRangeMaterial);
            }
        }

        foreach (var pathValue in movePath)
        {
            _mapPanelInfos[(pathValue.mapY, pathValue.mapX)].SetMaterial(_movePathMaterial);
            _mapPanelInfos[(pathValue.mapY, pathValue.mapX)].SetActive(true);
        }
    }

    /// <summary>
    /// 攻撃可能範囲に該当するパネルをアクティブにしてパネルの色を変更する
    /// </summary>
    /// <param name="movePath"></param>
    public void ShowAttackRangePanel(IReadOnlyCollection<Vector2Int> attackRanges)
    {
        foreach (var attackRange in attackRanges)
        {
            _mapPanelInfos[(attackRange.y, attackRange.x)].SetMaterial(_attackRangeMaterial);
            _mapPanelInfos[(attackRange.y, attackRange.x)].SetActive(true);
        }
    }

    /// <summary>
    /// マップパネルを非表示にする
    /// </summary>
    public void HideMapPanel()
    {
        foreach (var mapPanel in _mapPanelInfos.Values)
        {
            mapPanel.SetMaterial(_moveRangeMaterial);
            mapPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 引数の移動経路の対象となるパネルの座標を返す
    /// </summary>
    /// <param name="movePath">移動経路</param>
    /// <returns></returns>
    public List<Vector3> GetMovePathPanel(IReadOnlyCollection<(int mapY, int mapX, int movePower)> movePath, float playerPositionY)
    {
        List<Vector3> movePathPanels = new List<Vector3>(movePath.Count);

        Vector3 tmp;
        foreach (var item in movePath)
        {
            tmp = _mapPanelInfos[(item.mapY, item.mapX)].Position;
            tmp.y = playerPositionY;
            movePathPanels.Add(tmp);
        }

        return movePathPanels;
    }
}
