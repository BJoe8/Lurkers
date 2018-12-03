using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;	//Allows us to use UI.
using UnityEngine.SceneManagement;

namespace Completed
{
	//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
	public class Player : MovingObject
	{
		public static Player instance = null;
		public float restartLevelDelay = 1f;		//Delay time in seconds to restart level.
        public float resetTextDelay = 2f;           //Delay time in seconds for the health text changes
		public int armorPoints = 0;                 //How much armoor the player has
        public int numGems = 10;                     //How many gems the player has
        public int pointsPerFood = 25;				//Number of points to add to player food points when picking up a food object.
		public int pointsPerSoda = 25;				//Number of points to add to player food points when picking up a soda object.
		public int pointsPerIncreaseAttack = 10;    //How many attack points gained
		public int pointsPerDecreaseAttack = 10;    //How many attack points lost
		public int wallDamage = 1;					//How much damage a player does to a wall when chopping it.
		public int attackDamage;                //How much damage a player does to an enemy
		public int chestDamage = 1;             //How much damage is needed to open a chest
		private int attackMod = 0;              //How much the attack has been modified
		private int defenseMod = 0;             //How much the defense has been modified
		private int durability = 99;            //The durability of the weapons
		public Text foodText;						//UI Text to display current player food total.
		public Text damageText;                     //UI Text to display the damage the player deals.
        public Text gemText;                    //Text for the number of gems in the inventory
		public Image heartContainer0;           //First heart
		public Image heartContainer1;           //Second heart
        public Image heartContainer2;           //Third heart
        public Image heartContainer3;           //Fourth heart
        public Image sword;                     //Sword image
		public Image shield;                    //Shield image
		public Image keyImage;                  //Key image
		public Sprite[] healthSprites;          //Hearts
		public Sprite keySprite;                //Key sprite
		public Sprite emptyKeySprite;
		public AudioClip moveSound1;				//1 of 2 Audio clips to play when player moves.
		public AudioClip moveSound2;				//2 of 2 Audio clips to play when player moves.
		public AudioClip eatSound1;					//1 of 2 Audio clips to play when player collects a food object.
		public AudioClip eatSound2;					//2 of 2 Audio clips to play when player collects a food object.
		public AudioClip drinkSound1;				//1 of 2 Audio clips to play when player collects a soda object.
		public AudioClip drinkSound2;				//2 of 2 Audio clips to play when player collects a soda object.
		public AudioClip gameOverSound;				//Audio clip to play when player dies.
        public static bool updating = false;

		public AudioClip weaponBreakSound;
		private Animator animator;					//Used to store a reference to the Player's animator component.
		private int health;                           //Used to store player food points total during level.
		private bool hasKey;                        //Determines if the player has the key to move on
		private int hasReversePotion;               //Flag to indicate if movements are reversed
		private Dictionary<string, Item> inventory;
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
#endif
		
