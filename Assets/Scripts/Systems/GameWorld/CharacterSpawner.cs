using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _character;
    public void SpawnCharacter(Terrain terrain)
    {
        Instantiate(_character, terrain.transform);
    }
}
