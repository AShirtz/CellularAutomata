using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Fire_Cell", menuName = "Cell/Fire", order = 1)]
public class Fire_Cell : Cell
{
    //          REFERENCES
    public Material vizMat;

    //          PARAMETERS
    [Tooltip("Temperature at which plants ignite.")]
    public float ignitionTemperature;

    [Tooltip("Maximum fuel that a cell may have.")]
    public float maxFuel;

    [Tooltip("Rate at which a cell with plants grows fuel.")]
    public float fuelGrowthRate;

    [Tooltip("Rate at which a cell on fire consumes fuel.")]
    public float fuelBurnRate;

    [Tooltip("Rate at which a cell heats up while on fire.")]
    public float burnTempIncreaseRate;

    [Tooltip("Rate at which stone cools down.")]
    public float stoneCoolTempRate;

    [Tooltip("Visualizes the temperature and fuel in the red and green channels respectively.")]
    public bool viewData;
    

    //          INTERNAL
    private Texture2D _vizTex;

    //          BEHAVIORS
    public override void UpdateCell(Data self, Cell_Grid grid, float deltaTime)
    {
        Data result = self;

        switch(self.typ)
        {
            case Data.Type.PLANT:
                result = this.UpdatePlant(self, grid, deltaTime);
                break;
            case Data.Type.STONE:
                result = this.UpdateStone(self, grid, deltaTime);
                break;
            case Data.Type.LAVA:
                result = this.UpdateLava(self, grid, deltaTime);
                break;
            case Data.Type.FIRE:
                result = this.UpdateFire(self, grid, deltaTime);
                break;
            case Data.Type.WATER:
                result = this.UpdateWater(self, grid, deltaTime);
                break;
            default:
                break;
        }

        // Apply state to cell
        grid.SetCell(result);
    }

    private Data UpdatePlant(Data self, Cell_Grid grid, float deltaTime)
    {
        // Grow => increase fuel
        self.curFuel = Mathf.Min(self.curFuel + (this.fuelGrowthRate * deltaTime), this.maxFuel);

        // Set temperature to average of neighbors
        float avgTemp = 0f;
        int numNeighbors = 0;
        EnumerationCallback cb =
            c =>
            {
                // Count neighbor
                numNeighbors++;

                // Sum temperature
                avgTemp += c.curTemperature;
            };

        EnumerateNeighbors(self, 2, grid, cb);
        avgTemp /= (numNeighbors > 0 ? numNeighbors : 1);
        self.curTemperature = avgTemp;

        // Check for ignition
        if (self.curTemperature > this.ignitionTemperature)
            self.typ = Data.Type.FIRE;

        return self;
    }

    private Data UpdateStone(Data self, Cell_Grid grid, float deltaTime)
    {
        // Set temperature to average of neighbors
        float avgTemp = 0f;
        int numNeighbors = 0;
        EnumerationCallback cb =
            c =>
            {
                // Count neighbor
                numNeighbors++;

                // Sum temperature
                avgTemp += c.curTemperature;
            };

        EnumerateNeighbors(self, 2, grid, cb);
        avgTemp /= (numNeighbors > 0 ? numNeighbors : 1);
        self.curTemperature = avgTemp;

        // Cool stone
        self.curTemperature = Mathf.Max(self.curTemperature - (this.stoneCoolTempRate * deltaTime), 0f);

        return self;
    }

    private Data UpdateLava(Data self, Cell_Grid grid, float deltaTime)
    {
        // Just hangout, yo
        return self;
    }

    private Data UpdateFire(Data self, Cell_Grid grid, float deltaTime)
    {
        // Increase temperature
        self.curTemperature = Mathf.Min(self.curTemperature + (this.burnTempIncreaseRate * deltaTime), 2 * this.ignitionTemperature);

        // Consume fuel
        self.curFuel -= this.fuelBurnRate * deltaTime;

        // Check for extinguish
        if (self.curFuel < 0f)
        {
            self.curFuel = 0f;
            self.typ = Data.Type.STONE;
        }

        return self;
    }

    private Data UpdateWater(Data self, Cell_Grid grid, float deltaTime)
    {
        // Just hangout, yo
        return self;
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
                // Color based on state
                Color clr = Color.black;

                if (this.viewData)
                {
                    clr.r = Mathf.Clamp01(c.curTemperature / (2 * this.ignitionTemperature));
                    clr.g = Mathf.Clamp01(c.curFuel / this.maxFuel);
                }
                else
                {
                    switch (c.typ)
                    {
                        case Data.Type.PLANT:
                            clr = Color.green;
                            break;
                        case Data.Type.STONE:
                            clr = Color.grey;
                            break;
                        case Data.Type.LAVA:
                            clr = Color.red;
                            break;
                        case Data.Type.FIRE:
                            clr = Color.yellow;
                            break;
                        case Data.Type.WATER:
                            clr = Color.blue;
                            break;
                        default:
                            break;
                    }
                }
                
                this._vizTex.SetPixel(c.addr.x, c.addr.y, clr);
            };

        // Enumerate grid
        grid.EnumerateGrid(cb);

        // Apply texture writes
        this._vizTex.Apply();

        // Apply texture to material
        this.vizMat.SetTexture("_BaseMap", this._vizTex);
    }
}
