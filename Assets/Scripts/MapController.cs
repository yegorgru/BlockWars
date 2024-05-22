using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.Collections.AllocatorManager;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;
using Unity.VisualScripting;

[System.Serializable]
public class BlockType
{
    public GameObject prefab;
}

public enum GridCell
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

public class MapController : MonoBehaviour
{
    public BlockType[] blockTypes;
    public int rows = 10;
    public int cols = 10;
    public int paths = 1;
    public float blockSize = 1f;
    public float glassChanceBoost = 0.2f;
    public GameObject character;
    public GameObject enemy;
    public GameObject bullet;
    private Vector2Int heroPosition;
    private Vector2Int enemyPosition;
    private GameObject characterObject;
    private List<List<GridCell>> logicalGrid;

    void Start()
    {
        //yCoord = rows / 2;
        heroPosition.y = rows / 2;
        enemyPosition.y = rows / 2;
        GenerateGrid();
    }

    void Update()
    {
        /*int prevXCoord = xCoord;
        int prevYCoord = yCoord;
        if (Input.GetKeyDown(KeyCode.UpArrow) == true)
        {
            ++yCoord;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) == true)
        {
            --yCoord;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) == true)
        {
            --xCoord;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) == true)
        {
            ++xCoord;
        }
        if (!(xCoord >= 0 && xCoord < cols && yCoord >= 0 && yCoord < rows && logicalGrid[yCoord][xCoord] == GridCell.Empty))
        {
            xCoord = prevXCoord;
            yCoord = prevYCoord;
        }
        else
        {
            logicalGrid[yCoord][xCoord] = GridCell.Hero;
            logicalGrid[prevYCoord][prevXCoord] = GridCell.Empty;
            characterObject.transform.position = transform.position + new Vector3((xCoord + 0.5f) * blockSize, (yCoord + 0.5f) * blockSize, 0f);
        }
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 direction = mousePos - characterObject.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        characterObject.transform.rotation = rotation;
        if (Input.GetKeyDown(KeyCode.Space) == true)
        {
            Instantiate(bullet, characterObject.transform.position, characterObject.transform.rotation * Quaternion.Euler(0, 0, -90));
        }*/
    }

    public void ResetEnvironment()
    {
        // Implement logic to reset the environment and the hero's position
        GenerateGrid();
    }

    public List<List<GridCell>> GetLogicalGrid()
    {
        return logicalGrid;
    }

    void GenerateGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        logicalGrid = GenerateLogicalGrid();
        
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
                else if (logicalGrid[row][col] == GridCell.Enemy)
                {
                    prefab = enemy;
                }
                Vector3 position = new Vector3((col + 0.5f) * blockSize, (row + 0.5f) * blockSize, 0f);
                GameObject newBlock = Instantiate(prefab, transform.position + position, Quaternion.identity, transform);
                newBlock.transform.parent = transform;
                if (logicalGrid[row][col] == GridCell.Hero)
                {
                    characterObject = newBlock;
                    heroPosition = new Vector2Int(col, row);
                }
                if (logicalGrid[row][col] == GridCell.Enemy)
                {
                    newBlock.transform.rotation = Quaternion.Euler(0, 0, 180);
                    enemyPosition = new Vector2Int(col, row);
                }
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
        if(Random.Range(0f, 1f) < glassChanceBoost)
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

    public bool MoveHero(Vector2Int newPosition)
    {
        int newX = newPosition.x;
        int newY = newPosition.y;

        if (newX >= 0 && newX < cols && newY >= 0 && newY < rows && logicalGrid[newY][newX] == GridCell.Empty)
        {
            logicalGrid[newY][newX] = GridCell.Hero;
            logicalGrid[heroPosition.y][heroPosition.x] = GridCell.Empty;
            heroPosition = newPosition;
            characterObject.transform.position = transform.position + new Vector3((heroPosition.x + 0.5f) * blockSize, (heroPosition.y + 0.5f) * blockSize, 0f);
            return true;
        }
        return false;
    }

    public Vector2Int GetHeroPosition()
    {
        return heroPosition;
    }

    public Quaternion GetHeroRotation()
    {
        return characterObject.transform.rotation;
    }

    public Vector2Int GetEnemyPosition()
    {
        return enemyPosition;
    }

    public void SetHeroRotation(Quaternion rotation)
    {
        characterObject.transform.rotation = rotation;
    }

    public void HeroShoot()
    {
        Instantiate(bullet, characterObject.transform.position, characterObject.transform.rotation * Quaternion.Euler(0, 0, -90));
    }

    public Vector3 GetCharacterPosition()
    {
        return characterObject.transform.position;
    }
}
