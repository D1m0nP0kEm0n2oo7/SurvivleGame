using UnityEngine;

public class CharacterInputController : MonoBehaviour
{
    private GameInput _gameInput;
    private IControllable _controllable;

    private void Awake()
    {
        _gameInput = new GameInput();
        _gameInput.Enable();

        _controllable = GetComponent<IControllable>();

        if (_controllable == null)
        {
            Debug.Log("Компонент IControllable не найден");
        }
    }

    private void Update()
    {
        ReadMovement();
        ReadRotation();
    }

    private void ReadMovement()
    {
        var inputDirecton = _gameInput.Gameplay.Movement.ReadValue<Vector2>();
        var direction = new Vector3(inputDirecton.x, 0f, inputDirecton.y);

        _controllable.Move(direction);
    }

    private void ReadRotation()
    {
        float inputDirection = _gameInput.Gameplay.CameraRotate.ReadValue<float>();

        _controllable.Rotate(inputDirection);
    }
}
