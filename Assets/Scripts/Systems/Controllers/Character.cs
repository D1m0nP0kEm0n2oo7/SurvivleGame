using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour, IControllable
{
    [SerializeField] private float _speed = 10f;
     
    private CharacterController _controller;
    private Vector3 _moveDerection;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void FixedUpdate()
    {
        MoveInternal();
    }

    public void Move(Vector3 direction)
    {
        _moveDerection = direction;
    }
    private void MoveInternal()
    {
        _controller.Move(_moveDerection * _speed * Time.fixedDeltaTime);
    }
}
