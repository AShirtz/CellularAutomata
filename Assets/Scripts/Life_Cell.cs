using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Life_Cell : Cell
{
    //          REFERENCES
    public RenderTexture vizTex;

    //          BEHAVIORS


    //          CELL
    public override void Update(Data self, Cell_Grid grid, float deltaTime)
    {
        // Enumerate Neighbors, adding up those alive
        int numAlive = 0;
        EnumerationCallback cb = ng =>
        {
            if (ng.typ == Data.Type.LIFE && ng.alive)
                numAlive++;
        };

        this.EnumerateNeighbors(self, grid, cb);

        // Interpret results
        if (numAlive < 2)
            self.alive = false;
        else if (numAlive >= 2 && numAlive <= 3 && self.alive)
            self.alive = true;
        else if (numAlive > 3 && self.alive)
            self.alive = false;
        else if (numAlive == 3)
            self.alive = true;

        grid.SetCell(self, self.addr);
    }

    public override void Visualize(Data self, Cell_Grid grid)
    {
        // TODO
    }

    public override void Interact(Data self, Data other, Cell_Grid grid)
    {
        // TODO
    }
}
