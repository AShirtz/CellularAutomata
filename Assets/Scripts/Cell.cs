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

            // TODO: Add whatever types you want here

            // Conway's Game of Life
            LIFE        = 1,

            // Fire-Sim
            PLANT       = 2,
            STONE       = 3,
            LAVA        = 4,
            FIRE        = 5,
            WATER       = 6
        }

        public Type typ;
        public Vector2Int addr;

        // TODO: Add whatever variables you want here

        // Conway's Game of Life
        public bool alive;

        // Brush Fire Sim
        public float curTemperature;
        public float curFuel;
    }

    public delegate void EnumerationCallback(Data cl);

    //          PARAMETERS
    [Tooltip("Data for an uninitialized instance of this type.")]
    public Cell.Data defaultData;

    //          LIFECYCLE
    /// <summary>
    /// Allows the cell to update it's internal state.
    /// </summary>
    /// <param name="self">The cell that is acting.</param>
    /// <param name="grid">Reference to the cell grid.</param>
    /// <param name="deltaTime">The amount of time that's passed since the last update.</param>
    public abstract void UpdateCell(Cell.Data self, Cell_Grid grid, float deltaTime);

    /// <summary>
    /// Allows a cell to update it's visualization.
    /// May No-op for cells that are visualized as a group (e.g., as a texture).
    /// </summary>
    /// <param name="grid"></param>
    public abstract void Visualize(Cell_Grid grid);

    //          BEHAVIORS
    /// <summary>
    /// Scans the Moore neighborhood around the given cell.
    /// https://en.wikipedia.org/wiki/Moore_neighborhood
    /// </summary>
    /// <param name="self">The cell that is acting.</param>
    /// <param name="grid">Reference to the cell grid.</param>
    public virtual void EnumerateNeighbors(Cell.Data self, int radius, Cell_Grid grid, EnumerationCallback callback)
    {
        for (int xOff = -1 * radius; xOff <= radius; xOff++)
        {
            for (int yOff = -1 * radius; yOff <= radius; yOff++)
            {
                // Get address of other cell
                Vector2Int addr = self.addr + new Vector2Int(xOff, yOff);

                // Pass other cell to callback
                callback(grid.GetCell(addr));
            }
        }
    }
}
