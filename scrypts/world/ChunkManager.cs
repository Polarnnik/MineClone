using System.Collections.Generic;
using Godot;

namespace Mini.World;

public partial class ChunkManager : Node
{
	[Export] private FastNoiseLite Noise;
	[Export] public int RenderDistance = 10;
	private Dictionary<Vector2I, Chunk> loadedChunks = new();

	public override void _Ready()
	{
		base._Ready();
		
		Noise ??= new FastNoiseLite();
		GenerateChunk(new Vector2I(0, 0));
		//UpdateChunksAroundPosition(new Vector3(0, 0, 0));
	}

	public Chunk GenerateChunk(Vector2I position)
	{
		if (loadedChunks.TryGetValue(position, out Chunk existingChunk))
			return existingChunk;
		
		var chunk = new Chunk();
		chunk.Position = new Vector3(
			position.X * Chunk.Dimensions.X,
			0,
			position.Y * Chunk.Dimensions.Z);

		chunk.Noise = this.Noise;
		chunk.chunkManager = this;
		AddChild(chunk);
		loadedChunks.Add(position, chunk);
		return chunk;
	}
	
	public Vector2I GetChunkCoordinates(Vector3 globalPosition)
	{
		return new Vector2I(
			Mathf.FloorToInt(globalPosition.X / Chunk.Dimensions.X),
			Mathf.FloorToInt(globalPosition.Z / Chunk.Dimensions.Z)
		);
	}
	
	private void UnloadDistantChunks(Vector2I centerChunk)
	{
		var chunksToRemove = new List<Vector2I>();

		foreach (var kvp in loadedChunks)
		{
			if (Mathf.Abs(kvp.Key.X - centerChunk.X) > RenderDistance ||
				Mathf.Abs(kvp.Key.Y - centerChunk.Y) > RenderDistance)
			{
				chunksToRemove.Add(kvp.Key);
			}
		}

		foreach (var chunkCoord in chunksToRemove)
		{
			if (loadedChunks.TryGetValue(chunkCoord, out Chunk chunk))
			{
				chunk.QueueFree();
				loadedChunks.Remove(chunkCoord);
			}
		}
	}
	
	public Chunk GetChunkAtPosition(Vector3 globalPosition)
	{
		var chunkCoords = GetChunkCoordinates(globalPosition);
		return loadedChunks.TryGetValue(chunkCoords, out Chunk chunk) ? chunk : null;
	}
	
	public void UpdateChunksAroundPosition(Vector3 playerPosition)
	{
		var centerChunk = GetChunkCoordinates(playerPosition);

		for (int x = -RenderDistance; x <= RenderDistance; x++)
		{
			for (int z = -RenderDistance; z <= RenderDistance; z++)
			{
				var chunkCoords = centerChunk + new Vector2I(x, z);
				GenerateChunk(chunkCoords);
			}
		}

		UnloadDistantChunks(centerChunk);
	}
}
