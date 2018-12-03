using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Completed
{
	using System.Collections.Generic;		//Allows us to use Lists.
	using UnityEngine.UI;					//Allows us to use UI.

	public class GameManager : MonoBehaviour
	{
		public float levelStartDelay = 2f;						//Time to wait before starting level, in seconds.
		public float turnDelay = 0.1f;							//Delay between each Player turn.
		public int playerFoodPoints = 100;						//Starting value for Player food points.
		public static GameManager instance = null;				//Static instance of GameManager which allows it to be accessed by any other script.
		[HideInInspector] public bool playersTurn = true;		//Boolean to check if it's players turn, hidden in inspector but public.


		private Text levelText;									//Text to display current level number.
		private GameObject levelImage;                          //Image to block out level as levels are being set up, background for levelText.
        public GameObject traderImage;                          //Trader screen
        //private BoardManager boardScript;						//Store a reference to our BoardManager which will set up the level.
		public BoardCreator boardScript;                        //Store a reference to our BoardCreator which will set up the level.
        public Trader tradeScript;                              //The script for the trader (Pre-existing)
        public int level = 1;									//Current level number, expressed in game as "Day 1".
		private List<Enemy> enemies;							//List of all Enemy units, used to issue them move commands.
        private List<Boss> boss;                                //Bosses
        private int traderLevelNum = 4;                         //Number of levels for the trader to appear

		private bool enemiesMoving;								//Boolean to check if enemies are moving.                              
		private bool doingSetup = true;							//Boolean to check if we're setting up board, prevent Player from moving during setup.
		public bool fastEnemy = false;							//Boolean to check if fast enemies are enabled (Through player attack and/or current level)
		public bool smartEnemy = false;							//Boolean to check if smart enemies are nealbed (Through player attack and/or current level)



		//Awake is always called before any Start functions
		void Awake()
		{
            //Check if instance already exists
            if (instance == null)

                //if not, set instance to this
                instance = this;

            //If instance already exists and it's not this:
            else if (instance != this)

                //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
                Destroy(gameObject);

			//Sets this to not be destroyed when reloading scene
			DontDestroyOnLoad(gameObject);

            boardScript = this.GetComponent<BoardCreator>();
            //Get a component reference to the attached BoardManager script
            // Loaded directly via Inspector
            //boardScript = GetComponent<BoardManager>();

            //Assign enemies to a new List of Enemy objects.\
                enemies = new List<Enemy>();
                boss = new List<Boss>();

			//Call the InitGame function to initialize the first level
			InitGame();
		}

        //this is called only once, and the paramter tell it to be called only after the scene was loaded
        //(otherwise, our Scene Load callback would be called the very first load, and we don't want that)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static public void CallbackInitialization()
        {
            //register the callback to be called everytime the scene is loaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        //This is called each time a scene is loaded.
        static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            //instance.level++;
			//instance.boardScript.clearBoard ();
            //instance.InitGame();
        }

        //Go to the next level
        public void NextLevel()
        {
            instance.level++;   //Increase the level
            //Set levelImage to active blocking player's view of the game board during setup.
            levelImage.SetActive(true); //Show level image
            boardScript.clearBoard();   //Clear the board
			AdaptAI ();     //Set AI to be better
            Invoke("InitNextLevel", levelStartDelay);   //Start next level after a short time
        }


		//Initializes the game for each level.
		void InitGame()
		{
			//While doingSetup is true the player can't move, prevent player from moving while title card is up.
			doingSetup = true;

			//Get a reference to our image LevelImage by finding it by name.
			levelImage = GameObject.Find("LevelImage");

			//Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
			levelText = GameObject.Find("LevelText").GetComponent<Text>();

			//Set the text of levelText to the string "Day" and append the current level number.
			levelText.text = "Level " + level;

			//Set levelImage to active blocking player's view of the game board during setup.
			levelImage.SetActive(true);

			//Call the HideLevelImage function with a delay in seconds of levelStartDelay.
			Invoke("HideLevelImage", levelStartDelay);

			//Clear any Enemy objects in our List to prepare for next level.
			//enemies.Clear();

			//Call the SetupScene function of the BoardManager script, pass it current level number.
			boardScript.SetupScene();
		}

        //Setup the next level
        void InitNextLevel()
        {
            //While doingSetup is true the player can't move, prevent player from moving while title card is up.
            doingSetup = true;

            //Get a reference to our image LevelImage by finding it by name.
            levelImage = GameObject.Find("LevelImage");

            //Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
            levelText = GameObject.Find("LevelText").GetComponent<Text>();

            //Set the text of levelText to the string "Day" and append the current level number.
            levelText.text = "Level " + level;

            //Set levelImage to active blocking player's view of the game board during setup.
            levelImage.SetActive(true);

            //Call the HideLevelImage function with a delay in seconds of levelStartDelay.
            Invoke("HideLevelImage", levelStartDelay);

            //Clear any Enemy objects in our List to prepare for next level.
            enemies.Clear();

            //Open the trader if it is the correct level
            if(level % traderLevelNum == 0)
            {
                Invoke("OpenTrader", levelStartDelay);
            }
            //Create the board
            boardScript.SetupNextScene();
        }

        //Open the trader
        void OpenTrader()
        {
            traderImage.SetActive(true);
            tradeScript.UpdateOptions();
        }

		//Hides black image used between levels
		void HideLevelImage()
		{
			//Disable the levelImage gameObject.
			levelImage.SetActive(false);

			//Set doingSetup to false allowing player to move again.
			doingSetup = false;
		}

		//Update is called every frame.
		void Update()
		{
			//Check that playersTurn or enemiesMoving or doingSetup are not currently true.
			if(playersTurn || enemiesMoving || doingSetup)

				//If any of these are true, return and do not start MoveEnemies.
				return;

			//Start moving enemies.
            if(!this.boardScript.isBossLevel) {
                StartCoroutine(MoveEnemies());
            } else {
                StartCoroutine(MoveBoss());
            }
		}

		//Call this to add the passed in Enemy to the List of Enemy objects.
		public void AddEnemyToList(Enemy script)
		{
			//Add Enemy to List enemies.
			enemies.Add(script);
		}

        public void AddEnemyToList(Boss script) {
            //Add Boss to List boss.
            Debug.Log(this.boss.Count);
            boss.Add(script);
        }

        public void RemoveEnemyFromList(Enemy script)
        {
            //Remove Enemy from List enemies
            enemies.Remove(script);
        }

        public void RemoveBossFromList(Boss script)
        {
            boss.Remove(script);
        }

        //GameOver is called when the player reaches 0 food points
        public void GameOver()
		{
			//Set levelText to display number of levels passed and game over message
			levelText.text = "You survived " + level + " levels.";

			//Enable black background image gameObject.
			levelImage.SetActive(true);
            
            //Returns to Main Menu
            StartCoroutine(Quit());
            
        }

        //Coroutine to move enemies in sequence.
        IEnumerator MoveEnemies()
		{
            //While enemiesMoving is true player is unable to move.
			enemiesMoving = true;

			//Wait for turnDelay seconds, defaults to .1 (100 ms).
			yield return new WaitForSeconds(turnDelay);

			//If there are no enemies spawned (IE in first level):
			if (enemies.Count == 0)
			{
				//Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
                //Not Needed
				//yield return new WaitForSeconds(turnDelay);
			}

			//Loop through List of Enemy objects.
			for (int i = 0; i < enemies.Count; i++)
			{
				//Call the MoveEnemy function of Enemy at index i in the enemies List.
				enemies[i].MoveEnemy();

				//Wait for Enemy's moveTime before moving next Enemy,
                //Not Needed
				//yield return new WaitForSeconds(enemies[i].moveTime);
			}
			//Once Enemies are done moving, set playersTurn to true so player can move.
			playersTurn = true;

			//Enemies are done moving, set enemiesMoving to false.
			enemiesMoving = false;
		}

        IEnumerator MoveBoss() {
            enemiesMoving = true;

            //Wait for turnDelay seconds, defaults to .1 (100 ms).
            yield return new WaitForSeconds(turnDelay);

            //If there are no enemies spawned (IE in first level):
            if (boss.Count == 0)
            {
                //Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
                //Not Needed
                //yield return new WaitForSeconds(turnDelay);
            }

            //Loop through List of Enemy objects.
            for (int i = 0; i < boss.Count; i++)
            {
                //Call the MoveEnemy function of Enemy at index i in the enemies List.
                boss[i].MoveEnemy();

                //Wait for Enemy's moveTime before moving next Enemy,
                //Not Needed
                //yield return new WaitForSeconds(enemies[i].moveTime);
            }
            //Once Enemies are done moving, set playersTurn to true so player can move.
            playersTurn = true;

            //Enemies are done moving, set enemiesMoving to false.
            enemiesMoving = false;
        }


        //Waits a certain amount of time before returning to the main menu.
        public IEnumerator Quit()
        {
            yield return new WaitForSeconds(5);
            //Disable this GameManager.
            enabled = false;
            instance = null;
            SoundManager.instance = null;
            Player.instance = null;
            SceneManager.LoadScene(0);
        }

        //Update the AI
		public void AdaptAI()
		{
			if (level >= 2) {
				GameManager.instance.smartEnemy = true;
				GameManager.instance.fastEnemy = true;
			}
		}

        //Disable this GameManager
        public void Disable()
        {
            enabled = false;
            instance = null;
            SoundManager.instance = null;
            Player.instance = null;
        }
    }
}
