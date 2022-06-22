using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using Unity.VisualScripting;


public class ConstraintSolver : MonoBehaviour
{

    #region Serialized fields
    [SerializeField]
    public Vector3Int GridDimensions;
    [SerializeField]
    public Vector3 TileSize = new Vector3(12, 4, 12);


    #endregion

    #region public fields

    public GameObject[] GOPatternPrefabs;

    #endregion

    #region private fields
    public Tile[,,] TileGrid { private set; get; }
    List<TilePattern> _patternLibrary;
    List<Connection> _connections;

    public Vector3Int Index { get; private set; }


    private TilePattern _mat_ConPink;    //00
    private TilePattern _mat_ConYellow;  //01
    private TilePattern _mat_ConBlue;    //02
    private TilePattern _mat_Orange;     //03
    private TilePattern _mat_Cyan;       //04
    private TilePattern _mat_Green;      //05
    private TilePattern _mat_Stair;      //05

    private IEnumerator _propogateStep;  //changed from private to public
    private bool _isCollapsing = false;


    #endregion

    #region Unity Standard Methods

    void Start()
    {
        GOPatternPrefabs = new GameObject[]
        {
            //Mansion Block Prefabs
            Resources.Load<GameObject>("Prefabs/PrefabPatternA"),
            Resources.Load<GameObject>("Prefabs/PrefabPatternB"),
            Resources.Load<GameObject>("Prefabs/PrefabPatternC"),
            Resources.Load<GameObject>("Prefabs/PrefabPatternD"),
            Resources.Load<GameObject>("Prefabs/PrefabPatternStair"),
            Resources.Load<GameObject>("Prefabs/PrefabPatternStairTop"),
            Resources.Load<GameObject>("Prefabs/PrefabPatternF"),
            Resources.Load<GameObject>("Prefabs/PrefabPatternG"),
            Resources.Load<GameObject>("Prefabs/PrefabPatternH"),
            Resources.Load<GameObject>("Prefabs/PrefabPatternI"),
            Resources.Load<GameObject>("Prefabs/PrefabPatternJ"),
            Resources.Load<GameObject>("Prefabs/PrefabPatternK"),
            Resources.Load<GameObject>("Prefabs/PrefabPatternL")
            //Resources.Load<GameObject>("Prefabs/PrefabPatternM")


            //Highway In the Sky Prefabs
            //Resources.Load<GameObject>("Prefabs/PrefabPatternA2"),
            //Resources.Load<GameObject>("Prefabs/PrefabPatternB2"),
            //Resources.Load<GameObject>("Prefabs/PrefabPatternC2"),
            //Resources.Load<GameObject>("Prefabs/PrefabPatternD2"),
            //Resources.Load<GameObject>("Prefabs/PrefabPatternE2")


            //Resources.Load<GameObject>("Prefabs/PrefabPatternZ") is meant to be used for the facade

            //* summary of facade creation
            //[06/06/2022 14:14] Doria, David
            //components/stones voxelised, so that you can identify internal and façade
            //sections of your structure based on neighbouring status of the voxels.From there,
            //you can use another WFC or even a brute force strategy
            //That would require you to voxelise the structure, which wouldn't be a big problem,
            //but there's another way.For each chunk that you aggregate via the WFC, you have an option of façade and glazing solution
            //that you only activate if the component does not have a neighbour in that direction.
            //E.g.each connection also have a layer of façade pre-modelled and after you aggregate everything,
            //you which chunks are on the periphery of the aggregation and 'activate' their façade
            //notes from David

        };
        //Add all connections
        _connections = new List<Connection>();

        _connections.Add(new Connection(ConnectionType.conPink, "conPink"));          //00
        _connections.Add(new Connection(ConnectionType.conYellow, "conYellow"));      //01
        _connections.Add(new Connection(ConnectionType.conBlue, "conBlue"));          //02
        _connections.Add(new Connection(ConnectionType.conOrange, "conOrange"));      //03
        _connections.Add(new Connection(ConnectionType.conCyan, "conCyan"));          //04
        _connections.Add(new Connection(ConnectionType.conGreen, "conGreen"));        //05
        _connections.Add(new Connection(ConnectionType.conStair, "conStair"));        //06

        //Add all patterns
        _patternLibrary = new List<TilePattern>();
        for (int i = 0; i < GOPatternPrefabs.Length; i++)
        {
            var goPattern = GOPatternPrefabs[i];
            _patternLibrary.Add(new TilePattern(i, goPattern, _connections));
        }

        //Set up the tile grid
        MakeTiles();
        // add a random tile to a random position
        SetStairTile();

        GetNextTile();

        //look into making this into a bounding box
       _propogateStep = PropogateStep();
    }


    public void SetStairTile()
    {
        int rndX = Random.Range(0, GridDimensions.x);
        int rndZ = Random.Range(0, GridDimensions.z);

        TileGrid[rndX, 0, rndZ].AssignPattern(_patternLibrary[4]);
    }

    public void GetPlan()
    {

        PlanCreation.CreatePlanFromTiles(GetTilesFlattened());
    }

