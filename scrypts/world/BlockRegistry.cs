using Godot;
using Godot.Collections;
using System;

namespace Mini.World
{
	public partial class BlockRegistry : Node
	{
		[Export] public Dictionary<BlockType, BlockData> Blocks;
		[Export] public StandardMaterial3D BlockMaterial;
		[Export] public string BlockDataDirectory = "res://assets/blockBase/";
		[Export] public Vector2 BlockTextureSize = new Vector2(16, 16);

		public float TextureStep;
		public static BlockRegistry Instance { get; private set; }
		public Vector2 AtlasSize { get; private set; }

		public BlockRegistry()
		{
			Blocks = new Dictionary<BlockType, BlockData>();
		}
		
		public override void _Ready()
		{
			base._Ready();
			InitializeInstance();
			ValidateMaterialAndBlocks();
			InitializeAtlasSize();
			LoadUVs();
		}
		
		private void InitializeInstance()
		{
			if (Instance != null)
			{
				GD.PrintErr("Multiple instances of BlockRegistry detected.");
				return;
			}
			Instance = this;
		}

		private void ValidateMaterialAndBlocks()
		{
			if (BlockMaterial == null)
				GD.PrintErr("BlockMaterial is not assigned.");
			
			if (Blocks == null)
				GD.PrintErr("Blocks dictionary is null.");
		}

		private void InitializeAtlasSize()
		{
			if (BlockMaterial != null && BlockMaterial.AlbedoTexture is Texture2D texture)
			{
				AtlasSize = new Vector2(texture.GetWidth(), texture.GetHeight());
				TextureStep = AtlasSize.X / BlockTextureSize.X;
			}
			else
			{
				GD.PrintErr("No atlas given, impossible to generate UVs if there is none.");
			}
		}
		
		private void LoadUVs()
		{
			foreach (BlockType blockType in Enum.GetValues(typeof(BlockType)))
			{
				var blockDataPath = $"{BlockDataDirectory}{blockType}.tres";

				if (!ResourceLoader.Exists(blockDataPath))
				{
					var block = new BlockData(blockType.ToString());
					Blocks[blockType] = block;
					SaveBlockData(block, blockDataPath);
				}
				else
				{
					var blockData = ResourceLoader.Load<BlockData>(blockDataPath);
					Blocks[blockType] = blockData;
				}
			}
		}

		private void SaveBlockData(BlockData block, string path)
		{
			var result = ResourceSaver.Save(block, path);
			if (result != Error.Ok)
			{
				GD.PrintErr($"Failed to save BlockData to {path}: {result}");
			}
			else
			{
				GD.Print($"BlockData saved to {path}");
			}
		}
		
		public BlockData GetBlockData(BlockType type)
		{
			if (Blocks.TryGetValue(type, out var data))
				return data;

			throw new ArgumentException($"BlockType {type} not found in the registry.");
		}
	}
}
