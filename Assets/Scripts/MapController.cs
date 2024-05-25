using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockType
{
    public GameObject prefab;
}

public enum GridCell
{
    Empty,
    Agent,
    Target,
    UninitializedWall,
    Block,
    Box,
    Glass,
    Grass
}

public class MapController : MonoBehaviour
{
    public BlockType[] blockTypes;
    public GameObject agent;
    public GameObject hero;
    public GameObject target;
    public GameObject bullet;
    public bool moveHero = true;

    private int mRows = 15;
    private int mCols = 32;
    private float mBlockSize = 0.665f;
    private Vector2Int mAgentPosition;
    private Vector2Int mHeroPosition;
    private Vector2Int mTargetPosition;
    private GameObject mAgentObject;
    private GameObject mHeroObject;
    private List<List<GridCell>> mLogicalGrid;
    private int mPathsCount = 1;

    private float mLastShoot = 0f;
    private float mShootCoolDown = 0.2f;

    void Start()
    {
        mAgentPosition.y = mRows / 2;
        mTargetPosition.y = mRows / 2;
        GenerateGrid();
    }

    void FixedUpdate()
    {
        if(moveHero)
        {
            float currentTime = Time.time;
            if (currentTime - mLastShoot < mShootCoolDown)
            {
                return;
            }
            mHeroObject.SetActive(true);
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            Vector3 direction = mousePos - mHeroObject.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            mHeroObject.transform.rotation = rotation;
            if (Input.GetKey(KeyCode.UpArrow))
            {
                MoveHero(Vector2Int.up);
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                MoveHero(Vector2Int.down);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                MoveHero(Vector2Int.right);
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                MoveHero(Vector2Int.left);
            }
            else if (Input.GetKey(KeyCode.Space))
            {
                HeroShoot();
            }
            mLastShoot = currentTime;
        }
        else
        {
            mHeroObject.SetActive(false);
        }
    }

    public void ResetEnvironment()
    {
        mPathsCount = Random.Range(1, 8);
        GenerateGrid();
    }

    public List<List<GridCell>> GetLogicalGrid()
    {
        return mLogicalGrid;
    }

    void GenerateGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        mLogicalGrid = GenerateLogicalGrid();
        
