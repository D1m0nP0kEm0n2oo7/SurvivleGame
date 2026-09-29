using UnityEngine;

public class CameraController : MonoBehaviour, ICameraControllable
{
    [Header("Target")]
    [SerializeField] private Transform _targetObj;

    [Header("Settings")]
    [SerializeField] private float _distance = 5f; 
    [SerializeField] private float _followSpeed = 10f; 
    [SerializeField] private float _sensitivity = 0.1f;
    [SerializeField] private float _minPitch = -40f;
    [SerializeField] private float _maxPitch = 80f;

    private float _yaw = 0f;
    private float _pitch = 0f;
    private Vector2 _lookInput;

    private void Awake()
    {
        Vector3 angles = transform.eulerAngles;
        _yaw = angles.y;
        _pitch = angles.x;
    }

    public void Rotate(Vector2 direction)
    {
        _lookInput = direction;
    }

    private void LateUpdate()
    {
        // Применяем вращение на основе входных данных
        _yaw += _lookInput.x * _sensitivity;
        _pitch -= _lookInput.y * _sensitivity; // Инвертируем Y для привычного управления

        // Ограничиваем угол наклона, чтобы камера не переворачивалась
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

        // Вычисляем желаемую ротацию и позицию (сферические координаты)
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);
        Vector3 position = _targetObj.position - (rotation * Vector3.forward * _distance);

        // Плавно перемещаем камеру к целевой позиции
        transform.position = position;

        // Жестко задаем вращение, чтобы камера смотрела на цель
        transform.rotation = rotation;
    }
}

