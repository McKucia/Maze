using System;
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
    public GameObject virtualCamera;
    public GameObject tileRoomPrefab;
    public GameObject tileFloorPrefab;
    public GameObject tileWallPrefab;
    public GameObject tileBorderPrefab;
    public GameObject tileCarpetPrefab;
    public GameObject playerPrefab;
    [HideInInspector] public int _currentLevel = 0;

    [Range(0, 3)]   public int roomsPadding = 1;
    [Range(0, 3)]   public int roomExtraSize = 0;
    [Range(0, 100)] public int windingPercent = 60;
    [Range(0, 250)] public int numRoomTries = 50;
    [Range(21, 81)] public int gridSizeX = 41;
    [Range(21, 81)] public int gridSizeY = 41;

    MazeGenerator[] _generators;
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
        _generators = new MazeGenerator[_numLevels];

        int level = 0;
        while (level < _numLevels)
        {
            MazeGenerator generator = new MazeGenerator(level);
            _generators[level++] = generator;
        }
    }

    void OnValidate()
    {
        if (gridSizeX % 2 == 0) gridSizeX++;
        if (gridSizeY % 2 == 0) gridSizeY++;

        if (_generateCheck != _generate)
        {
            _generate = _generateCheck;

            foreach (var generator in _generators)
                generator.Generate();
        }
    }

    public Grid GetCurrentGrid()
    {
        return _generators[_currentLevel].Grid;
    }
}
