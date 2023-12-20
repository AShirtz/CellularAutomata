using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This class defines the data and behaviors of a cell automata.
///     This class provides the basics, and subclasses define specific behaviors.
///     
///     As this is a ScriptableObject, it's shared among all cells in the grid.
///     This means we don't end up instantitating objects per cell.
///     This also means that 'this' doesn't refer to the cell, but the ScriptableObject.
///     Instead, think about an instance of Cell.Data as being a single cell.
/// </summary>
public abstract class Cell : ScriptableObject
{
    //          TYPES
    [System.Serializable]
    public struct Data
    {
        public enum Type
        {
            INVALID     = 0,

            LIFE        = 1,

            GRASS       = 2,
            TREE        = 3,
            LAVA        = 4,
            FIRE        = 5
        }

        public Type typ;
        public Vector2Int addr;

        // TODO: Add whatever variables you want here

        // Conway's Game of Life
        public bool alive;

        // Brush Fire Sim
        public float curTemperature;
        public float curFuel;
        public float ignitionTemperature;
    }

    public delegate void EnumerationCallback(Data nghbr);

    //          STATIC

    //          REFERENCES

    //          PARAMETERS
    [Tooltip("Data for an uninitialized instance of this type.")]
    public Cell.Data defaultData;

    // https://en.wikipedia.org/wiki/Moore_neighborhood
    [Tooltip("Radius of the neighborhood of this cell type.")]
    public int neighborhoodRadius = 1;

    //          INTERNAL

    //          LIFECYCLE

    /// <summary>
    /// Allows the cell to update it's internal state.
    /// </summary>
    /// <param name="self">The cell that is acting.</param>
    /// <param name="grid">Reference to the cell grid.</param>
    /// <param name="deltaTime">The amount of time that's passed since the last update.</param>
    public abstract void Update(Cell.Data self, Cell_Grid grid, float deltaTime);

    /// <summary>
    /// Allows a cell to update it's visualization.
    /// May No-op for cells that are visualized as a group (e.g., as a texture).
    /// </summary>
    /// <param name="self"></param>
    /// <param name="grid"></param>
    public abstract void Visualize(Cell.Data self, Cell_Grid grid);

    //          BEHAVIORS

    /// <summary>
    /// Scans the Moore neighborhood around the given cell.
    /// https://en.wikipedia.org/wiki/Moore_neighborhood
    /// </summary>
    /// <param name="self">The cell that is acting.</param>
    /// <param name="grid">Reference to the cell grid.</param>
    public virtual void EnumerateNeighbors(Cell.Data self, Cell_Grid grid, EnumerationCallback callback)
    {
        for (int xOff = -1 * this.neighborhoodRadius; xOff <= 1; xOff++)
        {
            for (int yOff = -1 * this.neighborhoodRadius; yOff <= 1; yOff++)
            {
                // Don't act on self
                if (xOff == 0 && yOff == 0)
                    continue;

                // Get address of other cell
                Vector2Int addr = self.addr + new Vector2Int(xOff, yOff);

                // Pass other cell to callback
                callback(grid.GetCell(addr));
            }
        }
    }

    /// <summary>
    /// Allows cells to have their own interaction between types
    /// </summary>
    /// <param name="self">The cell that is acting.</param>
    /// <param name="other">The other cell that is being reacted to.</param>
    /// <param name="grid"></param>
    public abstract void Interact(Cell.Data self, Cell.Data other, Cell_Grid grid);
}
