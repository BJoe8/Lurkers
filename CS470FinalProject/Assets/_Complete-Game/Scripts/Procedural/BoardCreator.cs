using System.Collections;
using UnityEngine;
using Completed;


public class BoardCreator : MonoBehaviour
{
	// The type of tile that will be laid in a specific position.
	public enum TileType
	{
		Wall, Floor, Enemy
	}

	public static BoardCreator instance = null;
	public int columns = 100;                                 // The number of columns on the board (how wide it will be).
	public int rows = 100;                                    // The number of rows on the board (how tall it will be).
	private bool spawnedGoal = false;       //Flag for if the goal has been spawned 
	private bool spawnedPlayer = false;     //Flag for if the player has been spawned
    private bool spawnedEnemyK = false;     //Flag for if an enemy with a key has been spawned
    private int bossLevelNum = 5;           //Indicates when a boss level should be

	public IntRange potionNum = new IntRange(5,10);			//The range of the number of red potions in the game.
	public IntRange chestChance = new IntRange(0,100);      //A range for the chance of there being a chest on the level
    public IntRange lanternChance = new IntRange(0, 100);   //A range for the chance of there being a lantern on the level
	public IntRange chestNum;   //How many chests there are
	private int maxChests;  //How many chests there can be
    private int maxLantern; //How many lanterns there can be
	public IntRange numRooms = new IntRange (4,8);	          // The range of the number of rooms there can be.
	public IntRange roomWidth = new IntRange (3, 10);         // The range of widths rooms can have.
	public IntRange roomHeight = new IntRange (3, 10);        // The range of heights rooms can have.
	public IntRange corridorLength = new IntRange (6, 10);    // The range of lengths corridors between rooms can have.
	public IntRange enemyCount = new IntRange (3, 7);		  // The range of enemies from minimum to maximum count per level.
	public IntRange gemCount = new IntRange (1, 5);			  // Lower and upper limit for our random number of food items per level.
	private int maxEnemies;									  // Uses enemyCount to generate a random value. The current level acts a multiplier.
	private int currentLevel;								  // Fetches the current level from GameManger.instance.level
	private int maxNumOfPotions;							  //Number of potions

	public GameObject[] floorTiles;                           // An array of floor tile prefabs.
	public GameObject[] wallTiles;                            // An array of wall tile prefabs.
	public GameObject[] enemyTiles;							  // An array of enemy prefabs.
    public GameObject[] enemyKTiles;                          // An array of enemy prefabs who have the key
	public GameObject[] outerWallTiles;                       // An array of outer wall tile prefabs.
	public GameObject[] potions;					          // Array of Red Potion (Pre-existing)

	public GameObject player;								  // Prefab of Player(Pre-existing)
	public GameObject key;									  // Prefab of Key (Pre-existing)
	public GameObject exit;									  // Prefab of Objective (Pre-existing)
    public GameObject boss;                                   //Prefab of Boss (Pre-existing)
    public GameObject chest;                                  //Prefab of chest (Pre-existing)
    public GameObject lantern;                                //Prefab of lantern (Pre-existing)

	private Vector3 playerPos;								  // Player position tracking - For use for safe-buffer spawning
	private Vector3 objectivePos;							  // Objective position tracking - For longest distance from player or last chance spawning (Ran out of room to spawn, so spawn anyways)
	private TileType[][] tiles;                               // A jagged array of tile types representing the board, like a grid.
	private Room[] rooms;                                     // All the rooms that are created for this board.
	private Corridor[] corridors;                             // All the corridors that connect the rooms.
	private GameObject boardHolder;                           // GameObject that acts as a container for all other tiles.
	private int spawnBufferSize = 2;						  // Size of the spawn buffer the player has where an enemy cannot spawn in
    public bool isBossLevel = false;                          // Flag for if it is a boss level

