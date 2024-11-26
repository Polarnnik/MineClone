using System.Collections.Generic;
using Godot;

namespace Mini.World;

public enum BlockFace
{
    Top,
    Front,
    Back,
    Right,
    Left,
    Bottom,
}

public partial class BlockData : Resource
{
    [Export] public string Name {get; set; }
    [Export] public Godot.Collections.Dictionary<BlockFace, Vector2> UVs { get; set; }
    
    public BlockData()
    {
        Name = "UknownBlock";
    }

    public BlockData(string name, Vector2 uvAll)
    {
        foreach (BlockFace face in System.Enum.GetValues(typeof(BlockFace)))
        {
            UVs = new Godot.Collections.Dictionary<BlockFace, Vector2>();
            UVs[face] = uvAll;
        }
    }
    
    public BlockData(string name, Vector2 uvTop, Vector2 uvFront, Vector2 uvBottom) 
    {
        UVs = new Godot.Collections.Dictionary<BlockFace, Vector2>();
        UVs[BlockFace.Top] = uvTop;
        UVs[BlockFace.Bottom] = uvBottom;

        UVs[BlockFace.Front] = uvFront;
        UVs[BlockFace.Back] = uvFront;
        UVs[BlockFace.Left] = uvFront;
        UVs[BlockFace.Right] = uvFront;
    }
    
    public BlockData(string name, Godot.Collections.Dictionary<BlockFace, Vector2> uvs)
    {
        UVs = new Godot.Collections.Dictionary<BlockFace, Vector2>(uvs);
    }
    
    public BlockData(string name)
    {
        Name = name;
    }
    
    public Vector2 GetUV(BlockFace face)
    {
        return UVs.TryGetValue(face, out var uv) ? uv : CollectionExtensions.GetValueOrDefault(UVs, BlockFace.Front);

    }
}