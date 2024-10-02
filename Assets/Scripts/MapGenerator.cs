using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public Tilemap tilemap;                
    public TileBase backgroundTile;        
    public TileBase horizontalRoadTile;    
    public TileBase verticalRoadTile;      
    public TileBase tIntersectionTile;     
    public TileBase crossIntersectionTile; 
    public TileBase cornerTileDL;          
    public TileBase cornerTileUR;          
    public int mapWidth = 20;              
    public int mapHeight = 20;             
    public float noiseScale = 0.4f;        // Scale for Perlin noise
    int seed;                              // Seed for randomization

    public GameObject treePrefab;          
    public int treeCount = 55;             
    public float minTreeDistance = 0.01f;  

    private List<Vector3> treePositions = new List<Vector3>(); // List to track tree positions

    
    void Start()
    {
        ClearTilemap();                    
        seed = Random.Range(0, 10000);     // Generate a random seed
        FillBackground();                  
        GenerateRoads();                   
        SmoothRoads();                     
        PlaceFractalTrees();               
    }

    // Clear the tilemap by removing all existing tiles
    void ClearTilemap()
    {
        tilemap.ClearAllTiles();           
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                tilemap.SetTile(tilePosition, null); 
            }
        }
        tilemap.RefreshAllTiles();
    }

    // Fill the entire map with the background tile
    void FillBackground()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                tilemap.SetTile(tilePosition, backgroundTile);
            }
        }
    }

    // Generate roads using Perlin noise
    void GenerateRoads()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // Generate Perlin noise value for the current tile
                float perlinValue = PerlinNoiseGenerator.PerlinNoise((x + seed) * noiseScale, (y + seed) * noiseScale);

                // If the Perlin value exceeds the threshold, place a road tile
                if (perlinValue > 0.5f)
                {
                    Vector3Int tilePosition = new Vector3Int(x, y, 0);
                    PlaceCorrectRoadTile(x, y, tilePosition); 
                }
            }
        }
        tilemap.RefreshAllTiles();
    }

    // Place fractal trees randomly
    void PlaceFractalTrees()
    {
        int placedTreeCount = 0;

        
        while (placedTreeCount < treeCount)
        {
            int randomX = Random.Range(1, mapWidth - 1); 
            int randomY = Random.Range(1, mapHeight - 1); 
            Vector3Int tilePosition = new Vector3Int(randomX, randomY, 0);
            Vector3 worldPosition = tilemap.CellToWorld(tilePosition) + new Vector3(0.5f, 0.5f, 0);

            
            if (!IsRoadAtPosition(randomX, randomY) && !IsTooCloseToOtherTrees(worldPosition))
            {
                Instantiate(treePrefab, worldPosition, Quaternion.identity, this.transform); 
                treePositions.Add(worldPosition); 
                placedTreeCount++;
            }
        }
    }

    // Check if new tree is too close to existing trees
    bool IsTooCloseToOtherTrees(Vector3 position)
    {
        foreach (Vector3 treePos in treePositions)
        {
            if (Vector3.Distance(treePos, position) < minTreeDistance)
            {
                return true;
            }
        }
        return false;
    }

    // Determine the correct type of road tile to place based on neighboring tiles
    void PlaceCorrectRoadTile(int x, int y, Vector3Int tilePosition)
    {
        bool left = IsRoadAtPosition(x - 1, y);
        bool right = IsRoadAtPosition(x + 1, y);
        bool up = IsRoadAtPosition(x, y + 1);
        bool down = IsRoadAtPosition(x, y - 1);

        // Decide which road tile to place based on neighboring roads
        if (left && right && up && down)
        {
            tilemap.SetTile(tilePosition, crossIntersectionTile); // Cross-intersection
        }
        // T-intersections
        else if (left && right && up && !down)
        {
            tilemap.SetTile(tilePosition, tIntersectionTile); 
        }
        else if (left && right && !up && down)
        {
            tilemap.SetTile(tilePosition, verticalRoadTile);
        }
        else if (left && !right && up && down)
        {
            tilemap.SetTile(tilePosition, horizontalRoadTile);
        }
        else if (!left && right && up && down)
        {
            tilemap.SetTile(tilePosition, horizontalRoadTile); 
        }
        // Upper-left corner
        else if (left && up && !right && !down)
        {
            tilemap.SetTile(tilePosition, horizontalRoadTile); 
        }
        // Upper-right corner
        else if (right && up && !left && !down)
        {
            tilemap.SetTile(tilePosition, cornerTileUR); 
        }
        // Down-left corner
        else if (left && down && !right && !up)
        {
            tilemap.SetTile(tilePosition, cornerTileDL); 
        }
        // Down-right corner
        else if (right && down && !left && !up)
        {
            tilemap.SetTile(tilePosition, verticalRoadTile);
        }
        else if (left && right && !up && !down)
        {
            tilemap.SetTile(tilePosition, horizontalRoadTile); // Horizontal road
        }
        else if (!left && !right && up && down)
        {
            tilemap.SetTile(tilePosition, verticalRoadTile); // Vertical road
        }
    }
    
    bool IsRoadAtPosition(int x, int y)
    {
        if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight)
            return false;
        
        float perlinValue = PerlinNoiseGenerator.PerlinNoise((x+seed) * noiseScale, (y+seed) * noiseScale);
        return perlinValue > 0.5f;
    }

    // Smooth the road by adjusting isolated tiles
    void SmoothRoads()
    {
        for (int x = 1; x < mapWidth - 1; x++)
        {
            for (int y = 1; y < mapHeight - 1; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                // Check if the current tile is part of the road network
                bool isRoad = IsRoadAtPosition(x, y);
                bool left = IsRoadAtPosition(x - 1, y);
                bool right = IsRoadAtPosition(x + 1, y);
                bool up = IsRoadAtPosition(x, y + 1);
                bool down = IsRoadAtPosition(x, y - 1);

                
                if (isRoad && !left && right && !up && !down)
                {
                    tilemap.SetTile(tilePosition, horizontalRoadTile);
                }
                else if (isRoad && left && !right && !up && !down)
                {
                    tilemap.SetTile(tilePosition, horizontalRoadTile);
                }
                else if (isRoad && !left && !right && up && !down)
                {
                    tilemap.SetTile(tilePosition, verticalRoadTile);
                }
                else if (isRoad && !left && !right && !up && down)
                {
                    tilemap.SetTile(tilePosition, verticalRoadTile);
                }
            }
        }
    }
}

