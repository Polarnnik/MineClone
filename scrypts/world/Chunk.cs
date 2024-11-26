using Godot;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Mini.World;
[Tool]
public partial class Chunk : Node3D
{
	public static Vector3I Dimensions = new Vector3I(32, 384, 32);
	
	private BlockType[] blocks;
	private MeshInstance3D meshInstance;
	private readonly SurfaceTool surfTool = new();
	
	public Chunk()
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
		for (int x = 0; x < Dimensions.X; x++)
		{
			for (int z = 0; z < Dimensions.Z; z++)
			{
				for (int y = 0; y < Dimensions.Y; y++)
				{
					SetBlock(x, y, z, y < 50 ? BlockType.Dirt : BlockType.Air);
				}
			}
		}
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
					if (!(GetBlock(x, y, z) == BlockType.Air))
						GenerateBlock(new Vector3I(x, y, z));
				}
			}
		}

		surfTool.GenerateNormals();
		var mesh = surfTool.Commit();
		meshInstance.Mesh = mesh;
		surfTool.Clear();
		
		meshInstance.CreateTrimeshCollision();
	}

	private void GenerateBlock(Vector3I blockPosition)
	{
		if (CheckEmpty(blockPosition + Vector3I.Up))
		{
			CreateFaceMesh(BlockMeshData.TopFace, blockPosition);
		}
		if (CheckEmpty(blockPosition + Vector3I.Down))
		{
			CreateFaceMesh(BlockMeshData.BottomFace, blockPosition);
		}
		if (CheckEmpty(blockPosition + Vector3I.Right))
		{
			CreateFaceMesh(BlockMeshData.RightFace, blockPosition);
		}
		if (CheckEmpty(blockPosition + Vector3I.Left))
		{
			CreateFaceMesh(BlockMeshData.LeftFace, blockPosition);
		}
		if (CheckEmpty(blockPosition + Vector3I.Forward))
		{
			CreateFaceMesh(BlockMeshData.FrontFace, blockPosition);
		}
		if (CheckEmpty(blockPosition + Vector3I.Back))
		{
			CreateFaceMesh(BlockMeshData.BackFace, blockPosition);
		}
	}

	private void CreateFaceMesh(int[] face, Vector3I blockPosition)
	{
		var a = BlockMeshData.Verticies[face[0]] + blockPosition;
		var b = BlockMeshData.Verticies[face[1]] + blockPosition;
		var c = BlockMeshData.Verticies[face[2]] + blockPosition;
		var d = BlockMeshData.Verticies[face[3]] + blockPosition;

		var triangleOne = new Vector3[] { a, b, c };
		var triangleTwo = new Vector3[] { a, c, d };
		
		surfTool.AddTriangleFan(triangleOne);
		surfTool.AddTriangleFan(triangleTwo);
		
	}

	private bool CheckEmpty(Vector3I blockPosition)
	{
		if (GetBlock(blockPosition.X, blockPosition.Y, blockPosition.Z) == BlockType.Air)
		{
			return true;
		}
		return false;
	}
}
