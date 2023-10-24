using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// マップ上にある各マスのクラス
/// </summary>
public class MapPanel : MonoBehaviour, IPointerClickHandler
{
    private int _mapX;
    private int _mapY;
    private Action<int, int> _onClickCallback;

    public bool ActiveSelf => gameObject.activeSelf;

    public void Initialize(int mapX, int mapY, Action<int, int> onClickCallback)
    {
        _mapX = mapX;
        _mapY = mapY;
        _onClickCallback = onClickCallback;
    }

    /// <summary>
    /// 有効無効設定
    /// </summary>
    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    /// <summary>
    /// マテリアルの設定
    /// </summary>
    public void SetMaterial(Material material)
    {
        gameObject.GetComponent<Renderer>().material = material;
    }

    /// <summary>
    /// パネルクリック時の処理
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.pointerId)
        {
            case -1:
                _onClickCallback(_mapX, _mapY);
                break;
            default:
                break;
        }
    }
}
