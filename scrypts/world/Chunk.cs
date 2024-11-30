using System;
using System.Collections.Generic;
using Godot;

namespace Mini.World;
[Tool]
public partial class Chunk : Node3D
{
	[Export] public FastNoiseLite Noise;
	//[Export] public ChunkManager chunkManager;
	public static Vector3I Dimensions = new Vector3I(32, 384, 32);
	
	private BlockType[] blocks;
	private MeshInstance3D meshInstance;
	private readonly SurfaceTool surfTool = new();

	private readonly List<Vector3> Vertices = new();
	private List<int> Triangles = new();
	private List<Vector2> UVs = new();
	
	public override void _Ready()
	{
		blocks = new BlockType[Dimensions.X * Dimensions.Y * Dimensions.Z];
		meshInstance = new MeshInstance3D();
		AddChild(meshInstance);
		
		GenerateTerrain();
		GenerateMesh();
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
	
	public void GenerateTerrain()
	{
		var maxHeight = Dimensions.Y / 4; // Controls terrain elevation range

		for (int x = 0; x < Dimensions.X; x++)
		{
			for (int z = 0; z < Dimensions.Z; z++)
			{
				var globalBlockPos = GetGlobalBlockPosition(new Vector3(x, 0, z));
				var groundHeight = (int)(maxHeight * ((Noise.GetNoise2D(globalBlockPos.X, globalBlockPos.Z) + 1) / 2f));

				for (int y = 0; y < groundHeight; y++)
				{
					if (y < groundHeight / 2)
						blocks[GetBlockIndex(x, y, z)] =  BlockType.Stone; 
					else if (y < groundHeight - 1)
						blocks[GetBlockIndex(x, y, z)] = BlockType.Dirt; 
					else
						blocks[GetBlockIndex(x, y, z)] = BlockType.Grass; 
				}

				// Skip the rest of the column as it will be air
				for (int y = groundHeight; y < Dimensions.Y; y++)
					blocks[GetBlockIndex(x, y, z)] = BlockType.Air;
			}
		}
	}


	public Vector3 GetGlobalBlockPosition(Vector3 localPosition)
	{
		return Position + localPosition;
	}
	
	public void GenerateMesh()
	{
		// +Z
		GreedyMesh(
			GenerateMask(Vector3I.Forward, out var blockTypes),
			blockTypes,
			Vector3I.Forward,
			Vector3I.Zero,
			Vector3I.Right,
			Vector3I.Up
		);

		FinalizeMesh();
	}

	private void GreedyMesh(bool[,] mask, BlockType[,] blockTypes, Vector3I normal, Vector3I startOffset, Vector3I rightOffset, Vector3I upOffset)
	{
		int width = mask.GetLength(0);
		int height = mask.GetLength(1);

		for (int x = 0; x < width;)
		{
			for (int y = 0; y < height;)
			{
				if (!mask[x, y])
				{
					y++;
					continue;
				}

				int quadWidth = 1;
				int quadHeight = 1;

				while (x + quadWidth < width && mask[x + quadWidth, y] && blockTypes[x + quadWidth, y] == blockTypes[x, y])
					quadWidth++;

				while (y + quadHeight < height)
				{
					bool validRow = true;
					for (int w = 0; w < quadWidth; w++)
					{
						if (!mask[x + w, y + quadHeight] || blockTypes[x + w, y + quadHeight] != blockTypes[x, y])
						{
							validRow = false;
							break;
						}
					}
					if (!validRow) break;
					quadHeight++;
				}

				Vector3 basePosition = startOffset + (rightOffset * x) + (upOffset * y);
				AddQuad(basePosition, quadWidth, quadHeight, blockTypes[x, y], normal);

				for (int dx = 0; dx < quadWidth; dx++)
				for (int dy = 0; dy < quadHeight; dy++)
					mask[x + dx, y + dy] = false;

				y += quadHeight;
			}
			x++;
		}
	}
	
	private bool[,] GenerateMask(Vector3I normal, out BlockType[,] blockTypes)
	{
		int width = (normal == Vector3I.Right || normal == Vector3I.Left) ? Dimensions.Z : Dimensions.X;
		int height = (normal == Vector3I.Up || normal == Vector3I.Down) ? Dimensions.Z : Dimensions.Y;
		int depth = (normal == Vector3I.Forward || normal == Vector3I.Back) ? Dimensions.Y : Dimensions.X;

		bool[,] mask = new bool[width, height];
		blockTypes = new BlockType[width, height];

		for (int w = 0; w < width; w++)
		{
			for (int h = 0; h < height; h++)
			{
				Vector3I blockPos = normal switch
				{
					Vector3I(1, 0, 0) => new Vector3I(depth - 1, h, w),
					Vector3I(-1, 0, 0) => new Vector3I(0, h, w),
					Vector3I(0, 1, 0) => new Vector3I(w, height - 1, h),
					Vector3I(0, -1, 0) => new Vector3I(w, 0, h),
					Vector3I(0, 0, 1) => new Vector3I(w, h, depth - 1),
					Vector3I(0, 0, -1) => new Vector3I(w, h, 0),
					_ => throw new ArgumentException("Invalid normal vector"),
				};

				var blockType = GetBlock(blockPos.X, blockPos.Y, blockPos.Z);
				var neighborPos = blockPos + normal;
				var neighborType = GetBlock(neighborPos.X, neighborPos.Y, neighborPos.Z);

				mask[w, h] = blockType != BlockType.Air && neighborType == BlockType.Air;
				blockTypes[w, h] = blockType;
			}
		}

		return mask;
	}

	private void AddQuad(Vector3 position, int width, int height, BlockType blockType, Vector3 normal)
	{
		// Adjust quad vertices based on the face's normal
		Vector3 up = normal == Vector3.Forward || normal == Vector3.Back ? Vector3.Up : normal == Vector3.Up || normal == Vector3.Down ? Vector3.Forward : Vector3.Up;
		Vector3 right = normal.Cross(up).Normalized();

		Vector3 p0 = position;
		Vector3 p1 = position + right * width;
		Vector3 p2 = position + up * height;
		Vector3 p3 = position + right * width + up * height;

		// Offset the entire quad based on the normal direction
		p0 += normal;
		p1 += normal;
		p2 += normal;
		p3 += normal;

		// Add vertices
		Vertices.Add(p0);
		Vertices.Add(p1);
		Vertices.Add(p2);
		Vertices.Add(p3);

		// Add triangles
		int vertexStart = Vertices.Count - 4;
		Triangles.Add(vertexStart);
		Triangles.Add(vertexStart + 2);
		Triangles.Add(vertexStart + 1);
		Triangles.Add(vertexStart + 1);
		Triangles.Add(vertexStart + 2);
		Triangles.Add(vertexStart + 3);

		// Add UVs (adjust based on texture atlas if needed)
		UVs.Add(Vector2.Zero);
		UVs.Add(Vector2.Right);
		UVs.Add(Vector2.Up);
		UVs.Add(Vector2.One);
	}
	
	private void FinalizeMesh()
	{
		surfTool.Begin(Mesh.PrimitiveType.Triangles);
		for (int i = 0; i < Vertices.Count; i++)
		{
			surfTool.AddVertex(Vertices[i]);
		}
		foreach (int index in Triangles)
			surfTool.AddIndex(index);

		meshInstance.Mesh = surfTool.Commit();
	}


	private bool IsVoxelSolid(Vector3I blockPosition)
	{
		return blockPosition.X >= 0 && blockPosition.X < Dimensions.X &&
			   blockPosition.Y >= 0 && blockPosition.Y < Dimensions.Y &&
			   blockPosition.Z >= 0 && blockPosition.Z < Dimensions.Z &&
			   blocks[GetBlockIndex(blockPosition.X, blockPosition.Y, blockPosition.Z)] != BlockType.Air;
	}
}