    //Setup the first level
	public void SetupScene(){
		boardHolder = new GameObject ("BoardHolder");   //Create a boardHolder
		currentLevel = GameManager.instance.level;  //Get the current level(1)
		chestNum = new IntRange (1, currentLevel);  //Get at least one chest
		maxChests = chestNum.Random;    //Get a random number for the maximum number of chests
        maxLantern = 1; //Have 1 lantern
		maxEnemies = enemyCount.Random * currentLevel;  //The max number of enemies for the level
		maxNumOfPotions = potionNum.Random; //Get a random number of potions
        
        ////////////////////////////////////
        //
        //  Setup the board
        //  Includes:
        //      Creating rooms/corridors
        //      Spawning player/exit
        //      Spawning enemies
        //      Spawning items
        //
        ////////////////////////////////////
		SetupTilesArray ();
        
		CreateRoomsAndCorridors ();

		SetTilesValuesForRooms ();
		SetTilesValuesForCorridors ();

		InstantiateTiles ();
		InstantiateOuterWalls ();
	}

    //Setup the board for any level following the first
    public void SetupNextScene()
    {
        currentLevel = GameManager.instance.level;  //Get the current level
		chestNum = new IntRange (1, currentLevel);  //Get the number of chests
		maxChests = chestNum.Random;    //Get a random number for the max
        maxLantern = 1; //Set the number of lanterns
        maxEnemies = enemyCount.Random * currentLevel;
		maxNumOfPotions = potionNum.Random;

        ////////////////////////////////////
        //
        //  Setup the board
        //  Includes:
        //      Creating rooms/corridors
        //      Spawning player/exit
        //      Spawning enemies
        //      Spawning items
        //
        ////////////////////////////////////
        SetupTilesArray();

        CreateRoomsAndCorridors();

        SetTilesValuesForRooms();
        SetTilesValuesForCorridors();

        InstantiateTiles();
        InstantiateOuterWalls();
    }

    //Clear the objects from the board to get ready for the next level
	public void clearBoard(){
        GameObject[] temp = GameObject.FindGameObjectsWithTag("OuterWall");
        for (int i = 0; i < temp.Length; i++)
            Destroy(temp[i]);
        temp = GameObject.FindGameObjectsWithTag("Floor");
        for (int i = 0; i < temp.Length; i++)
            Destroy(temp[i]);
        temp = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < temp.Length; i++)
            Destroy(temp[i]);
			temp = GameObject.FindGameObjectsWithTag("Reverse");
		for (int i = 0; i < temp.Length; i++)
		    Destroy(temp[i]);
		temp = GameObject.FindGameObjectsWithTag("Heal");
		for (int i = 0; i < temp.Length; i++)
			Destroy(temp[i]);
		temp = GameObject.FindGameObjectsWithTag("Damage");
		for (int i = 0; i < temp.Length; i++)
				Destroy(temp[i]);
		temp = GameObject.FindGameObjectsWithTag("Random");
		for (int i = 0; i < temp.Length; i++)
				Destroy(temp[i]);
		temp = GameObject.FindGameObjectsWithTag("IncreaseAttack");
		for (int i = 0; i < temp.Length; i++)
				Destroy(temp[i]);
		temp = GameObject.FindGameObjectsWithTag("DecreaseAttack");
		for (int i = 0; i < temp.Length; i++)
				Destroy(temp[i]);
		temp = GameObject.FindGameObjectsWithTag("Chest");
		for (int i = 0; i < temp.Length; i++)
				Destroy(temp[i]);
		temp = GameObject.FindGameObjectsWithTag("Item");
		for (int i = 0; i < temp.Length; i++)
			Destroy(temp[i]);
        temp = GameObject.FindGameObjectsWithTag("Lantern");
        for (int i = 0; i < temp.Length; i++)
            Destroy(temp[i]);

        //Reset the room size
        roomWidth.SetMax(10);
        roomWidth.SetMin(3);
        roomHeight.SetMax(10);
        roomHeight.SetMin(3);
        
