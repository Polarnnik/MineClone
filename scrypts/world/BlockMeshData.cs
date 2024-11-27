using Godot;

namespace Mini.World;
public static class BlockMeshData
{

    public static readonly Vector3[] Verticies = new Vector3[]
    {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(1, 1, 0),
        new Vector3(0, 0, 1),
        new Vector3(1, 0, 1),
        new Vector3(0, 1, 1),
        new Vector3(1, 1, 1),
    };

    public static readonly int[] TopFace = new int[4] { 2, 3, 7, 6 };
    public static readonly int[] BottomFace = new int[4] { 0, 4, 5, 1 };
    public static readonly int[] LeftFace = new int[4] { 6, 4, 0, 2 };
    public static readonly int[] RightFace = new int[4] { 3, 1, 5, 7 };
    public static readonly int[] FrontFace = new int[4] { 7, 5, 4, 6 };
    public static readonly int[] BackFace = new int[4] { 2, 0, 1, 3 };

}