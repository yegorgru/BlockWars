using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockType
{
    public GameObject prefab;
    //public Color color;
}

public class MapGenerator : MonoBehaviour
{
    public BlockType[] blockTypes;
    public int rows = 10;
    public int columns = 10;
    public float blockSize = 1f;
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
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject prefab = (col == 0 || col == columns - 1) && row == rows / 2 ? 
                    character :
                    blockTypes[Random.Range(0, blockTypes.Length)].prefab;
                Vector3 position = new Vector3((col + 0.5f) * blockSize, (row + 0.5f) * blockSize, 0f);
                GameObject newBlock = Instantiate(prefab, transform.position + position, Quaternion.identity, this.transform);
                newBlock.transform.parent = transform;
                // SpriteRenderer spriteRenderer = newBlock.GetComponent<SpriteRenderer>();
                /*if (spriteRenderer != null)
                {
                    spriteRenderer.color = selectedBlockType.color;
                }*/
            }
        }
    }
}