        //Reset flags
        spawnedPlayer = false;
		spawnedGoal = false;
        spawnedEnemyK = false;
	}

	void SetupTilesArray ()
	{
		// Set the tiles jagged array to the correct width.
		tiles = new TileType[columns][];

		// Go through all the tile arrays...
		for (int i = 0; i < tiles.Length; i++)
		{
			// ... and set each tile array is the correct height.
			tiles[i] = new TileType[rows];
		}
	}


	void CreateRoomsAndCorridors ()
	{
        //If it needs to be a boss level then create one large room with the boss
        if( currentLevel % bossLevelNum == 0) {
            this.isBossLevel = true;
            rooms = new Room[1];            
            //Set room size
            roomWidth.SetMax(30);
            roomWidth.SetMin(30);
            roomHeight.SetMax(30);
            roomHeight.SetMin(30);
            rooms[0] = new Room();
            //Create room
            rooms[0].SetupRoom(roomWidth, roomHeight, columns, rows);
            corridors = null;   //Don't have any corridors
            //Spawn objects
            SpawnPlayer(0, spawnedPlayer);
            SpawnGoal(0, spawnedGoal, spawnedPlayer, isBossLevel);
            SpawnBoss();

        }
        //Otherwise create a normal level
        else
        {
            // Create the rooms array with a random size.

            //set boolean value to define that current level is not boss level in order to render room accordingly
            this.isBossLevel = false;

            rooms = new Room[numRooms.Random]; 

            // There should be one less corridor than there is rooms.
            corridors = new Corridor[rooms.Length - 1];

            // Create the first room and corridor.
            rooms[0] = new Room();
            corridors[0] = new Corridor();

            // Setup the first room, there is no previous corridor so we do not use one.
            rooms[0].SetupRoom(roomWidth, roomHeight, columns, rows);

            // Setup the first corridor using the first room.
            corridors[0].SetupCorridor(rooms[0], corridorLength, roomWidth, roomHeight, columns, rows, true);
        }

		for (int i = 1; i < rooms.Length; i++)
		{
			// Create a room.
			rooms[i] = new Room ();

			// Setup the room based on the previous corridor.
			rooms[i].SetupRoom (roomWidth, roomHeight, columns, rows, corridors[i - 1]);

			// If we haven't reached the end of the corridors array...
			if (i < corridors.Length)
			{
				// ... create a corridor.
				corridors[i] = new Corridor ();

				// Setup the corridor based on the room that was just created.
				corridors[i].SetupCorridor(rooms[i], corridorLength, roomWidth, roomHeight, columns, rows, false);
			} // End if

            /*
			 *							PLAYER SPAWN
			*/
            SpawnPlayer(i, spawnedPlayer);

			/*
			 *							CHEST SPAWN
			*/
			if (chestChance.Random > 0.55) {
				SpawnChest (i);
			}

            if(lanternChance.Random > 0.55) {

                if (maxLantern > 0)
                {
                    Vector3 lanternPos = new Vector3(rooms[i].xPos + 2, rooms[i].yPos, 0);
                    Instantiate(lantern, lanternPos, Quaternion.identity);
                    maxLantern--;
                }
            }

            /*
			 *							ENEMY SPAWN
			*/
			GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
            if (spawnedPlayer && !spawnedEnemyK && !isBossLevel)
            {
                if (!IsInRange(playerPos.x, playerPos.x + spawnBufferSize, rooms[i].xPos) && !IsInRange(playerPos.x - spawnBufferSize, playerPos.x, rooms[i].xPos))
                {
                    // Check to see if potential enemy spawn is within y tiles of the player's up and down position
                    if (!IsInRange(playerPos.y, playerPos.y + spawnBufferSize, rooms[i].yPos) && !IsInRange(playerPos.y - spawnBufferSize, playerPos.y, rooms[i].yPos))
                    {
                        Vector3 enemyPos = new Vector3(rooms[i].xPos, rooms[i].yPos, 0);
                        Instantiate(enemyKTiles[enemyCount.Random % 2], enemyPos, Quaternion.identity);
                        maxEnemies--;
                        spawnedEnemyK = true;
                    }
                }
            }
			
			
            else if (enemyCount.Random > 0.77 && maxEnemies > 0 && spawnedPlayer && spawnedEnemyK)
            {
                // Check to see if potential enemy spawn is within x tiles of the player's left and right position
                if (!IsInRange(playerPos.x, playerPos.x + spawnBufferSize, rooms[i].xPos) && !IsInRange(playerPos.x - spawnBufferSize, playerPos.x, rooms[i].xPos))
                {
                    // Check to see if potential enemy spawn is within y tiles of the player's up and down position
                    if (!IsInRange(playerPos.y, playerPos.y + spawnBufferSize, rooms[i].yPos) && !IsInRange(playerPos.y - spawnBufferSize, playerPos.y, rooms[i].yPos))
                    {
						// Check to see if the square is already occupied by an enemy. If it is, we don't populate it
						bool isOccupied = false;
						if(enemyArray.Length > 0) {
							for(int j = 0; j < enemyArray.Length; j++) {
								if(enemyArray[j].transform.position.x ==rooms[i].xPos && enemyArray[j].transform.position.y == rooms[i].yPos) {
									isOccupied = true;
									break;
								}
							}
						}
						if(!isOccupied) {
							Vector3 enemyPos = new Vector3(rooms[i].xPos, rooms[i].yPos, 0);
							Instantiate(enemyTiles[enemyCount.Random % 2], enemyPos, Quaternion.identity);
						}
                        if (maxNumOfPotions > 0)
                        {
                            Vector3 potionPos = new Vector3(rooms[i].xPos + 2, rooms[i].yPos, 0);
                            int randomIndex = Random.Range(0, potions.Length);
                            Instantiate(potions[randomIndex], potionPos, Quaternion.identity);
                            maxNumOfPotions--;
                        }

                        maxEnemies--;
                    }
                }
            }// End Enemy Spawn
            /*
			 *							OBJECTIVE SPAWN
			*/
            SpawnGoal(i, spawnedGoal, spawnedPlayer, isBossLevel);

		} // End For
        while(!spawnedEnemyK && !isBossLevel)
        {
            for (int i = 1; i < rooms.Length; i++)
            {
                if (!IsInRange(playerPos.x, playerPos.x + spawnBufferSize, rooms[i].xPos) && !IsInRange(playerPos.x - spawnBufferSize, playerPos.x, rooms[i].xPos))
                {
                    // Check to see if potential enemy spawn is within y tiles of the player's up and down position
                    if (!IsInRange(playerPos.y, playerPos.y + spawnBufferSize, rooms[i].yPos) && !IsInRange(playerPos.y - spawnBufferSize, playerPos.y, rooms[i].yPos))
                    {
                        Vector3 enemyPos = new Vector3(rooms[i].xPos, rooms[i].yPos, 0);
                        Instantiate(enemyKTiles[enemyCount.Random % 2], enemyPos, Quaternion.identity);
                        maxEnemies--;
                        spawnedEnemyK = true;
                        break;
                    }
                }
            }
        }
				//Spawning potions
	}

    void SpawnPlayer(int i, bool spawnedPlayer) {
        
        if (i == Mathf.Floor(rooms.Length * 0.5f) && !spawnedPlayer)
        {
            // Log player positon and set spawnedPlayer to true
            playerPos = new Vector3(rooms[i].xPos, rooms[i].yPos, 0);
            this.spawnedPlayer = true;

            if (maxNumOfPotions > 0)
            {
                Vector3 potionPos = new Vector3(rooms[i].xPos + 2, rooms[i].yPos, 0);
                int randomIndex = Random.Range(0, potions.Length);
                Instantiate(potions[randomIndex], potionPos, Quaternion.identity);
                maxNumOfPotions--;
            }

            // If the player is already spawned, relocate player
            if (GameManager.instance.level > 1)
            {
                player.transform.position = new Vector3(rooms[i].xPos, rooms[i].yPos, 0);
                Debug.Log(this.isBossLevel);
                if (this.isBossLevel)
                {
                    Camera.main.GetComponent<ScreenTransitionImageEffect>().maskValue = 0f;
                }
                else if(!this.isBossLevel || (Camera.main.GetComponent<ScreenTransitionImageEffect>().maskValue == 0f))
                {
                    Camera.main.GetComponent<ScreenTransitionImageEffect>().maskValue = .8f;
                }
                Player.updating = false;
            }
            // Else, player needs to be instantiated
            else
            {
                player = Instantiate(player, playerPos, Quaternion.identity);
                if (this.isBossLevel)
                {
                    Camera.main.GetComponent<ScreenTransitionImageEffect>().maskValue = 0f;
                } else if(!this.isBossLevel || (Camera.main.GetComponent<ScreenTransitionImageEffect>().maskValue == 0f)){
                    Camera.main.GetComponent<ScreenTransitionImageEffect>().maskValue = .8f;
                }
            }
        } // End Player Spawn

    }

	void SpawnChest(int i){
		if (maxChests > 0) {
			// Check to see if potential enemy spawn is within x tiles of the player's left and right position
			if (!IsInRange (playerPos.x, playerPos.x + spawnBufferSize, rooms [i].xPos) && !IsInRange (playerPos.x - spawnBufferSize, playerPos.x, rooms [i].xPos)) {
				// Check to see if potential enemy spawn is within y tiles of the player's up and down position
				if (!IsInRange (playerPos.y, playerPos.y + spawnBufferSize, rooms [i].yPos) && !IsInRange (playerPos.y - spawnBufferSize, playerPos.y, rooms [i].yPos)) {
					Vector3 chestPos = new Vector3 (rooms [i].xPos, rooms [i].yPos, 0);
					Instantiate (chest, chestPos, Quaternion.identity);
					maxChests--;
				}
			}	
		}
	}

    void SpawnBoss() {
        Vector3 bossPos = new Vector3(45, 45, 0);
        Instantiate(boss, bossPos, Quaternion.identity);
    }

    void SpawnGoal(int i, bool spawnedGoal, bool spawnedPlayer, bool isBossLevel) {
        
        //if (!spawnedGoal && spawnedPlayer)
        if(!spawnedGoal && spawnedPlayer && isBossLevel) {
            
            Vector3 objectivePos = new Vector3(64, 64, 0);
            // If exit is already spawned, relocate it
            if (currentLevel > 1)
                exit.transform.position = objectivePos;
            // Else, exit needs to be instantiated
            else
                exit = Instantiate(exit, objectivePos, Quaternion.identity);
            this.spawnedGoal = true;
        } else if(!spawnedGoal && spawnedPlayer && !isBossLevel)
        {
            Vector3 objectivePos = new Vector3(rooms[i].xPos, rooms[i].yPos, 0);
            // Check to see if potential objective spawn is within x + 10 tiles of the player's left and right position
            if (!IsInRange(playerPos.x, playerPos.x + spawnBufferSize + 10, rooms[i].xPos) && !IsInRange(playerPos.x - spawnBufferSize - 10, playerPos.x, rooms[i].xPos))
            {
                // Check to see if potential objective spawn is within y + 10 tiles of the player's up and down position
                if (!IsInRange(playerPos.y, playerPos.y + spawnBufferSize + 10, rooms[i].yPos) && !IsInRange(playerPos.y - spawnBufferSize - 10, playerPos.y, rooms[i].yPos))
                {
                    // If exit is already spawned, relocate it
                    if (currentLevel > 1)
                        exit.transform.position = new Vector3(rooms[i].xPos, rooms[i].yPos, 0);
                    // Else, exit needs to be instantiated
                    else
                        exit = Instantiate(exit, objectivePos, Quaternion.identity);
                    this.spawnedGoal = true;
                }
            }
            /*
             *              LAST DITCH EFFORT - OBJECTIVE SPAWN
             */
            if (i == rooms.Length - 1 && !spawnedGoal)
            {
                if (currentLevel > 1)
                    exit.transform.position = new Vector3(rooms[i].xPos, rooms[i].yPos, 0);
                else
                    exit = Instantiate(exit, objectivePos, Quaternion.identity);
                this.spawnedGoal = true;
            }

        } // End Objective Spawn
    }

	void SetTilesValuesForRooms ()
	{
		// Go through all the rooms...
		for (int i = 0; i < rooms.Length; i++)
		{
			Room currentRoom = rooms[i];

			// ... and for each room go through it's width.
			for (int j = 0; j < currentRoom.roomWidth; j++)
			{
				int xCoord = currentRoom.xPos + j;

				// For each horizontal tile, go up vertically through the room's height.
				for (int k = 0; k < currentRoom.roomHeight; k++)
				{
					int yCoord = currentRoom.yPos + k;

					// The coordinates in the jagged array are based on the room's position and it's width and height.

					tiles[xCoord][yCoord] = TileType.Floor;
				}
			}
		}
	}


	void SetTilesValuesForCorridors ()
	{
        // Go through every corridor...
        if (corridors != null)
        {

            for (int i = 0; i < corridors.Length; i++)
            {
                Corridor currentCorridor = corridors[i];

                // and go through it's length.
                for (int j = 0; j < currentCorridor.corridorLength; j++)
                {
                    // Start the coordinates at the start of the corridor.
                    int xCoord = currentCorridor.startXPos;
                    int yCoord = currentCorridor.startYPos;

                    // Depending on the direction, add or subtract from the appropriate
                    // coordinate based on how far through the length the loop is.
                    switch (currentCorridor.direction)
                    {
                        case Direction.North:
                            yCoord += j;
                            break;
                        case Direction.East:
                            xCoord += j;
                            break;
                        case Direction.South:
                            yCoord -= j;
                            break;
                        case Direction.West:
                            xCoord -= j;
                            break;
                    }

                    // Set the tile at these coordinates to Floor.
                    tiles[xCoord][yCoord] = TileType.Floor;
                }
            }
        }
	}


	void InstantiateTiles ()
	{
		// Go through all the tiles in the jagged array...
		for (int i = 0; i < tiles.Length; i++)
		{
			for (int j = 0; j < tiles[i].Length; j++)
			{
				// ... and instantiate a floor tile for it.
				if (tiles[i][j] == TileType.Floor) {
					InstantiateFromArray (floorTiles, i, j);
				}


				// If the tile type is Wall...
				if (tiles[i][j] == TileType.Wall)
				{
					// ... instantiate a wall over the top.
					InstantiateFromArray (wallTiles, i, j);
				}
			}
		}
	}


	void InstantiateOuterWalls ()
	{
		// The outer walls are one unit left, right, up and down from the board.
		float leftEdgeX = -1f;
		float rightEdgeX = columns + 0f;
		float bottomEdgeY = -1f;
		float topEdgeY = rows + 0f;

		// Instantiate both vertical walls (one on each side).
		InstantiateVerticalOuterWall (leftEdgeX, bottomEdgeY, topEdgeY);
		InstantiateVerticalOuterWall(rightEdgeX, bottomEdgeY, topEdgeY);

		// Instantiate both horizontal walls, these are one in left and right from the outer walls.
		InstantiateHorizontalOuterWall(leftEdgeX + 1f, rightEdgeX - 1f, bottomEdgeY);
		InstantiateHorizontalOuterWall(leftEdgeX + 1f, rightEdgeX - 1f, topEdgeY);
	}


	void InstantiateVerticalOuterWall (float xCoord, float startingY, float endingY)
	{
		// Start the loop at the starting value for Y.
		float currentY = startingY;

		// While the value for Y is less than the end value...
		while (currentY <= endingY)
		{
			// ... instantiate an outer wall tile at the x coordinate and the current y coordinate.
			InstantiateFromArray(outerWallTiles, xCoord, currentY);

			currentY++;
		}
	}


	void InstantiateHorizontalOuterWall (float startingX, float endingX, float yCoord)
	{
		// Start the loop at the starting value for X.
		float currentX = startingX;

		// While the value for X is less than the end value...
		while (currentX <= endingX)
		{
			// ... instantiate an outer wall tile at the y coordinate and the current x coordinate.
			InstantiateFromArray (outerWallTiles, currentX, yCoord);

			currentX++;
		}
	}


	void InstantiateFromArray (GameObject[] prefabs, float xCoord, float yCoord)
	{
		// Create a random index for the array.
		int randomIndex = Random.Range(0, prefabs.Length);

		// The position to be instantiated at is based on the coordinates.
		Vector3 position = new Vector3(xCoord, yCoord, 0f);

		// Create an instance of the prefab from the random index of the array.
		GameObject tileInstance = Instantiate(prefabs[randomIndex], position, Quaternion.identity) as GameObject;

		// Set the tile's parent to the board holder.
		tileInstance.transform.parent = boardHolder.transform;
	}

	// Returns true if a value x is within the range of a lower and upper bound
	bool IsInRange(int low, int high, int x){
		return (low <= x && x <= high);
	}

	bool IsInRange(double low, double high, double x){
		return (low <= x && x <= high);
	}
}
