using UnityEngine;
using System.Collections;

namespace Completed
{
	public class Chest : MonoBehaviour
	{
		public float chestDelay = 1f;
		public AudioClip chopSound1;
		public Sprite dmgSprite;					//Alternate sprite to display after Wall has been attacked by player.
		public int hp = 1;							//hit points for the wall.
		public Item randomItem;
		public GameObject item;
		//public GameObject[] potions;
		//public int maxNumOfPotions = 5;

		private SpriteRenderer spriteRenderer;		//Store a component reference to the attached SpriteRenderer.


		void Awake ()
		{
			//Get a component reference to the SpriteRenderer.
			spriteRenderer = GetComponent<SpriteRenderer> ();
		}


		//DamageWall is called when the player attacks a wall.
		public void DamageChest (int loss)
		{
			//Call the RandomizeSfx function of SoundManager to play one of two chop sounds.
			SoundManager.instance.RandomizeSfx (chopSound1, chopSound1);

			//Set spriteRenderer to the damaged wall sprite.
			spriteRenderer.sprite = dmgSprite;

			//Subtract loss from hit point total.
			hp -= loss;

			//If hit points are less than or equal to zero:
			if (hp <= 1)
			{
				DisableChest ();
				//Invoke("setGameObjectFalse", chestDelay);
				//Disable the gameObject.
				//gameObject.SetActive(false);
			}


		}

		public void DisableChest(){
			Invoke("setGameObjectFalse", chestDelay);


		}

		public void setGameObjectFalse() {
			gameObject.SetActive(false);
			randomItem.RandomItemInit ();
			GameObject toInstantiate = randomItem.gameObject;
			GameObject instance = Instantiate (toInstantiate, new Vector3 (transform.position.x, transform.position.y, 0f), Quaternion.identity) as GameObject;
			instance.transform.SetParent (transform.parent);
			gameObject.layer = 10;
			spriteRenderer.sortingLayerName = "Items";
		}
	}
}