		//Start overrides the Start function of MovingObject
		protected override void Start ()
		{
            //Check if there is already an instance of SoundManager
            if (instance == null)
                //if not, set it to this.
                instance = this;
            //If instance already exists:
            else if (instance != this)
                //Destroy this, this enforces our singleton pattern so there can only be one instance of SoundManager.
                Destroy(gameObject);

            //Get a component reference to the Player's animator component
            animator = GetComponent<Animator>();

			//Get the current food point total stored in GameManager.instance between levels.
			health = GameManager.instance.playerFoodPoints;

			//Set the foodText to reflect the current player food total.
			foodText = GameObject.Find ("Canvas/FoodText").GetComponent <UnityEngine.UI.Text> ();
			//foodText.text; = "Health: " + food;

            //Set the damage text
			damageText = GameObject.Find ("Canvas/DamageText").GetComponent <UnityEngine.UI.Text> ();
			damageText.text = "Attack: " + attackDamage;

            //Set up the heart system
			heartContainer0 = GameObject.Find ("Canvas/Lifebar/HeartContainer").GetComponent <UnityEngine.UI.Image> ();
			heartContainer1 = GameObject.Find ("Canvas/Lifebar/HeartContainer (1)").GetComponent <UnityEngine.UI.Image> ();
			heartContainer2 = GameObject.Find ("Canvas/Lifebar/HeartContainer (2)").GetComponent <UnityEngine.UI.Image> ();
			heartContainer3 = GameObject.Find ("Canvas/Lifebar/HeartContainer (3)").GetComponent <UnityEngine.UI.Image> ();

            //Get the key image
			keyImage = GameObject.Find ("Canvas/KeyHolder/Key").GetComponent <UnityEngine.UI.Image> ();

            //Add hearts to the heart system
			heartContainer0.sprite = healthSprites[2];
			heartContainer1.sprite = healthSprites[2];
			heartContainer2.sprite = healthSprites[2];
			heartContainer3.sprite = healthSprites[2];

            //Setup the inventory
			inventory = new Dictionary<string, Item> ();
			sword = GameObject.Find ("Canvas/Inventory/Sword").GetComponent <UnityEngine.UI.Image> ();
			shield = GameObject.Find ("Canvas/Inventory/Shield").GetComponent <UnityEngine.UI.Image> ();

			//Set the hasKey to false at the start of the level
			hasKey = false;
			keyImage.sprite = emptyKeySprite;
			hasReversePotion = -1;

            updating = false;

            numGems = 10;   //Give the player 10 gems to start
            gemText = GameObject.Find("Canvas/Inventory/Gem/GemCount").GetComponent<Text>();
            UpdateGemCount();

			//Call the Start function of the MovingObject base class.
			base.Start ();
		}


		//This function is called when the behaviour becomes disabled or inactive.
		// Throws an exception at kill time. (NULL REFERENCE)
		private void OnDisable ()
		{
            //When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
            if(GameManager.instance != null)
                GameManager.instance.playerFoodPoints = health;
		}


		private void Update ()
		{
			//If it's not the player's turn, exit the function.
			if(!GameManager.instance.playersTurn || updating) return;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Space Pressed");
				HitTextManager.Instance.CreateText(transform.position, "+" + pointsPerSoda, Color.green);
                GameManager.instance.playersTurn = false;
                return;
            }
            int horizontal = 0;  	//Used to store the horizontal move direction.
			int vertical = 0;		//Used to store the vertical move direction.

