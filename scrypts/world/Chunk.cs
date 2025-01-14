using Godot;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Mini.World;
[Tool]
public partial class Chunk : Node3D
{
	[Export] public FastNoiseLite Noise;
	public static Vector3I Dimensions = new Vector3I(32, 384, 32);
	
	private BlockType[] blocks;
	private MeshInstance3D meshInstance;
	private readonly SurfaceTool surfTool = new();
	
	public override void _Ready()
	{
		blocks = new BlockType[Dimensions.X * Dimensions.Y * Dimensions.Z];
		meshInstance = new MeshInstance3D();
		AddChild(meshInstance);
		
		GenerateTerrain();
		SetBlock(0, 285, 0, BlockType.Dirt);
		UpdateMesh();
	}
	
	private int GetBlockIndex(int x, int y, int z)
	{
		return x + (z * Dimensions.X) + (y * Dimensions.X * Dimensions.Z);
	}
	
	public BlockType GetBlock(int x, int y, int z)
	{
		if (x < 0 || x >= Dimensions.X || y < 0 || y >= Dimensions.Y || z < 0 || z >= Dimensions.Z)
			return BlockType.Air;
		return blocks[GetBlockIndex(x, y, z)];
	}
	
	public void SetBlock(int x, int y, int z, BlockType blockType)
	{
		if (x < 0 || x >= Dimensions.X || y < 0 || y >= Dimensions.Y || z < 0 || z >= Dimensions.Z)
			return;
			
		blocks[GetBlockIndex(x, y, z)] = blockType;
	}
	
	private void GenerateTerrain()
	{
		var maxHeight = Dimensions.Y / 4; // Controls terrain elevation range

		for (int x = 0; x < Dimensions.X; x++)
		{
			for (int z = 0; z < Dimensions.Z; z++)
			{
				// Calculate global position for noise and terrain height
				var globalBlockPos = GetGlobalBlockPosition(new Vector3(x, 0, z));
				var groundHeight = (int)(maxHeight * ((Noise.GetNoise2D(globalBlockPos.X, globalBlockPos.Z) + 1) / 2f));

				for (int y = 0; y < groundHeight; y++)
				{
					if (y < groundHeight / 2)
						SetBlock(x, y, z, BlockType.Stone); // Lower half is stone
					else if (y < groundHeight - 1)
						SetBlock(x, y, z, BlockType.Dirt); // Upper layer is dirt
					else
						SetBlock(x, y, z, BlockType.Grass); // Topmost block is grass
				}

				// Skip the rest of the column as it will be air
				for (int y = groundHeight; y < Dimensions.Y; y++)
					SetBlock(x, y, z, BlockType.Air);
			}
		}
	}


	public Vector3 GetGlobalBlockPosition(Vector3 localPosition)
	{
		return Position + localPosition;
	}
	
	public void UpdateMesh()
	{ ;
		surfTool.Begin(Mesh.PrimitiveType.Triangles);
		
		for (int x = 0; x < Dimensions.X; x++)
		{
			for (int z = 0; z < Dimensions.Z; z++)
			{
				for (int y = 0; y < Dimensions.Y; y++)
				{
					var block = GetBlock(x, y, z);
					if (!(block == BlockType.Air))
						GenerateBlock(new Vector3I(x, y, z), block);
				}
			}
		}

		//surfTool.GenerateNormals();
		var mesh = surfTool.Commit();
		meshInstance.Mesh = mesh;
		meshInstance.Mesh.SurfaceSetMaterial(0, BlockRegistry.Instance.BlockMaterial);
		surfTool.Clear();
		
		meshInstance.CreateTrimeshCollision();
	}

	private void GenerateBlock(Vector3I blockPosition, BlockType blockType)
	{
		if (CheckEmpty(blockPosition + Vector3I.Up))
		{
			CreateFaceMesh(BlockMeshData.TopFace, blockPosition, BlockFace.Top, blockType);
		}
		if (CheckEmpty(blockPosition + Vector3I.Down))
		{
			CreateFaceMesh(BlockMeshData.BottomFace, blockPosition, BlockFace.Bottom, blockType);
		}
		if (CheckEmpty(blockPosition + Vector3I.Right))
		{
			CreateFaceMesh(BlockMeshData.RightFace, blockPosition, BlockFace.Right, blockType);
		}
		if (CheckEmpty(blockPosition + Vector3I.Left))
		{
			CreateFaceMesh(BlockMeshData.LeftFace, blockPosition, BlockFace.Left, blockType);
		}
		if (CheckEmpty(blockPosition + Vector3I.Forward))
		{
			CreateFaceMesh(BlockMeshData.FrontFace, blockPosition, BlockFace.Front, blockType);
		}
		if (CheckEmpty(blockPosition + Vector3I.Back))
		{
			CreateFaceMesh(BlockMeshData.BackFace, blockPosition, BlockFace.Back, blockType);
		}
	}

	private void CreateFaceMesh(int[] face, Vector3I blockPosition, BlockFace direction, BlockType type)
	{
		var data = BlockRegistry.Instance.GetBlockData(type);
		var uvs = data.GetUV(direction);
		
		var a = BlockMeshData.Verticies[face[0]] + blockPosition;
		var b = BlockMeshData.Verticies[face[1]] + blockPosition;
		var c = BlockMeshData.Verticies[face[2]] + blockPosition;
		var d = BlockMeshData.Verticies[face[3]] + blockPosition;
		
		var bUV = new Vector2(uvs.X + BlockRegistry.Instance.TextureStep, uvs.Y);
		var cUV = new Vector2(uvs.X, uvs.Y + BlockRegistry.Instance.TextureStep);
		var dUV =  new Vector2(uvs.X + BlockRegistry.Instance.TextureStep, uvs.Y + BlockRegistry.Instance.TextureStep);

		var triangleOne = new Vector3[] { a, b, c };
		var uvOne = new Vector2[] { bUV, cUV, uvs};
		var triangleTwo = new Vector3[] { a, c, d };
		var uvTwo = new Vector2[] { bUV, dUV, cUV };
		
		surfTool.AddTriangleFan(triangleOne, uvTwo);
		surfTool.AddTriangleFan(triangleTwo, uvOne);
		
	}

	private bool CheckEmpty(Vector3I blockPosition)
	{
		var block = GetBlock(blockPosition.X, blockPosition.Y, blockPosition.Z);
		return block == BlockType.Air;
	}
}
