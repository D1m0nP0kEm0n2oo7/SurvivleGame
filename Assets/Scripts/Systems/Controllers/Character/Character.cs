using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour, IControllable
{
    [SerializeField] private float _speed = 10f;
    [Header("Camera settings")]
    [SerializeField] private Transform _mainCamera;
    [SerializeField] private float _rotateSpeed = 10f;


    private CharacterController _controller;
    private Vector3 _moveDirection;
    private float _rotateDirection;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void FixedUpdate()
    {
        MoveInternal();
        RotateInternal();
    }

    public void Move(Vector3 direction)
    {
        _moveDirection = direction;
    }

    private void MoveInternal()
    {
        Vector3 cameraForward = _mainCamera.forward;
        Vector3 cameraRight = _mainCamera.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * _moveDirection.z + cameraRight * _moveDirection.x).normalized;


        _controller.Move(moveDirection * _speed * Time.fixedDeltaTime);
    }

    public void Rotate(float direction)
    {
        _rotateDirection = direction;
    }

    private void RotateInternal()
    {
        _mainCamera.RotateAround(transform.position, Vector3.up, _rotateSpeed * _rotateDirection * Time.fixedDeltaTime);
    }
}
