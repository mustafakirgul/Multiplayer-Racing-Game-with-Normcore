using UnityEngine;

public enum Terrain
{
    sand,
    hard,
    nothing
}

public class TerrainType : MonoBehaviour
{
    public Terrain type = Terrain.hard;
}