			if (hasReversePotion == 1) {
                //Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER

                //Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
                int previousPosition = horizontal;
			horizontal = - (int) (Input.GetAxisRaw ("Horizontal"));

			//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
			vertical = - (int) (Input.GetAxisRaw ("Vertical"));



                //Check if moving horizontally, if so set vertical to zero.
            if (horizontal != 0)
			{
				vertical = 0;
			}
			//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

			//Check if Input has registered more than zero touches
			if (Input.touchCount > 0)
			{
				//Store the first touch detected.
				Touch myTouch = Input.touches[0];

				//Check if the phase of that touch equals Began
				if (myTouch.phase == TouchPhase.Began)
				{
					//If so, set touchOrigin to the position of that touch
					touchOrigin = myTouch.position;
				}

				//If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
				else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
				{
					//Set touchEnd to equal the position of this touch
					Vector2 touchEnd = myTouch.position;

					//Calculate the difference between the beginning and end of the touch on the x axis.
					float x = touchEnd.x - touchOrigin.x;

					//Calculate the difference between the beginning and end of the touch on the y axis.
					float y = touchEnd.y - touchOrigin.y;

					//Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
					touchOrigin.x = -1;

					//Check if the difference along the x axis is greater than the difference along the y axis.
					if (Mathf.Abs(x) > Mathf.Abs(y))
						//If x is greater than zero, set horizontal to 1, otherwise set it to -1
						horizontal = x > 0 ? 1 : -1;
					else
						//If y is greater than zero, set horizontal to 1, otherwise set it to -1
						vertical = y > 0 ? 1 : -1;
				}
			}

#endif //End of mobile platform dependendent compilation section started above with #elif
			//Check if we have a non-zero value for horizontal or vertical
			if(horizontal != 0 || vertical != 0)
			{
                if((previousPosition - horizontal) >= 0) {
                    //print("in true");
                    spriteRenderer.flipX = true;
                } else {
                    //print("in false");
                    spriteRenderer.flipX = false;
                }

                //Call AttemptMove passing in the generic parameters Wall, Enemy, Chest, Boss
				//Pass in horizontal and vertical as parameters to specify the direction to move Player in.
				AttemptMove<Wall, Enemy, Chest, Boss> (horizontal, vertical, "");
            }
					}
					else {

						#if UNITY_STANDALONE || UNITY_WEBPLAYER

									//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
						            int previousPosition = horizontal;
									horizontal = (int) (Input.GetAxisRaw ("Horizontal"));

									//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
									vertical = (int) (Input.GetAxisRaw ("Vertical"));

									//Check if moving horizontally, if so set vertical to zero.
									if(horizontal != 0)
									{
										vertical = 0;
									}
									//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
						#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE

									//Check if Input has registered more than zero touches
									if (Input.touchCount > 0)
									{
										//Store the first touch detected.
										Touch myTouch = Input.touches[0];

										//Check if the phase of that touch equals Began
										if (myTouch.phase == TouchPhase.Began)
										{
											//If so, set touchOrigin to the position of that touch
											touchOrigin = myTouch.position;
										}

										//If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
										else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
										{
											//Set touchEnd to equal the position of this touch
											Vector2 touchEnd = myTouch.position;

											//Calculate the difference between the beginning and end of the touch on the x axis.
											float x = touchEnd.x - touchOrigin.x;

											//Calculate the difference between the beginning and end of the touch on the y axis.
											float y = touchEnd.y - touchOrigin.y;

											//Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
											touchOrigin.x = -1;

											//Check if the difference along the x axis is greater than the difference along the y axis.
											if (Mathf.Abs(x) > Mathf.Abs(y))
												//If x is greater than zero, set horizontal to 1, otherwise set it to -1
												horizontal = x > 0 ? 1 : -1;
											else
												//If y is greater than zero, set horizontal to 1, otherwise set it to -1
												vertical = y > 0 ? 1 : -1;
										}
									}

						#endif //End of mobile platform dependendent compilation section started above with #elif
									//Check if we have a non-zero value for horizontal or vertical
									if(horizontal != 0 || vertical != 0)
									{
						                if((previousPosition - horizontal) >= 0) {
						                    //print("in true");
						                    spriteRenderer.flipX = true;
						                } else {
						                    //print("in false");
						                    spriteRenderer.flipX = false;
						                }

						                //Call AttemptMove passing in the generic parameter Wall, Enemy, Chest, Boss
										//Pass in horizontal and vertical as parameters to specify the direction to move Player in.
										AttemptMove<Wall, Enemy, Chest, Boss> (horizontal, vertical, "");
						            }
					}

		}

		//AttemptMove overrides the AttemptMove function in the base class MovingObject
		//AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
		protected override void AttemptMove <T, E, V, K> (int xDir, int yDir, string type)
		{
			//Call the AttemptMove method of the base class, passing in the component T (in this case Wall or enemy) and x and y direction to move.
			base.AttemptMove <T, E, V, K> (xDir, yDir, type);
            
			//Since the player has moved and lost food points, check if the game has ended.
			CheckIfGameOver ();

			//Set the playersTurn boolean of GameManager to false now that players turn is over.
			GameManager.instance.playersTurn = false;
		}


		//OnCantMove overrides the abstract function OnCantMove in MovingObject.
		//It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
		protected override void OnCantMove <T> (T component)
		{
			//Set hitWall to equal the component passed in as a parameter.
			Wall hitWall = null;
			Enemy hitEnemy = null;
			Chest hitChest = null;
            Boss hitBoss = null;

			//Player hit a wall
			if (component is Wall) {
				hitWall = component as Wall;
			}
            //Player hit an enemy
            else if (component is Enemy) {
				hitEnemy = component as Enemy;
			}
            //Player hit a chest
            else if (component is Chest) {
				hitChest = component as Chest;
            }
            //Player hit a boss
            else if (component is Boss) {
                hitBoss = component as Boss;
            }

            //Call damageWall method
			if (hitWall != null)
				hitWall.DamageWall (wallDamage);
            //Call damageEnemy method
			if (hitEnemy != null)
				hitEnemy.DamageEnemy (attackDamage);
            //Call damageChest method
			if (hitChest != null)
				hitChest.DamageChest (chestDamage);
            //Call damageEnemy method
            if (hitBoss != null)
                hitBoss.DamageEnemy(attackDamage);

            //Set the attack trigger of the player's animation controller in order to play the player's attack animation.
            animator.SetTrigger ("knightAttack");

            //Weapon looses durability
			if (durability != 99)
				durability--;

            //Weapon broke
			if (durability <= 0) {
				durability = 99;
				SoundManager.instance.RandomizeSfx (weaponBreakSound, weaponBreakSound);
			}
		}


