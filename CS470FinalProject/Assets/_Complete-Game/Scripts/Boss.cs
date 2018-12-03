using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Completed
{

    public class Boss : MovingObject
    {

        public int playerDamage;                            //The amount of food points to subtract from the player when attacking.
        public int enemyHealth;                             // The health of the enemy
        public AudioClip attackSound1;                      //First of two audio clips to play when attacking the player.
        public AudioClip attackSound2;                      //Second of two audio clips to play when attacking the player.
        public GameObject key;                              //The key to be dropped when the enemy dies
        public bool hasKey;                                 //Indicates if the enemy has the key

        private Animator animator;                          //Variable of type Animator to store a reference to the enemy's Animator component.
        private Transform target;                           //Transform to attempt to move toward each turn.
        private bool skipMove;                              //Boolean to determine whether or not enemy should skip a turn or move this turn.
        public GameObject[] items;

        //Start overrides the virtual Start function of the base class.
        protected override void Start()
        {
            enemyHealth = 40;
            //Register this enemy with our instance of GameManager by adding it to a list of Enemy objects.
            //This allows the GameManager to issue movement commands.
            GameManager.instance.AddEnemyToList(this);

            //Get and store a reference to the attached Animator component.
            animator = GetComponent<Animator>();

            //Find the Player GameObject using it's tag and store a reference to its transform component.
            target = GameObject.FindGameObjectWithTag("Player").transform;

            //Call the start function of our base class MovingObject.
            base.Start();
        }


        //Override the AttemptMove function of MovingObject to include functionality needed for Enemy to skip turns.
        //See comments in MovingObject for more on how base AttemptMove function works.
        protected override void AttemptMove<P, W, V, K>(int xDir, int yDir, string type)
        {
            //Check if skipMove is true, if so set it to false and skip this turn.
            if (skipMove && !GameManager.instance.fastEnemy)
            {
                skipMove = false;
                return;

            }

            //Call the AttemptMove function from MovingObject.
            base.AttemptMove<P, W, V, K>(xDir, yDir, type);

            //Now that Enemy has moved, set skipMove to true to skip next move.
            skipMove = true;
        }


        //MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player.
        public void MoveEnemy()
        {
            //Declare variables for X and Y axis move directions, these range from -1 to 1.
            //These values allow us to choose between the cardinal directions: up, down, left and right.
            int xDir = 0;
            int yDir = 0;

            //If the difference in positions is approximately zero (Epsilon) do the following:
            if (target != null)
            {

                if (GameManager.instance.smartEnemy)
                {

                    int xHeading = (int)target.position.x - (int)transform.position.x;
                    int yHeading = (int)target.position.y - (int)transform.position.y;
                    bool moveOnX = false;

                    if (Mathf.Abs(xHeading) >= Mathf.Abs(yHeading))
                    {
                        moveOnX = true;
                    }

                    for (int moveAttempt = 0; moveAttempt < 2; moveAttempt++)
                    {
                        if (moveOnX && xHeading < 0)
                        {
                            xDir = -1;
                            yDir = 0;
                        }
                        else if (moveOnX && xHeading > 0)
                        {
                            xDir = 1;
                            yDir = 0;
                        }
                        else if (!moveOnX && yHeading < 0)
                        {
                            yDir = -1;
                            xDir = 0;
                        }
                        else if (!moveOnX && yHeading > 0)
                        {
                            yDir = 1;
                            xDir = 0;
                        }

                        Vector3 start = transform.position;
                        Vector3 end = start + new Vector3(xDir, yDir, 0);
                        base.boxCollider.enabled = false;
                        RaycastHit2D hit = Physics2D.Linecast(start, end, base.blockingLayer);
                        base.boxCollider.enabled = true;

                        if (hit.transform != null)
                        {
                            if (hit.transform.gameObject.tag == "OuterWall")
                            {
                                if (moveOnX)
                                    moveOnX = false;
                                else
                                    moveOnX = true;
                            }
                            else
                                break;
                        }
                    }
                }

                // Normal Enemy Movement (1 Tile)
                else
                {

                    if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)

                        //If the y coordinate of the target's (player) position is greater than the y coordinate of this enemy's position set y direction 1 (to move up). If not, set it to -1 (to move down).
                        yDir = target.position.y > transform.position.y ? 1 : -1;

                    //If the difference in positions is not approximately zero (Epsilon) do the following:
                    else
                        //Check if target x position is greater than enemy's x position, if so set x direction to 1 (move right), if not set to -1 (move left).
                        xDir = target.position.x > transform.position.x ? 1 : -1;
                }


                //Call the AttemptMove function and pass in the generic parameter Player, because Enemy is moving and expecting to potentially encounter a Player
                AttemptMove<Player, Wall, Chest, Boss>(xDir, yDir, "Boss");
            }
        }


        //OnCantMove is called if Enemy attempts to move into a space occupied by a Player, it overrides the OnCantMove function of MovingObject
        //and takes a generic parameter T which we use to pass in the component we expect to encounter, in this case Player
        protected override void OnCantMove<T>(T component)
        {
            if (component is Wall)
                return;
            if (component is Chest)
                return;
            //Declare hitPlayer and set it to equal the encountered component.
            Player hitPlayer = component as Player;

            //Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
            hitPlayer.LoseFood(playerDamage);

            //Set the attack trigger of animator to trigger Enemy attack animation.
            animator.SetTrigger("bossAttack");

            //Call the RandomizeSfx function of SoundManager passing in the two audio clips to choose randomly between.
            SoundManager.instance.RandomizeSfx(attackSound1, attackSound2);
        }

        public void DamageEnemy(int loss)
        {
            // Flash sprite Red to indicate a hit was made
            StartCoroutine(Flash());

            enemyHealth -= loss;
            if (enemyHealth <= 0)
            {
                animator.SetTrigger("isKilled");

                if (hasKey)
                {
                    Instantiate(key, this.transform.position, Quaternion.identity);
                }
                gameObject.SetActive(false);
                int counter = 1;
                for (int i = 0; i < items.Length; i++) {
                    for (int j = 0; j < Random.Range(0, 4); j++) {
                        Instantiate(items[i], new Vector3(this.transform.position.x + counter, this.transform.position.y + counter, 1), Quaternion.identity);
                        counter++;
                    }
                }
                GameManager.instance.RemoveBossFromList(this);
            } else {
                animator.SetTrigger("bossHurt");
            }
        }

        IEnumerator Flash()
        {
            GetComponent<SpriteRenderer>().color = Color.red;
            yield return new WaitForSeconds(0.3f);
            GetComponent<SpriteRenderer>().color = Color.white;
            yield return new WaitForSeconds(0.3f);
        }

    }
}
