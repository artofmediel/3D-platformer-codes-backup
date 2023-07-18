using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;

    public PlayerController thePlayer;

    public float invincibilityLength;
    private float invincibilityCounter;

    //Invinsibility indicator
    public Renderer playerRenderer;
    //private float flashCounter;
    //public float flashLength = 0.1f;

    // Tint properties
    private Material characterMaterial;
    private Color originalColor;
    public Color tintedColor = Color.yellow;
    public float tintDuration = 0.1f;
    private float tintCounter;

    private bool isRespawning;
    public GameObject respawnPoint;
    //respawn point vector value
    public Vector3 respawnPointValue;

    public float respawnLength;

    public Animator playerAnimator;

    //add damage effect : supposed to be death effect in the tutorial
    public GameObject damageFX;

    //fade to black fx on death
    public Image blackScreen;
    private bool isFadeToBlack;
    private bool isFadeFromBlack;
    public float fadeSpeed;

    //disable controls on death
    private bool disablePlayerControl;

    //hold float value of allowed time of falling before trigger respawn
    private float allowedFallTimeBeforeRespawn;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;

        //thePlayer = FindObjectOfType<PlayerController>();

        // Get the material from the character's mesh renderer
        characterMaterial = playerRenderer.material;

        // Store the original color
        originalColor = characterMaterial.color;

        //respawnPointValues = respawnPoint.transform.position;

        disablePlayerControl = false;

    }

    // Update is called once per frame
    void Update()
    {
        respawnPointValue = respawnPoint.transform.position;

        if (invincibilityCounter > 0) 
        {
            invincibilityCounter -= Time.deltaTime;

            /*flashCounter -= Time.deltaTime;
            if(flashCounter <= 0) 
            {
                playerRenderer.enabled = !playerRenderer.enabled;
                flashCounter = flashLength;
            }

            if (invincibilityCounter <= 0)
            { 
                playerRenderer.enabled = true;
            }*/

            tintCounter -= Time.deltaTime;
            if (tintCounter <= 0)
            {
                // Tint the character mesh
                if (characterMaterial.color != tintedColor)
                {
                    characterMaterial.color = tintedColor;
                } else
                {
                    characterMaterial.color = originalColor;
                }
                tintCounter = tintDuration;

                if (tintCounter <= 0)
                {
                    // Reset the color to the original state
                    characterMaterial.color = originalColor;
                }
            }

            if (invincibilityCounter <= 0)
            {
                // Ensure the color is reset when invincibility ends
                characterMaterial.color = originalColor;
            }
        }

        if(disablePlayerControl)
        {
            //thePlayer.gameObject.SetActive(false);
            thePlayer.GetComponent<PlayerController>().enabled = false;
            // there's a bug that if you reach 0 health but still pressing movement keys, thePlayer wont teleport to respawn point
            // tried disabling the controller and it works fine now.
            thePlayer.controller.enabled = false;
        } 
        else
        {
            //thePlayer.gameObject.SetActive(true);
            thePlayer.GetComponent<PlayerController>().enabled = true;
            thePlayer.controller.enabled = true;
        }

        //fade effect
        if(isFadeToBlack) 
        {
            blackScreen.color = new Color(blackScreen.color.r, blackScreen.color.g, blackScreen.color.b, Mathf.MoveTowards(blackScreen.color.a, 1f, fadeSpeed * Time.deltaTime));
            if(blackScreen.color.a == 1f)
            {
                isFadeToBlack = false;
            }
        }
        if (isFadeFromBlack)
        {
            blackScreen.color = new Color(blackScreen.color.r, blackScreen.color.g, blackScreen.color.b, Mathf.MoveTowards(blackScreen.color.a, 0f, fadeSpeed * Time.deltaTime));
            if (blackScreen.color.a == 0f)
            {
                isFadeFromBlack = false;
            }
        }

    }

    public void HurtPlayer(int damage, Vector3 direction) 
    {
        if (invincibilityCounter <= 0)
        {
            currentHealth -= damage;

            //insert the damageFX
            Instantiate(damageFX,thePlayer.transform.position, thePlayer.transform.rotation);

            if(currentHealth <= 0)
            {
                thePlayer.Knockback(direction);
                invincibilityCounter = invincibilityLength;
                StartCoroutine(WaitForKnockbacktoDeathAnimation());

                //StartCoroutine(WaitForPlayertoReachGround());
                //Respawn();
            }
            else 
            {
                thePlayer.Knockback(direction);
                invincibilityCounter = invincibilityLength;

                /*playerRenderer.enabled = false;

                flashCounter = flashLength;*/

                // Start the tint effect
                characterMaterial.color = tintedColor;
                tintCounter = tintDuration;
            }
        }
    }

    public void BouncePlayer(float bounceForce, Vector3 bounceDirection)
    {

        Instantiate(damageFX, thePlayer.transform.position, thePlayer.transform.rotation);

        if (invincibilityCounter <= 0)
        {
            thePlayer.BouncePlayer(bounceForce, bounceDirection);
        }
    }

    public void Respawn()
    {
        if (!isRespawning) { 
            StartCoroutine("RespawnCo");
        }
    }

    private IEnumerator WaitForDeathAnimation()
    {

        playerAnimator.SetTrigger("isPlayerDead"); // Trigger the "DEATH" animation

        disablePlayerControl = true;

        yield return new WaitForSeconds(2); // Replace `deathAnimationDuration` with the actual duration of the "DEATH" animation

        Respawn();
    }

    private IEnumerator WaitForFallingAnimation()
    {
        if (thePlayer.controller.isGrounded)
        {
            playerAnimator.SetTrigger("isPlayerDead"); // Trigger the "DEATH" animation

            allowedFallTimeBeforeRespawn = 0;

            disablePlayerControl = true;

            yield return new WaitForSeconds(2f); // Replace `deathAnimationDuration` with the actual duration of the "DEATH" animation

            Respawn();
        }
        else
        {
            //check first if elapsed time is already 2 seconds
            if (allowedFallTimeBeforeRespawn < 2f)  
            {
                yield return new WaitForSeconds(0.5f); //Wait for a half a second 
                allowedFallTimeBeforeRespawn += 0.5f;   //add elapsed time before calling the function again
                StartCoroutine(WaitForFallingAnimation());

            } 
            else
            {
                allowedFallTimeBeforeRespawn = 0;

                disablePlayerControl = true;

                Respawn();
            }
        }
    }


    private IEnumerator WaitForKnockbacktoDeathAnimation()
    {
        yield return new WaitForSeconds(thePlayer.knockbackTime); //wait for knockbackTime to finish

        if (thePlayer.controller.isGrounded)
        {
            StartCoroutine(WaitForDeathAnimation()); // Start a coroutine to wait for the animation to finish before respawning
        }
        else {
            playerAnimator.SetTrigger("isFalling"); //Trigger Falling animation

            StartCoroutine(WaitForFallingAnimation()); //Start a couroutine to wait for the fall animation
        }
    }

    public IEnumerator RespawnCo()
    {
        //fade effect
        isFadeToBlack = true;

        isRespawning = true;

        yield return new WaitForSeconds(respawnLength);
        isRespawning = false;

        isFadeToBlack = true;
        //fade effect
        isFadeFromBlack = true;

        //TeleportToRespawnPoint();
        thePlayer.transform.position = new Vector3(respawnPointValue.x, respawnPointValue.y + 1, respawnPointValue.z);

        disablePlayerControl = false;

        playerAnimator.SetTrigger("isPlayerAlive");
        currentHealth = maxHealth;

        //invincibilityCounter = invincibilityLength;
        //characterMaterial.color = tintedColor;
        //tintCounter = tintDuration;
    }


    public void HealPlayer(int healAmount)
    {
        currentHealth += healAmount;

        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;

        }
    }

    public void SetSpawnPoint(Vector3 newPosition) 
    {
        respawnPoint.transform.position = newPosition;

    }
}
