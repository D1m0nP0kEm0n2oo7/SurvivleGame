using UnityEngine;

public class CharacterInputControler : MonoBehaviour
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
        ReadCameraRotation();
    }

    private void ReadMovement()
    {
        var inputDirecton = _gameInput.Gameplay.Movement.ReadValue<Vector2>();
        var direction = new Vector3(inputDirecton.x, 0f, inputDirecton.y);

        _controllable.Move(direction);
    }

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

}
