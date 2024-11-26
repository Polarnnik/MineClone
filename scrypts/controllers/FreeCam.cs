using Godot;
using System;

public partial class FreeCam : Camera3D
{
    [Export]
    public float MovementSpeed = 10.0f;
    
    [Export]
    public float FastSpeedMultiplier = 3.0f;
    
    [Export]
    public float MouseSensitivity = 0.003f;
    
    [Export]
    public float SmoothingSpeed = 15.0f;
    
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.Zero;
    private Vector2 mouseMotion = Vector2.Zero;
    private bool isCaptured = false;
    
    public override void _Ready()
    {
        targetPosition = Position;
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                isCaptured = mouseButton.Pressed;
                Input.MouseMode = isCaptured ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion && isCaptured)
        {
            this.mouseMotion = mouseMotion.Relative;
            // Применяем вращение камеры
            Vector3 rotationDegrees = RotationDegrees;
            rotationDegrees.Y -= mouseMotion.Relative.X * MouseSensitivity;
            rotationDegrees.X -= mouseMotion.Relative.Y * MouseSensitivity;
            rotationDegrees.X = Mathf.Clamp(rotationDegrees.X, -90, 90);
            RotationDegrees = rotationDegrees;
        }
    }
    
    public override void _PhysicsProcess(double delta)
    {
        ProcessMovement((float)delta);
    }
    
    private void ProcessMovement(float delta)
    {
        if (!isCaptured) return;
        
        Vector3 inputDir = GetMovementInput();
        float currentSpeed = MovementSpeed;
        
        // Ускорение при зажатом Shift
        if (Input.IsKeyPressed(Key.Shift))
        {
            currentSpeed *= FastSpeedMultiplier;
        }
        
        // Преобразуем input в локальные координаты камеры
        Vector3 forward = -Transform.Basis.Z;
        Vector3 right = Transform.Basis.X;
        Vector3 up = Vector3.Up;
        
        Vector3 direction = (forward * inputDir.Z + right * inputDir.X + up * inputDir.Y).Normalized();
        
        // Вычисляем целевую позицию
        targetPosition += direction * currentSpeed * delta;
        
        // Плавно перемещаем камеру к целевой позиции
        Position = Position.Lerp(targetPosition, SmoothingSpeed * delta);
    }
    
    private Vector3 GetMovementInput()
    {
        Vector3 inputDir = Vector3.Zero;
        
        // Вперед/назад
        if (Input.IsKeyPressed(Key.S))
            inputDir.Z -= 1;
        if (Input.IsKeyPressed(Key.W))
            inputDir.Z += 1;
            
        // Влево/вправо
        if (Input.IsKeyPressed(Key.A))
            inputDir.X -= 1;
        if (Input.IsKeyPressed(Key.D))
            inputDir.X += 1;
            
        // Вверх/вниз
        if (Input.IsKeyPressed(Key.Space))
            inputDir.Y += 1;
        if (Input.IsKeyPressed(Key.Shift))
            inputDir.Y -= 1;
            
        return inputDir.Normalized();
    }
}
