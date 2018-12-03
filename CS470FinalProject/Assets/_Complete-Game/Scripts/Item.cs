using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum itemType{
	shield, sword, gem
}	

public class Item : MonoBehaviour {
	public Sprite shield;   //Holds the shield sprite
	public Sprite sword;    //Hodls the sword sprite
    public Sprite gem;  //Holds the gem sprite

	public itemType type;   //The type of the item
	public Color baseColor; //The color of the item
	public int attackMod, defenseMod;   //Attack and defense values
	public int durability;  //How long it will last

	private SpriteRenderer spriteRenderer;

    //Set this to a random item
	public void RandomItemInit(){
		spriteRenderer = GetComponent<SpriteRenderer> ();
		SelectItem ();  //Select a random Item
	}

    //Set this to a sword
    public void GetSword()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        int powerLevel = Random.Range(0, 100);  //Get a random power level
        type = itemType.sword;  //Make it of type sword
        spriteRenderer.sprite = sword;  //Set the sprite to the sword sprite
        attackMod = 0;  //Set the attack power to 0 for now

        /////////////////////////////////////////////////////////////////////
        //
        //  Set the attack power and the color based on the powerlevel
        //  Order from weakest to strongest:
        //      Blue
        //      Green
        //      Yellow
        //      Magenta
        //
        /////////////////////////////////////////////////////////////////////
        if (powerLevel >= 0 && powerLevel < 50)
        {
            spriteRenderer.color = baseColor = Color.blue;
            attackMod += 5;
            durability = 2;
        }
        else if (powerLevel >= 50 && powerLevel < 75)
        {
            spriteRenderer.color = baseColor = Color.green;
            attackMod += 10;
            durability = 3;
        }
        else if (powerLevel >= 75 && powerLevel < 90)
        {
            spriteRenderer.color = baseColor = Color.yellow;
            attackMod += 25;
            durability = 4;
        }
        else
        {
            spriteRenderer.color = baseColor = Color.magenta;
            attackMod += 50;
            durability = 5;
        }
    }

    //Set this to a shield
    public void GetShield()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        int powerLevel = Random.Range(0, 100);  //Get a random power level
        type = itemType.shield; //Set it to type shield
        spriteRenderer.sprite = shield; //Set the sprite to the shield sprite

        defenseMod = 0; //Set the defense power to 0

        /////////////////////////////////////////////////////////////////////
        //
        //  Set the defense power and the color based on the powerlevel
        //  Order from weakest to strongest:
        //      Blue
        //      Green
        //      Yellow
        //      Magenta
        //
        /////////////////////////////////////////////////////////////////////
        if (powerLevel >= 0 && powerLevel < 50)
        {
            spriteRenderer.color = baseColor = Color.blue;
            defenseMod += 1;
        }
        else if (powerLevel >= 50 && powerLevel < 75)
        {
            spriteRenderer.color = baseColor = Color.green;
            defenseMod += 2;
        }
        else if (powerLevel >= 75 && powerLevel < 90)
        {
            spriteRenderer.color = baseColor = Color.yellow;
            defenseMod += 3;
        }
        else
        {
            spriteRenderer.color = baseColor = Color.magenta;
            defenseMod += 5;
        }
    }

    //Set this to a random item
	private void SelectItem()
    {
		int powerLevel = Random.Range (0, 100); //Get a random power level
		var itemCount = System.Enum.GetValues (typeof(itemType)).Length;    //Set how many items there are
		type = (itemType)Random.Range (0, itemCount);   //Get a random type

        //Set the sprite to the correct type
		switch (type) {
		case itemType.shield:
			{
				spriteRenderer.sprite = shield;
				break;
			}
		case itemType.sword:
			{
				spriteRenderer.sprite = sword;
				break;
			}
        case itemType.gem:
            {
                spriteRenderer.sprite = gem;
                break;
            }
		}

        //If the type is not a gem, set the color and attack/defense power of the item
        if (type != itemType.gem)
        {
            //Start at 0
            attackMod = 0;
            defenseMod = 0;

            /////////////////////////////////////////////////////////////////////
            //
            //  Set the attack/defense power and the color based on the powerlevel
            //  Order from weakest to strongest:
            //      Blue
            //      Green
            //      Yellow
            //      Magenta
            //
            /////////////////////////////////////////////////////////////////////
            if (powerLevel >= 0 && powerLevel < 50)
            {
                spriteRenderer.color = baseColor = Color.blue;
                if (type == itemType.shield)
                    defenseMod += 1;
                else
                {
                    attackMod += 5;
                    durability = 2;
                }
            }
            else if (powerLevel >= 50 && powerLevel < 75)
            {
                spriteRenderer.color = baseColor = Color.green;
                if (type == itemType.shield)
                    defenseMod += 2;
                else
                {
                    attackMod += 10;
                    durability = 3;
                }
            }
            else if (powerLevel >= 75 && powerLevel < 90)
            {
                spriteRenderer.color = baseColor = Color.yellow;
                if (type == itemType.shield)
                    defenseMod += 3;
                else
                {
                    attackMod += 25;
                    durability = 4;
                }
            }
            else
            {
                spriteRenderer.color = baseColor = Color.magenta;
                if (type == itemType.shield)
                    defenseMod += 5;
                else
                {
                    attackMod += 50;
                    durability = 5;
                }
            }
        }
	}
}
