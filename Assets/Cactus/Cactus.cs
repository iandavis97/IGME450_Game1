/** Jonathan So, jds7523@rit.edu
 * This cactus object was used in an animated preview I had made a couple years ago. It may serve as the basis for our spring's movement.
 * The cactus will charge its jump when the spacebar is held; it will jump upon release.
 */
using UnityEngine;
using System.Collections;

public class Cactus : MonoBehaviour {

	public KeyCode cactus; // The key to hit for jumping.

	[SerializeField]
	private float jumpFX = 0f; // Horizontal Force to apply to make the cactus jump.
	[SerializeField]
	private float jumpFY = 0f; // Vertictal Force to apply to make the cactus jump.

	// Forces for maximum jump strength.
	private const float MAX_JUMP_FX = 600f; 
	private const float MAX_JUMP_FY = 1200f; 
	private const float HOLD_TIME = 2f; // Amount of time it takes to hold the button for a maximum jump.

	// Keeps track of the current coroutine that's running.
	private Coroutine currCoroutine = null;

	private Animator anim;
	private Rigidbody2D rb;
	private SpriteRenderer sr;

	void Awake() {
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody2D>();
		sr = GetComponent<SpriteRenderer>();
	}

	// Input detection
	void Update () {
		if (Input.GetKeyDown(cactus) && !anim.GetBool("InAir")) { // Charge our jump.
			currCoroutine = StartCoroutine(ChargeJump());
			anim.SetBool("Charging", true);
		} 
		if (Input.GetKeyUp(cactus) && !anim.GetBool("InAir")) { // Release our jump.
			StopCoroutine(currCoroutine);
			sr.color = Color.white;
			rb.AddForce(new Vector2(jumpFX, jumpFY));
			anim.SetBool("Charging", false);
			anim.SetBool("InAir", true);
		}
	}
		
	// Charges the power of our jump. Jump strength is indicated by our color.
	private IEnumerator ChargeJump() {
		float timer = 0f;
		// Default "tap-jump" is a tiny hop that gets us off the ground. 
		jumpFX = 100f;
		jumpFY = 200f;
		Color tempColor = Color.white;
		while (jumpFX < MAX_JUMP_FX || jumpFY < MAX_JUMP_FY) {
			jumpFX = Mathf.Lerp(100, MAX_JUMP_FX, (timer / HOLD_TIME));
			jumpFY = Mathf.Lerp(200, MAX_JUMP_FY, (timer / HOLD_TIME));
			tempColor.r = Mathf.Lerp(Color.white.r, Color.red.r, (timer / HOLD_TIME));
			tempColor.g = Mathf.Lerp(Color.white.g, Color.red.g, (timer / HOLD_TIME));
			tempColor.b = Mathf.Lerp(Color.white.b, Color.red.b, (timer / HOLD_TIME));
			sr.color = tempColor;
			timer += Time.deltaTime;
			yield return new WaitForSeconds(Time.deltaTime);
		}
	}

	void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag == "Ground") {
			anim.SetBool("InAir", false);
		}
	}
}
