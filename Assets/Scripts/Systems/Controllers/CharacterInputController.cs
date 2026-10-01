using UnityEngine;

public class CharacterInputController : MonoBehaviour
{
    private GameInput _gameInput;
    private IControllable _controllable;
    private ICameraControllable _cameraControllable;

    private void Awake()
    {
        _gameInput = new GameInput();
        _gameInput.Enable();

        _controllable = GetComponent<IControllable>();
        _cameraControllable = GetComponentInChildren<ICameraControllable>();

        if (_controllable == null)
        {
            Debug.Log("Компонент IControllable не найден");
        }

        if (_cameraControllable == null)
        {
            Debug.Log("Компонент ICameraControllable не найден");
        }
    }

    private void Update()
    {
        ReadMovement();
<<<<<<<< HEAD:Assets/Scripts/Systems/Controllers/Character/CharacterInputControler.cs
        ReadCameraRotation();
========
        ReadRotation();
>>>>>>>> new-branch:Assets/Scripts/Systems/Controllers/CharacterInputController.cs
    }

    private void ReadMovement()
    {
        var inputDirecton = _gameInput.Gameplay.Movement.ReadValue<Vector2>();
        var direction = new Vector3(inputDirecton.x, 0f, inputDirecton.y);

        _controllable.Move(direction);
    }

<<<<<<<< HEAD:Assets/Scripts/Systems/Controllers/Character/CharacterInputControler.cs
    private void ReadCameraRotation()
    {
        if (_cameraControllable == null) return;

        if (_gameInput.Gameplay.CameraActivate.IsPressed())
        {
            var lookInput = _gameInput.Gameplay.CameraLook.ReadValue<Vector2>();
            _cameraControllable.Rotate(lookInput);
        }
        else
        {
            _cameraControllable.Rotate(Vector2.zero);
        }
    }

========
    private void ReadRotation()
    {
        float inputDirection = _gameInput.Gameplay.CameraRotate.ReadValue<float>();

        _controllable.Rotate(inputDirection);
    }
>>>>>>>> new-branch:Assets/Scripts/Systems/Controllers/CharacterInputController.cs
}