		//OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
		private void OnTriggerEnter2D (Collider2D other)
		{
			//Check if the tag of the trigger collided with is Exit.
			if(other.tag == "Exit" && hasKey)
			{
				//Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
				Invoke ("Restart", restartLevelDelay);

                //Disable the player object since level is over.
                //enabled = false;
                updating = true;
			}

			//Check if the tag of the trigger collided with is Food.
			else if(other.tag == "Heal")
			{
				if(health < 200) {
                    health += pointsPerSoda;
                    UpdateHearts(health);
	
                    //Update foodText to represent current total and notify player that they gained points
                    //foodText.text = "+" + pointsPerFood + " Health: " + food;
					HitTextManager.Instance.CreateText(transform.position, "+" + pointsPerSoda, Color.green);
                }

				//Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
				SoundManager.instance.RandomizeSfx (eatSound1, eatSound2);

				//Disable the food object the player collided with.
				other.gameObject.SetActive (false);
			}

			//Check if the tag of the trigger collided with is Soda.
			else if(other.tag == "Damage")
			{
				//Add pointsPerSoda to players food points total
				health -= pointsPerSoda;
				UpdateHearts(health);

				//Update foodText to represent current total and notify player that they gained points
				//foodText.text = "-" + pointsPerSoda + " Health: " + food;

				//Call the RandomizeSfx function of SoundManager and pass in two drinking sounds to choose between to play the drinking sound effect.
				SoundManager.instance.RandomizeSfx (drinkSound1, drinkSound2);
				HitTextManager.Instance.CreateText(transform.position, "-"+pointsPerSoda, Color.red);
				//Disable the soda object the player collided with.
				other.gameObject.SetActive (false);
			}

            //Player picked up an item
			else if(other.tag == "Item")
			{
                //Update the inventory
				UpdateInventory (other.GetComponent<Item>());
				other.gameObject.SetActive (false);
			}

            //Player picked up the key
			else if(other.tag == "Key")
			{
					hasKey = true;
					EnableKeySprite();
					other.gameObject.SetActive(false);
			}

            //Player picked up reverse potion
			else if(other.tag == "Reverse")
			{
					hasReversePotion *= -1;
					other.gameObject.SetActive(false);
			}

            //Player picked up increaseAttack potion
			else if(other.tag == "IncreaseAttack")
			{
				attackDamage += pointsPerIncreaseAttack;

				//Update foodText to represent current total and notify player that they gained points
				damageText.text = "+" + pointsPerIncreaseAttack + " Attack: " + attackDamage;
                Invoke("ResetAttackText", resetTextDelay);

                //Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
                //SoundManager.instance.RandomizeSfx (eatSound1, eatSound2);

                //Disable the food object the player collided with.
                other.gameObject.SetActive (false);
			}
            //Player picked up decreaseAttack potion
			else if(other.tag == "DecreaseAttack")
			{
                if (attackDamage > 1)
                {
                    int subtractDamage = attackDamage / 2;
                    attackDamage -= subtractDamage;

                    //Update foodText to represent current total and notify player that they gained points
                    damageText.text = "-" + subtractDamage + " Attack: " + attackDamage;
                    Invoke("ResetAttackText", resetTextDelay);
                }
                
				//Disable the food object the player collided with.
				other.gameObject.SetActive (false);
			}
            //Player picked up Random potion
			else if(other.tag == "Random")
			{
                RandomEffect();
                //Disable the food object the player collided with.
                other.gameObject.SetActive(false);
            }
            //Player picked up a gem
            else if(other.tag == "Gem")
            {
                numGems++;
                UpdateGemCount();
                other.gameObject.SetActive(false);
            }
            //Player picked up the lantern
            else if(other.tag == "Lantern") {
                Camera.main.GetComponent<ScreenTransitionImageEffect>().maskValue = 0;
                other.gameObject.SetActive(false);
            }
		}

