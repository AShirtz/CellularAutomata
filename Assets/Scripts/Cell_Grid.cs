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

    public Vector2Int Size;
    public Wrap wrp;

    public TypeMapping[] Mapping;

    //          PROPERTIES
    private Cell.Data[,] _mapRead { get { return this._map[this._rwToggle ? 0 : 1]; } }
    private Cell.Data[,] _mapWrite { get { return this._map[this._rwToggle ? 1 : 0]; } }

    //          INTERNAL
    private Dictionary<Cell.Data.Type, Cell> _typeMapping;

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

        if (this.run != RunType.NONE)
        {
            if (this.run == RunType.SINGLE)
                this.run = RunType.NONE;

            this.RunAutomata();
        }
    }

    //          BEHAVIORS
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
        if (this._map == null || this._size != this.Size)
        {
            this._size = this.Size;

            // Create two copies of data; a "current" and "previous"
            this._map = new Cell.Data[2][,];
            this._map[0] = new Cell.Data[this._size.x, this._size.y];
            this._map[1] = new Cell.Data[this._size.x, this._size.y];
        }

        // TODO: Load initial state somehow
    }

    private void RunAutomata()
    {
        float deltaT = Time.time - this._lastUpdateTime;

        for (int x = 0; x < this._size.x; x++)
        {
            for (int y = 0; y < this._size.y; y++)
            {
                // Get cell data
                Cell.Data dt = this.GetCell(x, y);

                // Get Cell behavior from mapping
                if (this._typeMapping != null && this._typeMapping.ContainsKey(dt.typ))
                {
                    // Trigger cell to act
                    this._typeMapping[dt.typ].Update(dt, this, deltaT);
                }
            }
        }

        // Swap Read and Write
        this._rwToggle = !this._rwToggle;

        // Record update time
        this._lastUpdateTime = Time.time;
    }

    private void VisualizeAutomata()
    {
        for (int x = 0; x < this._size.x; x++)
        {
            for (int y = 0; y < this._size.y; y++)
            {
                // Get cell data
                Cell.Data dt = this.GetCell(x, y);

                // Get Cell behavior from mapping
                if (this._typeMapping != null && this._typeMapping.ContainsKey(dt.typ))
                {
                    // Trigger cell to act
                    this._typeMapping[dt.typ].Visualize(dt, this);
                }
            }
        }
    }

    public Cell.Data GetCell(int x, int y)
    {
        return this.GetCell(new Vector2Int(x, y));
    }

    public Cell.Data GetCell(Vector2Int addr)
    {
        // Condition address
        addr = this.ConditionAddress(addr);

        // Return
        return this._mapRead[addr.x, addr.y];
    }

    public void SetCell(Cell.Data dt)
    {
        // Condition address
        Vector2Int addr = this.ConditionAddress(dt.addr);

        // Set
        this._mapWrite[addr.x, addr.y] = dt;
    }
}
