using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;					//Allows us to use UI.
using Completed;

public class Trader : MonoBehaviour
{
    public int numOptions = 3;  //Number of choices
    public int[] values;        //Cost of each item
    public int[] types;         //Type of item
    public Text[] valuesText;   //Text for the cost of each item
    public GameObject[] items;  //The items (Pre-existing)
    public Sprite[] possible;   //Possible items (Pre-existing)
    public GameObject[] buttons;    //The buttons for the options
    public Item[] itemOption;   //The items that can be chosen (Pre-existing)

    private GameObject player;  //The player

    // Use this for initialization
    void Start ()
    {
        buttons = new GameObject[numOptions];
        UpdateOptions();
	}

    //Create random items for the 3 options
    public void UpdateOptions()
    {
        for(int i = 0; i < numOptions; i++)
        {
            types[i] = Random.Range(0, possible.Length);    //get a random type
            values[i] = types[i];   //Set the value to the type
            valuesText[i].text = "" + values[i];    //Set the text
            //Set the sprite to the correct sword sprite
            if (types[i] == 1)
            {
                itemOption[0].GetSword();
                items[i].GetComponent<Image>().sprite = itemOption[0].sword;
                items[i].GetComponent<Image>().color = itemOption[0].baseColor;
            }
            //Set the sprite to the correct shield sprite
            else if (types[i] == 2)
            {
                itemOption[1].GetShield();
                items[i].GetComponent<Image>().sprite = itemOption[1].shield;
                items[i].GetComponent<Image>().color = itemOption[1].baseColor;
            }
            //Set the sprite to a potion
            else
                items[i].GetComponent<Image>().sprite = possible[types[i]];
        }
    }

    //One of the options was selected. Num indicates which option
    public void Selected(int num)
    {
        player = GameObject.FindWithTag("Player");  //Get the player
        //Return if there is no player
        if (player == null)
            return;
        //Get the number of gems the player has
        int numGems = Player.instance.numGems;
        //Return if the player does not have enough gems
        if (numGems < values[num - 1])
            return;
        //Remove gems from player
        Player.instance.numGems -= values[num - 1];
        //Update gem count
        Player.instance.UpdateGemCount();
        ApplyItem(num - 1); //Apply the purchased item
        //Remove the button so player can only buy one
        buttons[num - 1] = GameObject.Find("Option" + num);
        buttons[num - 1].SetActive(false);
    }

    //Apply the effect of the purchased item
    public void ApplyItem(int num)
    {
        switch(types[num])
        {
            //Heal potion
            case 0:
                Player.instance.Heal();
                break;
            //Sword
            case 1:
                Player.instance.UpdateInventory(itemOption[0]);
                break;
            //Shield
            case 2:
                Player.instance.UpdateInventory(itemOption[1]);
                break;
            //Random potion
            case 3:
                Player.instance.RandomEffect();
                break;
        }
    }

    //Leave trader screen
    public void Exit()
    {
        ResetButtons(); //Reset buttons for next time
        GameObject.Find("Trader").SetActive(false);
    }

    //ReActivates the buttons for next time the trader is visited
    public void ResetButtons()
    {
        for (int i = 0; i < numOptions; i++)
            if (buttons[i] != null)
                buttons[i].SetActive(true);
    }
}
