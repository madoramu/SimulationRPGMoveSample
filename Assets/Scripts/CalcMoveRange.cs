using UnityEngine;

/// <summary>
/// 移動力計算用クラス
/// </summary>
public class CalcMoveRange
{
    /// <summary> 移動可能範囲のマップ情報格納用 </summary>
    private int[,] _resultMoveRangeMaps;

    ///<summary> マップ情報 </summary>
    private MapInfo _mapInfo;

    public CalcMoveRange(MapInfo mapInfo)
    {
        _mapInfo = mapInfo;
    }

    /// <summary>
    /// 引数の移動開始地点と移動力から移動可能な範囲データを返す
    /// </summary>
    /// <param name="startPosition">初期マップ座標</param>
    /// <param name="movePower">移動力</param>
    /// <returns>移動可能範囲データが格納された2次元配列</returns>
    public int[,] GetMoveRangeList(Vector2Int startPosition, int movePower)
    {
        return GetMoveRangeList(startPosition.x, startPosition.y, movePower);
    }

    /// <summary>
    /// 引数の移動開始地点と移動力から移動可能な範囲データを返す
    /// </summary>
    /// <param name="positionX">マップX座標</param>
    /// <param name="positionY">マップY座標</param>
    /// <param name="movePower">移動力</param>
    /// <returns>移動可能範囲データが格納された2次元配列</returns>
    public int[,] GetMoveRangeList(int positionX, int positionY, int movePower)
    {
        _resultMoveRangeMaps = _mapInfo.MAP.Clone() as int[,];

        // 移動開始地点は必ず移動可能なので現在の移動力を代入
        movePower = Mathf.Max(0, movePower);
        _resultMoveRangeMaps[positionY, positionX] = movePower;

        // 移動力ある場合は検索
        if (movePower > 0)
        {
            SearchInFourDirections(positionX, positionY, movePower);
        }

        return _resultMoveRangeMaps;
    }

    /// <summary>
    /// 引数の位置から上下左右の4方向に対して移動可能か検索していく
    /// </summary>
    /// <param name="positionX">マップX座標</param>
    /// <param name="positionY">マップY座標</param>
    /// <param name="movePower">移動力</param>
    private void SearchInFourDirections(int positionX, int positionY, int movePower)
    {
        // 引数の座標がマップ外の場合は検索せず終了
        if (IsOutsideMap(positionX, positionY)) return;

        SearchMove(positionX, positionY - 1, movePower);
        SearchMove(positionX, positionY + 1, movePower);
        SearchMove(positionX - 1, positionY, movePower);
        SearchMove(positionX + 1, positionY, movePower);
    }

    /// <summary>
    /// 引数のマップ座標に対して引数の移動力で移動可能かどうかの検索
    /// </summary>
    /// <param name="positionX">マップX座標</param>
    /// <param name="positionY">マップY座標</param>
    /// <param name="movePower">移動力</param>
    private void SearchMove(int positionX, int positionY, int movePower)
    {
        // マップ領域内チェック
        if (IsOutsideMap(positionX, positionY)) return;

        movePower = movePower + _mapInfo.MAP[positionY, positionX];

        // 現在の移動力で上書きできそうな場合のみ上書きする
        if (movePower <= _resultMoveRangeMaps[positionY, positionX]) return;

        if (movePower >= 0)
        {
            _resultMoveRangeMaps[positionY, positionX] = movePower;
            if (movePower > 0)
            {
                SearchInFourDirections(positionX, positionY, movePower);
            }
        }
    }

    /// <summary>
    /// 引数の座標がマップ領域内かどうか
    /// </summary>
    /// <param name="positionX">マップX座標</param>
    /// <param name="positionY">マップY座標</param>
    /// <returns>領域外ならtrue, 領域内ならfalse</returns>
    private bool IsOutsideMap(int positionX, int positionY)
    {
        return positionX < 0 || positionX >= MapInfo.MAP_WIDTH || positionY < 0 || positionY >= MapInfo.MAP_HEIGHT;
    }
}
