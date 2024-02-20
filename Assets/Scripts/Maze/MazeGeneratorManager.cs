using Palmmedia.ReportGenerator.Core;
using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static Cinemachine.DocumentationSortingAttribute;

public class MazeGeneratorManager : MonoBehaviour
{
    public static MazeGeneratorManager Instance { get; private set; }

    public bool isCircle = false;
    public GameObject virtualCamera;
    public GameObject tileRoomPrefab;
    public GameObject tileFloorPrefab;
    public GameObject tileWallPrefab;
    public GameObject tileBorderPrefab;
    public GameObject tileCarpetPrefab;
    public GameObject tileExitPrefab;
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public NavMeshSurface surface;
    public int _currentLevel = 0;
    [HideInInspector] public bool IsReady { get { return _generators[_numLevels - 1].IsReady; } }

    [Range(0, 3)]   public int roomsPadding = 1;
    [Range(0, 3)]   public int roomExtraSize = 0;
    [Range(0, 100)] public int windingPercent = 60;
    [Range(0, 250)] public int numRoomTries = 50;
    [Range(21, 81)] public int gridSizeX = 41;
    [Range(21, 81)] public int gridSizeY = 41;

    List<MazeGenerator> _generators;
    bool _generateCheck = false;
    int _currentGeneratingLevel = 0;
    bool _generate = true;
    int _numLevels = 0;

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

        for (int i = 0; i < 2; i++)
            AddLevel();
    }

    void Update()
    {
        if (_generate)
        {
            _generators[_currentGeneratingLevel].Generate();
            _generate = false;
        }
        if (_generators[_currentGeneratingLevel].IsReady && _currentGeneratingLevel + 1 < _numLevels)
        {
            _currentGeneratingLevel++;
            _generate = true;
        }
    }

    // void OnValidate()
    // {
    //     if (gridSizeX % 2 == 0) gridSizeX++;
    //     if (gridSizeY % 2 == 0) gridSizeY++;
    // 
    //     if (_generateCheck != _generate)
    //     {
    //         _generate = _generateCheck;
    // 
    //         foreach (var generator in _generators)
    //             generator.Generate();
    //     }
    // }

    void AddLevel()
    {
        var generatorObject = new GameObject("Maze Level " + _numLevels);
        generatorObject.AddComponent<MazeGenerator>();
        generatorObject.transform.Translate(0, 100, 0);

        MazeGenerator mazeGenerator = generatorObject.GetComponent<MazeGenerator>();
        mazeGenerator.Level = _numLevels;
        _generators.Add(mazeGenerator);

        _numLevels++;
    }

    public Grid GetCurrentGrid()
    {
        return _generators[_currentLevel].Grid;
    }

    public void IncrementLevel()
    {
        _generators[_currentLevel].SetActive(false);
        _currentLevel++;
        _generators[_currentLevel].SetActive(true);
        AddLevel();
    }

    public void DecrementLevel()
    {
        _generators[_currentLevel].SetActive(true);
        _currentLevel--;
        _generators[_currentLevel].SetActive(false);
    }

    public void SetNextMazeGeneratorFloor(Tile tile)
    {
        if (_currentGeneratingLevel + 1 >= _numLevels) return;
        _generators[_currentGeneratingLevel + 1].SetFloor(tile);
    }

    public void DisplayMinimapTile(Tile tile)
    {
        if(_generators[_currentLevel].gameObject.activeSelf == true)
            _generators[_currentLevel].DisplayMinimapTile(tile);
    }
}
