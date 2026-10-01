using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour, IControllable
{
    [SerializeField] private float _speed = 10f;
    [Header("Camera settings")]
    [SerializeField] private Transform _mainCamera;
    [SerializeField] private float _rotateSpeed = 10f;


    private CharacterController _controller;
    private Vector3 _moveDerection;
    private float _rotateDerection;

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
        _moveDerection = direction;
    }

    private void MoveInternal()
    {
        _controller.Move(_moveDerection * _speed * Time.fixedDeltaTime);
    }

    public void Rotate(float direction)
    {
        _rotateDerection = direction;
    }

    private void RotateInternal()
    {
        _mainCamera.RotateAround(transform.position, Vector3.up, _rotateSpeed * _rotateDerection * Time.fixedDeltaTime);
    }
}
