using Godot;
using System;
using Mini.World;
public partial class Main : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var scene = GD.Load<PackedScene>("res://scenes/World/Chunk.tscn");
		Node chunk = scene.Instantiate();
		AddChild(chunk);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