    //Buttons, Unity Buttons on Canvas are not compadible with script
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 200, 50), "Collapse Me"))
        {
            if (!_isCollapsing)
                StartCoroutine(_propogateStep);
            else
            {
                StopCoroutine(_propogateStep);
                _isCollapsing = false;
            }


        }
        //if (GUI.Button(new Rect(10, 100, 200, 50), "Hide Me"))       //Only needed for troubleshooting connections
        //{
        //  VoidVisability();
        //}

        //if (GUI.Button(new Rect(10, 190, 200, 50), "Plan Me"))   //Trying to phase out this button method
        //{
        //    GetPlan();

       // }
   
    }
    #endregion

    #region public functions

    public void StartStopCollapsing()
    {
        if (!_isCollapsing)
            StartCoroutine(_propogateStep);
        else
        {
            StopCoroutine(_propogateStep);
            _isCollapsing = false;
        }
    }

    #endregion

    #region private functions

    //Create the tile grid
    private void MakeTiles()
    {
        TileGrid = new Tile[GridDimensions.x, GridDimensions.y, GridDimensions.z];
        for (int x = 0; x < GridDimensions.x; x++)
        {
            for (int y = 0; y < GridDimensions.y; y++)
            {
                for (int z = 0; z < GridDimensions.z; z++)
                {
                    TileGrid[x, y, z] = new Tile(new Vector3Int(x, y, z), _patternLibrary, this, TileSize);
                }
            }
        }
    }


    private IEnumerator PropogateStep() //changed from private to public
    {
        while (true)
        {
            _isCollapsing = true;
            GetNextTile();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void GetNextTile()
    {
        // <summary>
        // get all unset tiles -> tiles that have no tile pattern assinged (tile.CurrentPattern)
        // for each of the unset, get the PossibleConnections
        // sort your unset tiles by the length of possible connection -> 0 == smallest lenght
        // get index 0 from the unset
        // do tile.AssingPattern() and assign a random tile pattern from its PossibleConnections
        // OUTSIDE THIS METHOD: Reapeat until no more tiles are left unset
        // <summary>

        List<Tile> UnsetTiles = GetUnsetTiles();

        //Check if there still are tiles to set

        if (UnsetTiles.Count == 0)
        {
            Debug.Log("all tiles are set");
            return;
        }

        //this is currently not going to give you the lowest tile
        List<Tile> lowestTiles = new List<Tile>();
        int lowestTile = int.MaxValue;

        //PropogateGrid on the set tile                     

        foreach (Tile tile in UnsetTiles)
        {
            if (tile.NumberOfPossiblePatterns < lowestTile)
            {
                lowestTiles = new List<Tile>() { tile };

                lowestTile = tile.NumberOfPossiblePatterns;

            }
            else if (tile.NumberOfPossiblePatterns == lowestTile)
            {
                lowestTiles.Add(tile);
            }

            Debug.Log("Propagating Grid");
        }

        //Select a random tile out of the list
        int rndIndex = Random.Range(0, lowestTiles.Count);
        Tile tileToSet = lowestTiles[rndIndex];

        Debug.Log($" Random Index { rndIndex }: {tileToSet.PossiblePatterns.Count}");


        //Assign one of the possible patterns to the tile
        tileToSet.AssignRandomPossiblePattern();
    }

    //Cardinal Directions Establishment 
    public List<Vector3Int> GetTileDirectionList()
    {
        List<Vector3Int> tileDirections = new List<Vector3Int>();
        foreach (Vector3Int tileDirection in Util.Directions)
        {
            if (Util.CheckInBounds(GridDimensions, Index))
            {
                tileDirections.Add((Vector3Int)tileDirection);
            }
        }
        return tileDirections;

    }

    //tile to unsetTile, added possibleNeighbours, List<Tile> newPossiblePatterns
    public List<Tile> GetNeighbour(List<TilePattern> newPossiblePatterns)
    {
        List<Tile> possibleNeighbours = new List<Tile>();
        IEnumerable<object> tileDirections = null;
        foreach (var unsetTiles in tileDirections)
        {
            if (unsetTiles == newPossiblePatterns)
            {
                possibleNeighbours.Add((Tile)unsetTiles);
            }
        }

        return possibleNeighbours;
    }

    public List<Tile> GetUnsetTiles()
    {
        List<Tile> unsetTiles = new List<Tile>();

        //Loop over all the tiles and check which ones are not set
        foreach (var tile in GetTilesFlattened())
        {
            if (!tile.Set) unsetTiles.Add(tile);

            Debug.Log(tile.PossiblePatterns.Count);
        }
        Debug.Log(unsetTiles.Count);
        return unsetTiles;
    }

    private List<Tile> GetTilesFlattened()
    {
        List<Tile> tiles = new List<Tile>();
        for (int x = 0; x < GridDimensions.x; x++)
        {
            for (int y = 0; y < GridDimensions.y; y++)
            {
                for (int z = 0; z < GridDimensions.z; z++)
                {
                    tiles.Add(TileGrid[x, y, z]);
                }
            }
        }
        return tiles;
    }

    //this function removes the colored sides used for connections
    public void VoidVisability()
    {
        foreach (var tile in TileGrid)
        {
            if (tile.Set)
            {
                tile.VisibilitySwitch();
            }
        }

    }

    #endregion
}



