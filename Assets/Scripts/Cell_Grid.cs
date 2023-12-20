using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This class manages a cellular automata grid.
///     Responsibilities include:
///         - Data management
///         - Mapping from Cell.Data.Type to Cell ScriptableObject reference
///         - Lifecycle stuff
/// </summary>
[ExecuteAlways]
public class Cell_Grid : MonoBehaviour
{
    //          TYPES
    public enum RunType
    {
        NONE,
        SINGLE,
        CONTINUOUS
    }

    public enum Wrap
    {
        CLAMP,
        REPEAT
    }

    [System.Serializable]
    public struct TypeMapping
    {
        public Cell.Data.Type typ;
        public Cell cellBehavior;
    }

    //          STATIC

    //          REFERENCES

    //          PARAMETERS
    public bool Initialize = false;
    public RunType run = RunType.NONE;
    public float updateInterval = 0.1f;

    public Vector2Int Size;
    public Wrap wrp;

    public TypeMapping[] Mapping;
    public Cell.Data[] initialState;
    public Cell defaultState;

    //          PROPERTIES
    private Cell.Data[,] _mapRead { get { return this._map[this._rwToggle ? 0 : 1]; } }
    private Cell.Data[,] _mapWrite { get { return this._map[this._rwToggle ? 1 : 0]; } }

    //          INTERNAL
    private Dictionary<Cell.Data.Type, Cell> _typeMapping;
    private HashSet<Cell> _activeCellTypes;

    private bool _rwToggle;
    private Cell.Data[][,] _map;
    private Vector2Int _size;

    private float _lastUpdateTime;

    //          LIFECYCLE
    private void Start()
    {
        this.InitializeGrid();
    }

    private void Update()
    {
        if (this.Initialize)
            this.InitializeGrid();

        if (this.run != RunType.NONE && (Time.time - this._lastUpdateTime) > this.updateInterval)
        {
            if (this.run == RunType.SINGLE)
                this.run = RunType.NONE;

            this.RunAutomata();
            this.VisualizeAutomata();
        }
    }

    //          BEHAVIORS
    public void EnumerateGrid(Cell.EnumerationCallback callback)
    {
        for (int x = 0; x < this._size.x; x++)
            for (int y = 0; y < this._size.y; y++)
                callback(this.GetCell(x, y));
    }

    private Vector2Int ConditionAddress(Vector2Int addr)
    {
        Vector2Int result = new Vector2Int();

        switch(this.wrp)
        {
            case Wrap.CLAMP:
                result.x = Mathf.Max(Mathf.Min(addr.x, this._size.x), 0);
                result.y = Mathf.Max(Mathf.Min(addr.y, this._size.y), 0);
                break;
            case Wrap.REPEAT:
                result.x = (int) Mathf.Repeat(addr.x, this._size.x);
                result.y = (int) Mathf.Repeat(addr.y, this._size.y);
                break;
            default:
                break;
        }

        return result;
    }

    private void InitializeGrid()
    {
        // Catch input
        this.Initialize = false;

        // Create mapping
        this._typeMapping = new Dictionary<Cell.Data.Type, Cell>();
        for (int i = 0; i < this.Mapping.Length; i++)
            this._typeMapping[this.Mapping[i].typ] = this.Mapping[i].cellBehavior;

        // Create storage
        if (true || (this._map == null || this._size != this.Size))
        {
            this._size = this.Size;

            // Create two copies of data; a "current" and "previous"
            this._map = new Cell.Data[2][,];
            this._map[0] = new Cell.Data[this._size.x, this._size.y];
            this._map[1] = new Cell.Data[this._size.x, this._size.y];
        }

        // Load default state
        Cell.EnumerationCallback cb = c =>
        {
            // Copy default state
            Cell.Data dflt = this.defaultState.defaultData;
            dflt.addr = c.addr;

            // Set cell state
            this.SetCell(dflt);
        };
        this.EnumerateGrid(cb);

        // Load initial state
        this._activeCellTypes = new HashSet<Cell>();
        for (int i = 0; i < this.initialState.Length; i++)
        {
            this.SetCell(this.initialState[i]);
            this._activeCellTypes.Add(this._typeMapping[this.initialState[i].typ]);
        }

        // Visualize inital state
        this.VisualizeAutomata();
    }

    private void RunAutomata()
    {
        float deltaT = Time.time - this._lastUpdateTime;
        Cell.EnumerationCallback cb = c =>
        {
            // Get Cell behavior from mapping
            if (this._typeMapping != null && this._typeMapping.ContainsKey(c.typ))
            {
                // Trigger cell to act
                this._typeMapping[c.typ].UpdateCell(c, this, deltaT);
            }
        };

        this.EnumerateGrid(cb);

        // Swap Read and Write
        this._rwToggle = !this._rwToggle;

        // Record update time
        this._lastUpdateTime = Time.time;
    }

    private void VisualizeAutomata()
    {
        foreach (Cell cl in this._activeCellTypes)
            cl.Visualize(this);
    }

    public Cell.Data GetCell(int x, int y)
    {
        return this.GetCell(new Vector2Int(x, y));
    }

    public Cell.Data GetCell(Vector2Int addr)
    {
        // Condition address
        addr = this.ConditionAddress(addr);

        // Get cell data
        Cell.Data dt = this._mapRead[addr.x, addr.y];

        // Catch uninitialized addresses
        if (dt.addr != addr)
            dt.addr = addr;

        // Return
        return dt;
    }

    public void SetCell(Cell.Data dt)
    {
        // Condition address
        Vector2Int addr = this.ConditionAddress(dt.addr);

        // Set
        this._mapWrite[addr.x, addr.y] = dt;
    }
}
