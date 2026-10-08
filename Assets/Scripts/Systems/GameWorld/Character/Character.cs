using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour, IControllable
{
    [Header("Character settings")]
    [SerializeField] private Transform _characterArmature;
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _turnSpeed = 15f;
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
        RotateCharacterInternal();
    }

    public void Move(Vector3 direction)
    {
        _moveDirection = direction;
    }

    private void MoveInternal()
    {
        _controller.Move(DirectionCalc() * _speed * Time.fixedDeltaTime);
    }

    public void Rotate(float direction)
    {
        _rotateDirection = direction;
    }

    private void RotateInternal()
    {
        _mainCamera.RotateAround(transform.position, Vector3.up, _rotateSpeed * _rotateDirection * Time.fixedDeltaTime);
    }

    private void RotateCharacterInternal()
    {
        Vector3 moveDirection = DirectionCalc();
        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        _characterArmature.rotation = Quaternion.RotateTowards(
            _characterArmature.rotation,
            targetRotation,
            _turnSpeed * Time.fixedDeltaTime
        );

    }

    private Vector3 DirectionCalc()
    {
        Vector3 cameraForward = _mainCamera.forward;
        Vector3 cameraRight = _mainCamera.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        return (cameraForward * _moveDirection.z + cameraRight * _moveDirection.x).normalized;
    }
}
