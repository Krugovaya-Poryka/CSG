using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridBehavior : MonoBehaviour
{
	public int seed = 123;
	public float obstacleChance = 0.2f;
	public GridMover mover;
	public bool findDistance = false;
	public int rows = 10;
	public int columns = 10;
	public int scale = 1;
	public GameObject gridPrefab;
	public Vector2 leftBottomLocation = new Vector2(0, 0);
	public GameObject[,] gridArray;
	public int startX = 0;
	public int startY = 0;
	public int endX = 2;
	public int endY = 2;
	public List<GameObject> path = new List<GameObject>();
	
    void Awake()
    {
	    gridArray = new GameObject[columns, rows];
	    if (gridPrefab)
		    GenerateGrid();
	    else print("No grid prefab found");
    }

    void Update()
    {
	    if (findDistance)
	    {
		    SetDistance();
		    SetPath();
		    path.Reverse();
		    mover.StartMoving();
		    startX = endX;
		    startY = endY;
		    findDistance = false;
		    
	    }
    }
    void GenerateGrid()
    {
	    Random.InitState(seed);
	    for(int i = 0; i<columns; i++){
		    for(int j = 0; j<rows; j++)
		    {
			    GameObject obj = Instantiate(gridPrefab, new Vector2(leftBottomLocation.x + scale * i, leftBottomLocation.y + scale * j), Quaternion.identity);
			    obj.transform.SetParent(gameObject.transform);
			    obj.GetComponent<GridStat>().x = i;
			    obj.GetComponent<GridStat>().y = j;
			    float randomValue = Random.value;
			    if (Random.value < obstacleChance)
			    {
				    obj.GetComponent<GridStat>().walkable = false;
			    }
			    else
			    {
				    obj.GetComponent<GridStat>().walkable = true;
			    }
			    obj.GetComponent<GridStat>().UpdateText();
			    gridArray[i, j] = obj;
		    }
	    }
	    
    }

    void SetPath()
    {
	    int step;
	    int x = endX;
	    int y = endY;
	    List<GameObject> tempList = new List<GameObject>();
	    path.Clear();
	    if (gridArray[endX, endY] && gridArray[endX, endY].GetComponent<GridStat>().visited > 0 )
	    {
		    path.Add(gridArray[x, y]);
			    step = gridArray[x, y].GetComponent<GridStat>().visited;
	    }
	    else
	    {
		    print("Cant't find path");
		    return;
	    }

	    while (step > 0)
	    {
		    tempList.Clear();

		    if (TestDirections(x, y, step - 1, 1))
			    tempList.Add(gridArray[x, y + 1]);

		    if (TestDirections(x, y, step - 1, 2))
			    tempList.Add(gridArray[x, y - 1]);

		    if (TestDirections(x, y, step - 1, 3))
			    tempList.Add(gridArray[x + 1, y]);

		    if (TestDirections(x, y, step - 1, 4))
			    tempList.Add(gridArray[x - 1, y]);

		    GameObject tempObj = FindClosest(gridArray[x, y].transform, tempList);

		    path.Add(tempObj);

		    x = tempObj.GetComponent<GridStat>().x;
		    y = tempObj.GetComponent<GridStat>().y;

		    step--;
	    }

	    tempList.Clear();
    }

    void InitialSetUp()
    {
	    foreach (GameObject obj in gridArray)
	    {
		    obj.GetComponent<GridStat>().visited = -1;
	    }
	    gridArray[startX, startY].GetComponent<GridStat>().visited = 0;
    }

    bool TestDirections(int x, int y, int step, int direction)
    {
	    switch (direction)
	    {
		    case 4:
			    if(x-1 >= 0 && gridArray[x -1 , y] && gridArray[x - 1, y].GetComponent<GridStat>().visited == step && gridArray[x - 1, y].GetComponent<GridStat>().walkable)
				    return true;
			    else
				    return false;
		    case 3:
			    if(x+1<columns && gridArray[x + 1, y] && gridArray[x + 1, y].GetComponent<GridStat>().visited == step && gridArray[x + 1, y].GetComponent<GridStat>().walkable) 
				    return true;
			    else
				    return false;
		    case 2:
			    if(y-1 >= 0 && gridArray[x, y-1] && gridArray[x, y-1].GetComponent<GridStat>().visited == step && gridArray[x, y - 1].GetComponent<GridStat>().walkable)
				    return true;
			    else
				    return false;
		    case 1:
			    if(y+1<rows && gridArray[x, y+1] && gridArray[x, y+1].GetComponent<GridStat>().visited == step && gridArray[x, y + 1].GetComponent<GridStat>().walkable)
					return true;
			    else
					return false;
				    
			    
	    }
	    return false;
    }

    void SetDistance()
    {
	    InitialSetUp();
	    int x = startX;
	    int y = startY;
	    int[] textArray = new int[rows * columns];
	    for (int step = 1; step <= rows * columns; step++)
	    {
		    foreach (GameObject obj in gridArray)
		    {
			    if(obj && obj.GetComponent<GridStat>().visited == step - 1)
				    TestFourDirections(obj.GetComponent<GridStat>().x, obj.GetComponent<GridStat>().y, step);
		    }
	    }

    }

    void TestFourDirections(int x, int y, int step)
    {
	    if(TestDirections(x, y, -1, 1))
		    SetVisited(x, y + 1, step);
	    if(TestDirections(x, y, -1, 2))
		    SetVisited(x, y - 1, step);
	    if(TestDirections(x, y, -1, 3))
		    SetVisited(x + 1, y, step);
	    if(TestDirections(x, y, -1, 4))
		    SetVisited(x - 1, y, step);
    }

    void SetVisited(int x, int y, int step)
    {
	    if(gridArray[x,y])
		    gridArray[x,y].GetComponent<GridStat>().visited = step;
    }

    GameObject FindClosest(Transform targetLocation, List<GameObject> list)
    {
	    
	    if (list.Count == 0)
		    return null;
	    float CurrentDistance = scale*rows*columns;
	    int indexNumber = 0;
	    for (int i = 0; i < list.Count; i++)
	    {
		    if (Vector3.Distance(targetLocation.position, list[i].transform.position) < CurrentDistance)
		    {
			    CurrentDistance = Vector3.Distance(targetLocation.position, list[i].transform.position);
			    indexNumber = i;
		    }
	    }
	    return list[indexNumber];
		
    }
}
