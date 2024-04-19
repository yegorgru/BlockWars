using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.Collections.AllocatorManager;
using UnityEngine.UIElements;

[System.Serializable]
public class BlockType
{
    public GameObject prefab;
    //public Color color;
}

enum GridCell
{
    Empty,
    Hero,
    Enemy,
    UninitializedWall,
    Block,
    Box,
    Glass,
    Grass
}

public class MapGenerator : MonoBehaviour
{
    public BlockType[] blockTypes;
    public int rows = 10;
    public int cols = 10;
    public int paths = 1;
    public float blockSize = 1f;
    public float glassChance = 0.2f;
    public GameObject character;

    void Start()
    {
        GenerateGrid();
    }

    void Update()
    {
        
    }

    void GenerateGrid()
    {
        List<List<GridCell>> logicalGrid = GenerateLogicalGrid();
        
        for (int row = 0; row < logicalGrid.Count; row++)
        {
            for (int col = 0; col < logicalGrid[0].Count; col++)
            {
                if(logicalGrid[row][col] == GridCell.Empty)
                {
                    continue;
                }
                GameObject prefab = character;
                if (logicalGrid[row][col] == GridCell.Block)
                {
                    prefab = blockTypes[0].prefab;
                }
                else if (logicalGrid[row][col] == GridCell.Box)
                {
                    prefab = blockTypes[1].prefab;
                }
                else if (logicalGrid[row][col] == GridCell.Glass)
                {
                    prefab = blockTypes[2].prefab;
                }
                else if (logicalGrid[row][col] == GridCell.Grass)
                {
                    prefab = blockTypes[3].prefab;
                } 
                Vector3 position = new Vector3((col + 0.5f) * blockSize, (row + 0.5f) * blockSize, 0f);
                GameObject newBlock = Instantiate(prefab, transform.position + position, Quaternion.identity, this.transform);
                newBlock.transform.parent = transform;
            }
        }
    }

    List<List<GridCell>> GenerateLogicalGrid()
    {
        List<List<GridCell>> result = new List<List<GridCell>>();

        for (int i = 0; i < rows; i++)
        {
            result.Add(new List<GridCell>());
            for (int j = 0; j < cols; j++)
            {
                result[i].Add(GridCell.UninitializedWall);
            }
        }
        BuildPaths(result);
        BuildWalls(result);
        result[rows / 2][0] = GridCell.Hero;
        result[rows / 2][cols - 1] = GridCell.Enemy;
        return result;
    }

    void BuildPaths(List<List<GridCell>> grid)
    {
        BuildPath(grid, true);
        for (int i = 1; i < paths; ++i)
        {
            BuildPath(grid, false);
        }
    }

    void BuildWalls(List<List<GridCell>> grid)
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if(grid[i][j] == GridCell.UninitializedWall)
                {
                    BuildWall(grid, i, j);
                }          
            }
        }
    }

    void BuildPath(List<List<GridCell>> grid, bool toCenter)
    {
        int y = toCenter ? rows / 2 : Random.Range(0, rows);
        for(int x = 0; x < cols;)
        {
            while(true)
            {
                grid[y][x] = GridCell.Empty;
                int direction = Random.Range(0, 3);
                if(direction == 0)
                {
                    --y;
                    if (y < 0)
                    {
                        y = 0;
                        ++x;
                        break;
                    }
                }
                else if(direction == 1)
                {
                    ++x;
                    break;
                }
                else
                {
                    ++y;
                    if (y > rows - 1)
                    {
                        y = rows - 1;
                        ++x;
                        break;
                    }
                }
            }
        }
        if(toCenter)
        {
            int minY = Mathf.Min(y, rows / 2);
            int maxY = Mathf.Max(y, rows / 2);
            for(int i = minY; i < maxY; ++i)
            {
                grid[i][cols - 1] = GridCell.Empty;
            }
        }
    }

    void BuildWall(List<List<GridCell>> grid, int row, int col)
    {
        if(Random.Range(0f, 1f) < glassChance)
        {
            grid[row][col] = GridCell.Glass;
            return;
        }
        float blockValue = Random.Range(0f, 4f);
        int neighborsCount = 0;
        float neighborsValue = 0f;
        for(int i = row > 0 ? row - 1 : 0; i < (row < rows - 1 ? row + 1 : rows - 1); ++i)
        {
            for (int j = col > 0 ? col - 1 : 0; j < (col < cols - 1 ? col + 1 : cols - 1); ++j)
            {
                if (grid[i][j] == GridCell.UninitializedWall || grid[i][j] == GridCell.Empty ||
                    grid[i][j] == GridCell.Hero || grid[i][j] == GridCell.Enemy)
                {
                    continue;
                }
                else
                {
                    neighborsCount++;
                    if(grid[i][j] == GridCell.Block)
                    {
                        neighborsValue += 1f;
                    }
                    else if (grid[i][j] == GridCell.Glass)
                    {
                        neighborsValue += 2f;
                    }
                    else if (grid[i][j] == GridCell.Box)
                    {
                        neighborsValue += 3f;
                    }
                }
            }
        }
        if(neighborsCount != 0)
        {
            blockValue = (blockValue + (neighborsValue / neighborsCount)) / 2f;
        }
        int blockCastedValue = (int)blockValue;
        if(blockCastedValue == 0)
        {
            grid[row][col] = GridCell.Grass;
        }
        else if (blockCastedValue == 1)
        {
            grid[row][col] = GridCell.Block;
        }
        else if (blockCastedValue == 2)
        {
            grid[row][col] = GridCell.Glass;
        }
        else if (blockCastedValue == 3)
        {
            grid[row][col] = GridCell.Box;
        }
    }
}
