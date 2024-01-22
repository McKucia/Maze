using System.Collections.Generic;
using UnityEngine;

public class MazeGeneratorManager : MonoBehaviour
{
    public static MazeGeneratorManager Instance { get; private set; }

    [SerializeField]
    [Range(1, 10)]
    int _numLevels = 3;

    [SerializeField] 
    bool _generate;

    public bool isCircle = false;
    public GameObject tileRoomPrefab;
    public GameObject tileFloorPrefab;
    public GameObject tileWallPrefab;
    public GameObject tileBorderPrefab;
    public GameObject tileCarpetPrefab;

    [Range(0, 3)]   public int roomsPadding = 1;
    [Range(0, 3)]   public int roomExtraSize = 0;
    [Range(0, 100)] public int windingPercent = 60;
    [Range(0, 250)] public int numRoomTries = 50;
    [Range(21, 81)] public int gridSizeX = 41;
    [Range(21, 81)] public int gridSizeY = 41;

    [HideInInspector]
    public List<Vector2Int> directions = new List<Vector2Int>()
        {
            new Vector2Int(0, 1),  // up
            new Vector2Int(1, 0),  // right
            new Vector2Int(0, -1), // down
            new Vector2Int(-1, 0)  // left
        };

    List<MazeGenerator> _generators;
    int _currentLevel = 0;
    bool _generateCheck = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    void Start()
    {
        _generators = new List<MazeGenerator>();

        while (_currentLevel++ < _numLevels)
        {
            MazeGenerator generator = new MazeGenerator(_currentLevel);
            _generators.Add(generator);
        }
    }

    void OnValidate()
    {
        if (gridSizeX % 2 == 0) gridSizeX++;
        if (gridSizeY % 2 == 0) gridSizeY++;

        if (_generateCheck != _generate)
        {
            _generate = _generateCheck;
            _generators.ForEach(g => g.Generate());
        }
    }
}