// Class responsible for generating Perlin noise
class PerlinNoiseGenerator
{
    // Predefined gradient vectors used in Perlin noise
    private static Vector2[] gradients = {
        new Vector2(1,1), new Vector2(-1,1), new Vector2(1,-1), new Vector2(-1,-1),
        new Vector2(1,0), new Vector2(-1,0), new Vector2(0,1), new Vector2(0,-1)
    };

    // Smoothing function for transitions between points
    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    // Dot product of the gradient and distance vector
    private static float DotGridGradient(int ix, int iy, float x, float y)
    {
        int gradientIndex = (ix + iy) % gradients.Length;
        Vector2 gradient = gradients[gradientIndex];

        float dx = x - ix;
        float dy = y - iy;

        return (dx * gradient.x + dy * gradient.y);
    }

    // Linear interpolation between two points
    private static float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }

    // Generates Perlin noise at given coordinates
    public static float PerlinNoise(float x, float y)
    {
        int x0 = Mathf.FloorToInt(x);
        int x1 = x0 + 1;
        int y0 = Mathf.FloorToInt(y);
        int y1 = y0 + 1;

        float sx = x - x0;
        float sy = y - y0;

        float fadeX = Fade(sx);
        float fadeY = Fade(sy);

        float n0, n1, ix0, ix1, value;

        n0 = DotGridGradient(x0, y0, x, y);
        n1 = DotGridGradient(x1, y0, x, y);
        ix0 = Lerp(n0, n1, fadeX);

        n0 = DotGridGradient(x0, y1, x, y);
        n1 = DotGridGradient(x1, y1, x, y);
        ix1 = Lerp(n0, n1, fadeX);

        value = Lerp(ix0, ix1, fadeY);
        return (value + 1) / 2.0f;
    }
}
