/// <summary>
/// マップ情報
/// </summary>
public class MapInfo
{
    ///<summary> マップの横のマス目 </summary>
    public const int MAP_WIDTH = 7;
    ///<summary> マップの縦のマス目 </summary>
    public const int MAP_HEIGHT = 7;

    /// <summary>
    /// マップ情報
    /// </summary>
    public readonly int[,] MAP = new int[MAP_HEIGHT, MAP_WIDTH]
    {
        {-1, -1, -1, -1, -1, -1, -1},
        {-1, -1, -10, -10, -10, -1, -1},
        {-1, -1, -1, -1, -1, -1, -1},
        {-1, -1, -1, -1, -1, -1, -1},
        {-1, -2, -2, -1, -1, -1, -1},
        {-1, -2, -2, -1, -1, -1, -1},
        {-1, -1, -1, -1, -1, -1, -1}
    };
}
