using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Life_Cell",menuName = "Cell/Life", order = 0)]
public class Life_Cell : Cell
{
    //          REFERENCES
    public Material vizMat;

    //          INTERNAL
    private Texture2D _vizTex;

    //          BEHAVIORS
    public override void UpdateCell(Data self, Cell_Grid grid, float deltaTime)
    {
        // Create delegate for callback
        int numAlive = 0;
        EnumerationCallback cb = ng =>
        {
            // Skip self
            if (ng.addr == self.addr)
                return;

            // Count up alive neighbors
            if (ng.typ == Data.Type.LIFE && ng.alive)
                numAlive++;
        };

        // Enumerate neighbors
        this.EnumerateNeighbors(self, 1, grid, cb);

        // Interpret results
        if (numAlive < 2)
            self.alive = false;
        else if (numAlive >= 2 && numAlive <= 3 && self.alive)
            self.alive = true;
        else if (numAlive > 3 && self.alive)
            self.alive = false;
        else if (numAlive == 3)
            self.alive = true;

        // Apply state to self
        grid.SetCell(self);
    }

    public override void Visualize(Cell_Grid grid)
    {
        // Create CPU side texture
        if (this._vizTex == null || this._vizTex.width != grid.Size.x || this._vizTex.height != grid.Size.y)
        {
            this._vizTex = new Texture2D(grid.Size.x, grid.Size.y, TextureFormat.BGRA32, false);
            this._vizTex.filterMode = FilterMode.Point;
        }

        // Create enumeration callback
        EnumerationCallback cb =
            c =>
            {
                // Guard
                if (c.typ != Data.Type.LIFE)
                    return;

                // Color based on state
                this._vizTex.SetPixel(c.addr.x, c.addr.y, (c.alive ? Color.blue : Color.red));
            };

        // Enumerate grid
        grid.EnumerateGrid(cb);

        // Apply texture writes
        this._vizTex.Apply();

        // Apply texture to material
        this.vizMat.SetTexture("_BaseMap", this._vizTex);
    }
}