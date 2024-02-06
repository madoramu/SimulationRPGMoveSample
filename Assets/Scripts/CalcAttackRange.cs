using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 攻撃範囲計算クラス
/// </summary>
public class CalcAttackRange
{
    ///<summary> マップ情報 </summary>
    private MapInfo _mapInfo;

    ///<summary> 移動可能範囲 </summary>
    private int[,] _moveRange;

    public CalcAttackRange(MapInfo mapInfo)
    {
        _mapInfo = mapInfo;
    }

    /// <summary>
    /// 攻撃射程と移動可能範囲から攻撃可能範囲マスリストを返す
    /// </summary>
    /// <param name="attackRangeDistance">攻撃射程</param>
    /// <param name="moveRange">移動可能範囲(マップ範囲と同じ長さ)</param>
    /// <returns></returns>
    public IReadOnlyCollection<Vector2Int> GetAttackRangeMaps((int min, int max) attackRangeDistance, int[,] moveRange)
    {
        List<Vector2Int> attackRangeMaps = new List<Vector2Int>();
        _moveRange = moveRange;

        int power = -1;
        for (int y = 0; y < MapInfo.MAP_HEIGHT; y++)
        {
            for (int x = 0; x < MapInfo.MAP_WIDTH; x++)
            {
                power = moveRange[y, x];
                if (power <= -1) continue;

                SearchInAttackRange(attackRangeDistance, new Vector2Int(x, y), attackRangeMaps);
            }
        }
        return attackRangeMaps;
    }

    /// <summary>
    /// 引数のマスから攻撃射程範囲内のマスを検索する
    /// </summary>
    /// <param name="attackRangeDistance">攻撃射程</param>
    /// <param name="searchPosition">検索開始位置</param>
    /// <param name="attackRangeMaps">格納する攻撃範囲リスト</param>
    private void SearchInAttackRange((int min, int max) attackRangeDistance, Vector2Int searchPosition, List<Vector2Int> attackRangeMaps)
    {
        int positionAbsDiff;
        Vector2Int currentPosition;
        for (int y = searchPosition.y + attackRangeDistance.max; y >= searchPosition.y - attackRangeDistance.max; y--)
        {
            for (int x = searchPosition.x + attackRangeDistance.max; x >= searchPosition.x - attackRangeDistance.max; x--)
            {
                // マップ外 or 移動可能マスの場合はスキップ
                if (IsOutsideMap(x, y) || IsMovablePosition(x, y)) continue;

                // 検索位置が攻撃射程内でまだ未格納の場合のみリストに格納する
                positionAbsDiff = Math.Abs(searchPosition.y - y) + Math.Abs(searchPosition.x - x);
                currentPosition = new Vector2Int(x, y);
                if (IsBetween(positionAbsDiff, attackRangeDistance.min, attackRangeDistance.max) && !attackRangeMaps.Contains(currentPosition))
                {
                    attackRangeMaps.Add(currentPosition);
                }
            }
        }
    }

    /// <summary>
    /// 移動可能マスか
    /// </summary>
    /// <param name="x">マップX座標</param>
    /// <param name="y">マップY座標</param>
    /// <returns>移動可能ならtrue, 移動不可ならfalse</returns>
    private bool IsMovablePosition(int x, int y)
    {
        return _moveRange[y, x] > -1;
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

    /// <summary>
    /// targetの値がminmaxの間にあるか
    /// </summary>
    private bool IsBetween(int target, int min, int max)
    {
        return target <= max && target >= min;
    }
}