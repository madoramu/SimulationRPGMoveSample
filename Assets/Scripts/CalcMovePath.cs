using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 移動経路計算クラス
/// </summary>
public class CalcMovePath
{
    ///<summary> 移動経路 </summary>
    private List<(int mapY, int mapX, int movePower)> _resultMovePath;

    ///<summary> 移動可能範囲 </summary>
    private int[,] _moveRange;

    ///<summary> 移動開始地点 </summary>
    private Vector2Int _startPosition;
    ///<summary> 移動開始地点の移動力 </summary>
    private int _startPositionMovePower;

    ///<summary> マップ情報 </summary>
    private MapInfo _mapInfo;

    public CalcMovePath(MapInfo mapInfo)
    {
        _mapInfo = mapInfo;
    }

    /// <summary>
    /// 引数の座標に向かうための移動経路座標リストを返す
    /// </summary>
    /// <param name="targetMapPosition">移動目標地点</param>
    /// <param name="moveRange">移動可能範囲</param>
    /// <param name="movePower">移動力</param>
    public List<(int mapY, int mapX, int movePower)> GetMovePath(Vector2Int targetMapPosition, Vector2Int startMapPosition, int[,] moveRange)
    {
        return GetMovePath(targetMapPosition.x, targetMapPosition.y, startMapPosition.x, startMapPosition.y, moveRange);
    }

    /// <summary>
    /// 引数の座標に向かうための移動経路座標リストを返す
    /// </summary>
    /// <param name="targetMapPositionX">移動目標マップX座標</param>
    /// <param name="targetMapPositionY">移動目標マップY座標</param>
    /// <param name="moveRange">移動可能範囲</param>
    /// <param name="movePower">移動力</param>
    public List<(int mapY, int mapX, int movePower)> GetMovePath(int targetMapPositionX, int targetMapPositionY, int startMapPositionX, int startMapPositionY, int[,] moveRange)
    {
        _startPosition = new Vector2Int(startMapPositionX, startMapPositionY);
        _startPositionMovePower = moveRange[_startPosition.y, _startPosition.x];

        // 移動先が現在地と同じ場合はそのまま現在地を渡して終了
        if (targetMapPositionX == _startPosition.x && targetMapPositionY == _startPosition.y)
        {
            _resultMovePath = new List<(int mapY, int mapX, int movePower)>(){ (startMapPositionY, startMapPositionX, _startPositionMovePower) };
            return _resultMovePath;
        }

        // 移動目標の座標は先に格納しておく
        _moveRange = moveRange;
        _resultMovePath = new List<(int mapY, int mapX, int movePower)>(_moveRange[startMapPositionY, startMapPositionX])
        {
            (targetMapPositionY, targetMapPositionX, _moveRange[targetMapPositionY, targetMapPositionX])
        };

        // 移動目標座標から移動開始座標までの経路を検索
        int movePower = _moveRange[targetMapPositionY, targetMapPositionX] - _mapInfo.MAP[targetMapPositionY, targetMapPositionX];
        SearchInFourDirections(targetMapPositionX, targetMapPositionY, movePower);

        // 逆順で格納されているため順番を開始座標～目標座標に直してから返す
        _resultMovePath.Reverse();
        return _resultMovePath;
    }

    /// <summary>
    /// 引数の座標と移動力から移動経路検索を行う
    /// </summary>
    /// <param name="positionX">マップX座標</param>
    /// <param name="positionY">マップY座標</param>
    /// <param name="movePower">座標到達時の移動力</param>
    private bool CheckSearchPath(int positionX, int positionY,  int movePower)
    {
        // 引数の座標がマップ外の場合は検索せず終了
        if (positionX < 0 || positionX >= MapInfo.MAP_WIDTH) return false;
        if (positionY < 0 || positionY >= MapInfo.MAP_HEIGHT) return false;

        // 引数の座標に登録されている移動力と、引数の移動力が一致していれば正しい移動経路としてみなす
        return _moveRange[positionY, positionX] == movePower;
    }

    /// <summary>
    /// 引数の座標の上下左右に対して、引数の移動力と一致する移動範囲マスの検索を行う
    /// </summary>
    /// <param name="positionX">マップX座標</param>
    /// <param name="positionY">マップY座標</param>
    /// <param name="movePower">座標到達時の移動力</param>
    private void SearchInFourDirections(int positionX, int positionY, int movePower)
    {
        // 引数の座標がマップ外の場合は検索せず終了
        if (positionX < 0 || positionX >= MapInfo.MAP_WIDTH) return;
        if (positionY < 0 || positionY >= MapInfo.MAP_HEIGHT) return;

        // 上
        if (CheckSearchPath(positionX, positionY - 1, movePower))
        {
            AddMovePath(positionX, positionY - 1, movePower);
            UpdateMovePower(positionX, positionY - 1, movePower);
            return;
        }

        // 下
        if (CheckSearchPath(positionX, positionY + 1, movePower))
        {
            AddMovePath(positionX, positionY + 1, movePower);
            UpdateMovePower(positionX, positionY + 1, movePower);
            return;
        }

        // 左
        if (CheckSearchPath(positionX - 1, positionY, movePower))
        {
            AddMovePath(positionX - 1, positionY, movePower);
            UpdateMovePower(positionX - 1, positionY, movePower);
            return;
        }

        // 右
        if (CheckSearchPath(positionX + 1, positionY, movePower))
        {
            AddMovePath(positionX + 1, positionY, movePower);
            UpdateMovePower(positionX + 1, positionY, movePower);
            return;
        }
    }

    /// <summary>
    /// 引数の座標と移動力を経路として追加する
    /// </summary>
    /// <param name="positionX">マップX座標</param>
    /// <param name="positionY">マップY座標</param>
    /// <param name="movePower">座標到達時の移動力</param>
    private void AddMovePath(int positionX, int positionY, int movePower)
    {
        _resultMovePath.Add((positionY, positionX, movePower));
    }

    /// <summary>
    /// 移動力の更新
    /// </summary>
    /// <param name="positionX">マップX座標</param>
    /// <param name="positionY">マップY座標</param>
    /// <param name="movePower">座標到達時の移動力</param>
    private void UpdateMovePower(int positionX, int positionY, int movePower)
    {
        // 現在の座標のマップ値だけ減算する(マップ値は基本的に負値なので加算となる)
        movePower = movePower - _mapInfo.MAP[positionY, positionX];

        // 移動力が移動開始地点と同じになったら経路が完成したので
        // 移動開始地点の座標を格納して終了
        if (movePower == _startPositionMovePower)
        {
            _resultMovePath.Add((_startPosition.y, _startPosition.x, movePower));
            return;
        }

        // 一致しない場合はまだ経路が完成していないのでまた上下左右で一致する移動力を検索する
        SearchInFourDirections(positionX, positionY, movePower);
    }
}