        //Heal player
        public void Heal()
        {
            //Add pointsPerFood to the players current food total.
            health += pointsPerFood;
            UpdateHearts(health);
        }

        //Grant random effect to player
        public void RandomEffect()
        {
            int randomIndex = Random.Range(0, 5);

            switch (randomIndex)
            {
                //Increase health
                case 0:
                    if (health < 200)
                    {
                        health += pointsPerFood;
                        UpdateHearts(health);
                    }

                    //Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
                    SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

                    break;
                //Decrease health
                case 1:
                    health -= pointsPerSoda;
                    UpdateHearts(health);
					HitTextManager.Instance.CreateText(transform.position, "-"+pointsPerSoda, Color.red);
                    //Call the RandomizeSfx function of SoundManager and pass in two drinking sounds to choose between to play the drinking sound effect.
                    SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);

                    break;
                //Reverse current movements
                case 2:
                    hasReversePotion *= -1;
                    break;
                //Increase attackDamage
                case 3:
                    attackDamage += pointsPerIncreaseAttack;

                    //Update foodText to represent current total and notify player that they gained points
                    damageText.text = "+" + pointsPerIncreaseAttack + " Attack: " + attackDamage;
                    Invoke("ResetAttackText", resetTextDelay);

                    break;
                //Decrease attackDamage
                case 4:
                    //Only decrease the attackDamage if it is greater than 1
                    if (attackDamage > 1)
                    {
                        int subtractDamage = attackDamage / 2;
                        attackDamage -= subtractDamage;

                        //Update foodText to represent current total and notify player that they gained points
                        damageText.text = "-" + subtractDamage + " Attack: " + attackDamage;
                        Invoke("ResetAttackText", resetTextDelay);
                    }
                    break;
            }
        }

		//Restart reloads the scene when called.
		private void Restart ()
		{
            GameManager.instance.NextLevel();   //Start the next level
            hasKey = false; //Player used the key
			keyImage.sprite = emptyKeySprite;
			hasReversePotion = -1;  //Movement no longer reversed
		}


		//LoseFood is called when an enemy attacks the player.
		//It takes a parameter loss which specifies how many points to lose.
		public void LoseFood (int loss)
		{
			//Set the trigger for the player animator to transition to the playerHit animation.
			animator.SetTrigger ("knightHit");

			//Subtract lost food points from the players total.
			health -= loss;

			//Update the food display with the new total.
			UpdateHearts(health);

			// Flash sprite Red to indicate a hit was made
			StartCoroutine(Flash());

			HitTextManager.Instance.CreateText(transform.position, "-"+loss, Color.red);
			//Check to see if game has ended.
			CheckIfGameOver ();
		}


		//CheckIfGameOver checks if the player is out of food points and if so, ends the game.
		private void CheckIfGameOver ()
		{
			//Check if food point total is less than or equal to zero.
			if (health <= 0)
			{
				//Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
				SoundManager.instance.PlaySingle (gameOverSound);

				//Stop the background music.
				SoundManager.instance.musicSource.Stop();

				//Call the GameOver function of GameManager.
				GameManager.instance.GameOver ();
			}
		}

        //Flash red when player is injured
		IEnumerator Flash(){
			GetComponent<SpriteRenderer> ().color = Color.red;
			yield return new WaitForSeconds (0.3f);
			GetComponent<SpriteRenderer> ().color = Color.white;
			yield return new WaitForSeconds (0.3f);
		}

        //Update the heart system based on the health of the player
		void UpdateHearts(int health) {
			switch(health) {
				case 0:
					heartContainer0.sprite = healthSprites[0];
					heartContainer1.sprite = healthSprites[0];
					heartContainer2.sprite = healthSprites[0];
					heartContainer3.sprite = healthSprites[0];
					break;
				case 25:
					heartContainer0.sprite = healthSprites[1];
					heartContainer1.sprite = healthSprites[0];
					heartContainer2.sprite = healthSprites[0];
					heartContainer3.sprite = healthSprites[0];
					break;
				case 50:
					heartContainer0.sprite = healthSprites[2];
					heartContainer1.sprite = healthSprites[0];
					heartContainer2.sprite = healthSprites[0];
					heartContainer3.sprite = healthSprites[0];
					break;
				case 75:
					heartContainer0.sprite = healthSprites[2];
					heartContainer1.sprite = healthSprites[1];
					heartContainer2.sprite = healthSprites[0];
					heartContainer3.sprite = healthSprites[0];
					break;
				case 100:
					heartContainer0.sprite = healthSprites[2];
					heartContainer1.sprite = healthSprites[2];
					heartContainer2.sprite = healthSprites[0];
					heartContainer3.sprite = healthSprites[0];
					break;
				case 125:
					heartContainer0.sprite = healthSprites[2];
					heartContainer1.sprite = healthSprites[2];
					heartContainer2.sprite = healthSprites[1];
					heartContainer3.sprite = healthSprites[0];
					break;
				case 150:
					heartContainer0.sprite = healthSprites[2];
					heartContainer1.sprite = healthSprites[2];
					heartContainer2.sprite = healthSprites[2];
					heartContainer3.sprite = healthSprites[0];
					break;
				case 175:
					heartContainer0.sprite = healthSprites[2];
					heartContainer1.sprite = healthSprites[2];
					heartContainer2.sprite = healthSprites[2];
					heartContainer3.sprite = healthSprites[1];
					break;
				case 200:
					heartContainer0.sprite = healthSprites[2];
					heartContainer1.sprite = healthSprites[2];
					heartContainer2.sprite = healthSprites[2];
					heartContainer3.sprite = healthSprites[2];
					break;
			}
		}

		void EnableKeySprite() {
			keyImage.sprite = keySprite;
		}
        
        //Update the sprites in the inventory with the item passed in
		public void UpdateInventory(Item itemData) {

			// Restore original attack and armor (Keeps effects given by potions)
			if (attackMod > 0)
				attackDamage -= attackMod;
			if (defenseMod > 0)
				armorPoints -= defenseMod;

            //Perform an action based on the type of the item
			switch (itemData.type)
            {
            //Add the shield if it is stronger than the current one
			case itemType.shield:
				{
					if (!inventory.ContainsKey ("Shield"))
						inventory.Add ("Shield", itemData);
					else if (itemData.defenseMod >= defenseMod)
						inventory ["Shield"] = itemData;
					else
						break;
					shield.color = itemData.baseColor;
					break;
				}
             //Add the sword if it is stronger than the current one
			case itemType.sword:
				{
					if (!inventory.ContainsKey ("Sword")) {
						inventory.Add ("Sword", itemData);
					} else if (itemData.attackMod >= attackMod) {
						inventory ["Sword"] = itemData;
					} else {
						durability = itemData.durability;
						break;
					}
					durability = itemData.durability;
					sword.color = itemData.baseColor;
					break;
				}
            //Increase the gem count
            case itemType.gem:
                {
                    numGems++;
                        UpdateGemCount();
                    break;
                }
			}

            //Update the attack and defense values
			attackMod = 0;
			defenseMod = 0;

			foreach (KeyValuePair<string, Item> gear in inventory) {
				attackMod += gear.Value.attackMod;
				defenseMod += gear.Value.defenseMod;
			}

			attackDamage += attackMod;
			armorPoints += defenseMod;

			if (armorPoints > 4)
				armorPoints = 4;

            if (attackMod > 0)
            {
                damageText.text = "+" + attackMod + " Attack: " + attackDamage;
                Invoke("ResetAttackText", resetTextDelay);
            }
		}

        private void ResetAttackText()
        {
            damageText.text = "Attack: " + attackDamage;
        }

        public void UpdateGemCount()
        {
            gemText.text = "" + numGems;
        }
	}
}
