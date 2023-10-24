using System;
using System.Collections;
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

    private MapInfo _mapInfo;

    private Dictionary<(int mapY, int mapX), MapPanel> _mapPanelInfos;

    private Action<int, int> _onClickMapPanelCallback;

    // Start is called before the first frame update
    void Start()
    {
        _mapPanelObject.gameObject.SetActive(false);

        _mapInfo = new MapInfo();
        _mapPanelInfos = new Dictionary<(int mapX, int mapY), MapPanel>();

        // マップ情報をもとに各マスのオブジェクトを作成
        Vector2Int leftUpPanelPosition = new Vector2Int(-MapInfo.MAP_WIDTH / 2, -MapInfo.MAP_HEIGHT / 2);
        Vector2Int panelSize = Vector2Int.one;
        Vector3 instantiatePosition = _mapPanelObject.transform.localPosition;
        Quaternion instantiateQuaternion = _mapPanelObject.transform.localRotation;
        for (int y = 0; y < MapInfo.MAP_HEIGHT; y++)
        {
            for (int x = 0; x < MapInfo.MAP_WIDTH; x++)
            {
                instantiatePosition.x = leftUpPanelPosition.x + panelSize.x * x;
                instantiatePosition.z = leftUpPanelPosition.y + panelSize.y * y;
                MapPanel mapPanel = Instantiate<MapPanel>(_mapPanelObject, instantiatePosition, instantiateQuaternion, transform);
                mapPanel.Initialize(x, y, _onClickMapPanelCallback);
                _mapPanelInfos.Add((y, x), mapPanel);
            }
        }
    }

    /// <summary>
    /// マップのパネルをクリックしたときのコールバック登録
    /// </summary>
    /// <param name="callback"></param>
    public void SetOnClickMapPanelCallback(Action<int, int> callback)
    {
        _onClickMapPanelCallback = callback;
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
    public void ShowMovePathPanel(List<(int mapY, int mapX, int movePower)> movePath)
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
    /// マップパネルを非表示にする
    /// </summary>
    public void HideMapPanel()
    {
        foreach (var mapPanel in _mapPanelInfos.Values)
        {
            mapPanel.SetActive(false);
        }
    }
}