        for (int row = 0; row < mLogicalGrid.Count; row++)
        {
            for (int col = 0; col < mLogicalGrid[0].Count; col++)
            {
                if(mLogicalGrid[row][col] == GridCell.Empty)
                {
                    continue;
                }
                GameObject prefab = agent;
                if (mLogicalGrid[row][col] == GridCell.Block)
                {
                    prefab = blockTypes[0].prefab;
                }
                else if (mLogicalGrid[row][col] == GridCell.Box)
                {
                    prefab = blockTypes[1].prefab;
                }
                else if (mLogicalGrid[row][col] == GridCell.Glass)
                {
                    prefab = blockTypes[2].prefab;
                }
                else if (mLogicalGrid[row][col] == GridCell.Grass)
                {
                    prefab = blockTypes[3].prefab;
                }
                else if (mLogicalGrid[row][col] == GridCell.Target)
                {
                    prefab = target;
                }
                Vector3 position = new Vector3((col + 0.5f) * mBlockSize, (row + 0.5f) * mBlockSize, 0f);
                GameObject newBlock = Instantiate(prefab, transform.position + position, Quaternion.identity, transform);
                newBlock.transform.parent = transform;
                if (mLogicalGrid[row][col] == GridCell.Agent)
                {
                    mAgentObject = newBlock;
                    mAgentPosition = new Vector2Int(col, row);

                    GameObject heroBlock = Instantiate(hero, transform.position + position, Quaternion.identity, transform);
                    heroBlock.transform.parent = transform;
                    mHeroObject = heroBlock;
                    mHeroPosition = new Vector2Int(col, row);
                }
                if (mLogicalGrid[row][col] == GridCell.Target)
                {
                    mTargetPosition = new Vector2Int(col, row);
                }
            }
        }
    }

    List<List<GridCell>> GenerateLogicalGrid()
    {
        List<List<GridCell>> result = new List<List<GridCell>>();

        for (int i = 0; i < mRows; i++)
        {
            result.Add(new List<GridCell>());
            for (int j = 0; j < mCols; j++)
            {
                result[i].Add(GridCell.UninitializedWall);
            }
        }
        BuildPaths(result);
        BuildWalls(result);
        result[mRows / 2][0] = GridCell.Agent;
        PlaceTarget(result);
        ConnectPaths(result);
        return result;
    }

    void PlaceTarget(List<List<GridCell>> grid)
    {
        int col = Random.Range(mCols/2, mCols);
        int path = Random.Range(0, mPathsCount);
        int pathCounter = 0;
        for(int i = 0; i < mRows; ++i)
        {
            if (grid[i][col] == GridCell.Empty && pathCounter++ == path)
            {
                grid[i][col] = GridCell.Target;
            }
        }
    }

    private void ConnectPaths(List<List<GridCell>> grid)
    {
        int upRow = 0;
        while (grid[upRow][0] != GridCell.Empty)
        {
            upRow++;
        }
        int downRow = mRows - 1;
        while (grid[downRow][0] != GridCell.Empty)
        {
            downRow--;
        }
        for(int i = upRow + 1; i < downRow; ++i) {
            if(grid[i][0] != GridCell.Agent)
            {
                grid[i][0] = GridCell.Empty;
            }
        }
    }

    void BuildPaths(List<List<GridCell>> grid)
    {
        BuildPath(grid, true);
        for (int i = 1; i < mPathsCount; ++i)
        {
            BuildPath(grid, false);
        }
    }

    void BuildWalls(List<List<GridCell>> grid)
    {
        for (int i = 0; i < mRows; i++)
        {
            for (int j = 0; j < mCols; j++)
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
        int y = toCenter ? mRows / 2 : Random.Range(0, mRows);
        for(int x = 0; x < mCols;)
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
                    if (y > mRows - 1)
                    {
                        y = mRows - 1;
                        ++x;
                        break;
                    }
                }
            }
        }
        if(toCenter)
        {
            int minY = Mathf.Min(y, mRows / 2);
            int maxY = Mathf.Max(y, mRows / 2);
            for(int i = minY; i < maxY; ++i)
            {
                grid[i][mCols - 1] = GridCell.Empty;
            }
        }
    }

    void BuildWall(List<List<GridCell>> grid, int row, int col)
    {
        float blockValue = Random.Range(0f, 4f);
        int neighborsCount = 0;
        float neighborsValue = 0f;
        for(int i = row > 0 ? row - 1 : 0; i < (row < mRows - 1 ? row + 1 : mRows - 1); ++i)
        {
            for (int j = col > 0 ? col - 1 : 0; j < (col < mCols - 1 ? col + 1 : mCols - 1); ++j)
            {
                if (grid[i][j] == GridCell.UninitializedWall || grid[i][j] == GridCell.Empty ||
                    grid[i][j] == GridCell.Agent || grid[i][j] == GridCell.Target)
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

    public bool MoveAgent(Vector2Int newPosition)
    {
        int newX = newPosition.x;
        int newY = newPosition.y;

        if (newX >= 0 && newX < mCols && newY >= 0 && newY < mRows && mLogicalGrid[newY][newX] == GridCell.Empty)
        {
            mLogicalGrid[newY][newX] = GridCell.Agent;
            mLogicalGrid[mAgentPosition.y][mAgentPosition.x] = GridCell.Empty;
            mAgentPosition = newPosition;
            mAgentObject.transform.position = transform.position + new Vector3((mAgentPosition.x + 0.5f) * mBlockSize, (mAgentPosition.y + 0.5f) * mBlockSize, 0f);
            return true;
        }
        return false;
    }

    public bool MoveHero(Vector2Int newPosition)
    {
        newPosition += mHeroPosition;
        int newX = newPosition.x;
        int newY = newPosition.y;

        if (newX >= 0 && newX < mCols && newY >= 0 && newY < mRows && (mLogicalGrid[newY][newX] == GridCell.Empty || mLogicalGrid[newY][newX] == GridCell.Agent))
        {
            mHeroPosition = newPosition;
            mHeroObject.transform.position = transform.position + new Vector3((mHeroPosition.x + 0.5f) * mBlockSize, (mHeroPosition.y + 0.5f) * mBlockSize, 0f);
            return true;
        }
        return false;
    }

    public Vector2Int GetAgentPosition()
    {
        return mAgentPosition;
    }

    public Quaternion GetAgentRotation()
    {
        return mAgentObject.transform.rotation;
    }

    public Vector2Int GetTargetPosition()
    {
        return mTargetPosition;
    }

    public void SetAgentRotation(Quaternion rotation)
    {
        mAgentObject.transform.rotation = rotation;
    }

    public void AgentShoot()
    {
        Instantiate(bullet, mAgentObject.transform.position, mAgentObject.transform.rotation * Quaternion.Euler(0, 0, -90));
    }

    public void HeroShoot()
    {
        Instantiate(bullet, mHeroObject.transform.position, mHeroObject.transform.rotation * Quaternion.Euler(0, 0, -90));
    }

    public Vector3 GetAgentPositionV3()
    {
        return mAgentObject.transform.position;
    }
}
