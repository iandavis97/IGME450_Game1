/** Jonathan So, jds7523@rit.edu
 * This cactus object was used in an animated preview I had made a couple years ago. It may serve as the basis for our spring's movement.
 * The cactus will charge its jump when the spacebar is held; it will jump upon release.
 */
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Cactus : MonoBehaviour {

	public GameObject deathBounds;//area where player falls & dies
	public Animator animDust; // Child gameobject which has impact animations.

	public KeyCode cactus; // The key to hit for jumping.
    public int facingRight = 1;//used for changing directions, should always start facing right. +1 is right, -1 is left. Possible values are +1 and -1.
    public int segments = 10; // number of segments in the prediction arc

    //audio clips
    public AudioSource deathSound;
    public AudioSource springSound;

    private float jumpFX = 0f; // Horizontal Force to apply to make the cactus jump.
	private float jumpFY = 0f; // Vertictal Force to apply to make the cactus jump.
    private float grav;

	// Forces for maximum jump strength.
	private const float MIN_JUMP_FX = 4f; 
	private const float MIN_JUMP_FY = 8f; 
	private const float MAX_JUMP_FX = 20f; 
	private const float MAX_JUMP_FY = 40f; 
	private const float HOLD_TIME = 1f; // Amount of time it takes to hold the button for a maximum jump.
	private const float MAX_DELAY = 0.125f; // Amount of time to hold onto maximum charge before oscillating back down.

	// Keeps track of the current coroutine that's running.
	private Coroutine currCoroutine = null;

	private GameObject winPanel; // A win screen called "Win Panel".

	private Animator anim;
	private Rigidbody2D rb;
	private SpriteRenderer sr;
	private LineRenderer line;
    private Vector2 direction;

	private Vector3 spawn;//a position to be saved so player can be respawned

	private int layerMask;

    private bool chargeUp = false;
    private bool chargeDown = false;
    private float timeUp;
    private float timeDown;
    

	void Awake() {
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody2D>();
		sr = GetComponent<SpriteRenderer>();
		line = GetComponent<LineRenderer>();

		// setup for the lineRenderer for drawing prediction line
		line.startWidth = .05f;
		line.endWidth = line.startWidth;
		spawn = transform.position;//the spawn point should be where player begins in scene
		winPanel = GameObject.Find("Win Panel");
		winPanel.SetActive(false);
		layerMask = 1 << 0; // Only collide with objects of layer 0 (default).

        // set up a variable to keep track of gravity so it can be turned on and off easily
        grav = GetComponent<Rigidbody2D>().gravityScale;
    }

	// Input detection
	void Update () {
        direction = GetMousePos();


		// New GroundChecking implementation. 
		if (Physics2D.Raycast(new Vector2(transform.position.x + 0.5f, transform.position.y), Vector2.down, 1.25f, layerMask) || Physics2D.Raycast(new Vector2(transform.position.x - 0.5f, transform.position.y), Vector2.down, 1.25f, layerMask)) {
			anim.SetBool("InAir", false);
		} else {
			anim.SetBool("InAir", true);
		}

        if (Input.GetKeyDown(cactus) && !anim.GetBool("InAir")) { // Charge our jump.
			currCoroutine = StartCoroutine(ChargeJump());
		} 
		if (Input.GetKeyUp(cactus) && !anim.GetBool("InAir")) { // Release our jump.
            //play sound effect
            if (springSound.isPlaying == false)
                springSound.Play();
			StopCoroutine(currCoroutine);
			animDust.SetTrigger("jump");
			animDust.transform.position = new Vector2(transform.position.x - (transform.localScale.x * 0.5f), transform.position.y -0.25f);
			// animDust.transform.localScale = transform.localScale;
			sr.color = Color.white;
			if (direction.y < 0.1f) { // Handle the case where the mouse is pointing below our character. This is a short hop.
				rb.AddForce(new Vector2(direction.x * MIN_JUMP_FX, MIN_JUMP_FY), ForceMode2D.Impulse);
			} else {
				rb.AddForce(new Vector2(direction.x * jumpFX, direction.y * jumpFY), ForceMode2D.Impulse);
			}
			// Reset jump force components to avoid "perfect landing jumps"
			jumpFX = 0;
			jumpFY = 0;
			anim.SetBool("Charging", false);
            chargeDown = false;
            chargeUp = false;
		} else if (Input.GetKeyUp(cactus) && anim.GetBool("InAir")) { // Cancel our jump charge.
			StopCoroutine(currCoroutine);
			anim.SetBool("Charging", false);
            chargeDown = false;
            chargeUp = false;
        }

		DrawLine();
        //changing directions before jump
        if ((direction.x <= 0 && (facingRight == 1)) || (direction.x > 0 && facingRight == -1))  {
			Facing();
        }
		//checking if below the death bounds
		if (deathBounds.transform.position.y>=transform.position.y)
		{
			Death();
		}
    }
		
	// Charges the power of our jump. Jump strength is indicated by our color.
	private IEnumerator ChargeJump() {
		float timer;
		// Default "tap-jump" is a tiny hop that gets us off the ground. 
		jumpFX = MIN_JUMP_FX;
		jumpFY = MIN_JUMP_FY;
		Color tempColor = Color.white;
		// Oscillating jump
		while (true) {
			anim.SetBool("Charging", true);
			anim.SetInteger("ChargePhase", 0); // ChargePhase goes from 0 to 4 inclusive, where 0 is minimum, 4 is max.
			// Our jump strength increases...
			timer = 0f;
			while (jumpFX < MAX_JUMP_FX || jumpFY < MAX_JUMP_FY) { 
                if(!chargeUp)
                {
                    timeUp = Time.time;
                    chargeUp = true;
                }
				jumpFX = Mathf.Lerp(MIN_JUMP_FX, MAX_JUMP_FX, (timer / HOLD_TIME));
				jumpFY = Mathf.Lerp(MIN_JUMP_FY, MAX_JUMP_FY, (timer / HOLD_TIME));
                timer = Time.time - timeUp;
				yield return new WaitForSeconds(Time.deltaTime);
				// Use jumpFX to check the strength of the jump and adjust animation accordingly.
				if (jumpFX > (3 * MAX_JUMP_FX / 4)) {
					anim.SetInteger("ChargePhase", 3);
				} else if (jumpFX > (2 * MAX_JUMP_FX / 4)) {
					anim.SetInteger("ChargePhase", 2);
				} else if (jumpFX > (1 * MAX_JUMP_FX / 4)) {
					anim.SetInteger("ChargePhase", 1);
				}
			}
			anim.SetInteger("ChargePhase", 4);
            Debug.Log("Maybe");
			yield return new WaitForSeconds(MAX_DELAY);
            Debug.Log("No");
			// Then, it decreases.
			timer = 0f;
            chargeUp = false;
            yield return new WaitForSeconds(MAX_DELAY);
            // Then, it decreases.
            timer = 0f;
            while (jumpFX > MIN_JUMP_FX || jumpFY > MIN_JUMP_FY)
            {
                if (!chargeDown)
                {
                    timeDown = Time.time;
                    chargeDown = true;
                }
                jumpFX = Mathf.Lerp(MAX_JUMP_FX, MIN_JUMP_FX, (timer / HOLD_TIME));
                jumpFY = Mathf.Lerp(MAX_JUMP_FY, MIN_JUMP_FY, (timer / HOLD_TIME));
                timer = Time.time - timeDown;
                yield return new WaitForSeconds(Time.deltaTime);
                // Use jumpFX to check the strength of the jump and adjust animation accordingly.
                if (jumpFX > (3 * MAX_JUMP_FX / 4))
                {
                    anim.SetInteger("ChargePhase", 3);
                }
                else if (jumpFX > (2 * MAX_JUMP_FX / 4))
                {
                    anim.SetInteger("ChargePhase", 2);
                }
                else if (jumpFX > (1 * MAX_JUMP_FX / 4))
                {
                    anim.SetInteger("ChargePhase", 1);
                }
            }
            chargeDown = false;
			anim.SetInteger("ChargePhase", 0);
			yield return new WaitForSeconds(Time.deltaTime);
		}
	}

    /* Returns the normalized 2d vector that points from the player to the mouse position
    */
    private Vector2 GetMousePos()
    {
        float distanceToCamera = 10; //this is used because ScreenToWorldPoint creates the point on a X/Y plane Z units away from the camera

        Vector3 playerWorldPos = gameObject.transform.position; //the players position in the world
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToCamera)); //mouses postion on the same plane as the player
        Vector2 direction = (mouseWorldPos - playerWorldPos).normalized; //normalized direction vector

        Debug.DrawRay(playerWorldPos,direction,Color.red);
        return direction;
    }

    /* Changes the direction that this object is facing.
	 * We take our current scale and save a temporary copy; we then modify individual components
	 * of it and then save it back into our current scale.
	 */
    private void Facing() {
		Vector3 tempScale = transform.localScale;
		tempScale.x *= -1;
		facingRight *= -1;
		transform.localScale = tempScale;
	}

	//when player dies (whether out of bounds or hit by obstacle) then respawns
	private void Death()
	{
        //play sound effect
        if (deathSound.isPlaying == false)
            deathSound.Play();
        //dying visual effect?
        rb.velocity = Vector2.zero;
		transform.position = spawn;
	}

	void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag == "Ground") {
			animDust.SetTrigger("land");
			animDust.transform.position = new Vector2(transform.position.x, transform.position.y - 0.25f);
		}


        // When player hits something sticky, they shouldn't bounce
        if (coll.gameObject.tag == "Sticky")
        {
            anim.SetBool("InAir", false);
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
        }
    }

	void OnTriggerEnter2D(Collider2D coll) {
		if (coll.gameObject.tag.Equals("Goal")) {
			winPanel.SetActive(true);
			rb.velocity = Vector2.zero;
			rb.isKinematic = true;
		}
	}

    // Draws a line to indicate the direction of the jump
    void DrawLine()
    {
        if (anim.GetBool("Charging"))
        {
            // declare local variables
            Vector3 end = new Vector3(jumpFX, jumpFY, 0);
            Vector3 start;

            // project the magnitude onto the direction vector to make the line point toward the cursor instead of being more accurate to the actual movement, because it feels better.
            end = end.magnitude * direction;

            // make sure the line is facing the correct x direction
            if (facingRight == -1)
            {
                end.x *= -1;
            }
            
            start = Vector3.zero;
            end += start;
            end /= rb.mass * 5; // I played with this number til I thought it felt right

            // draw the line
            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }
        else
        {
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, Vector3.zero);
        }
    }
}
