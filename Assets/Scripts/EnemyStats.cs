using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyStats : MonoBehaviour
{

    public float maxHealth = 100;
    public float currentHealth;
    
    public float maxStamina = 50;
    public float currentStamina;
    public float staminaRegenRate = 0.5f;

    public float attackRange = 1;
    public int enemySoulLevel = 1;
    public GameObject hitParticles;

    private float regenTimer = 0;

    PlayerEquipment playerEquipment;

    public Slider healthBar;
    public BetterEnemyHealthbar betterHealthBar;

    public GameObject[] blood;
    public GameObject bloodParticles;
    public GameObject bloodParticles1;

    private PlayerStats playerStats;
    private PlayerUpgradeHandler playerUpgradeHandler;
    private EnemyAI enemyAI;

    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        playerEquipment = FindObjectOfType<PlayerEquipment>();
        /*
        if (enemySoulLevel > 1000)
        {
            healthBar = FindObjectOfType<BossRoom>().bossHealthbar.GetComponent<Slider>();
            bossBar = FindObjectOfType<BossBar>();
        }*/
        playerStats = FindObjectOfType<PlayerStats>();
        playerUpgradeHandler = FindObjectOfType<PlayerUpgradeHandler>();
        healthBar.maxValue = maxHealth;
        betterHealthBar.SetHealth(betterHealthBar.GetHealthNormalized(currentHealth,maxHealth));
        maxStamina = 25;
        enemyAI = GetComponent<EnemyAI>();
    }

    private void FixedUpdate()
    {
        RegnerateStamina();
        healthBar.value = currentHealth;
    }

    public void TakeHit(Collider other)
    {
        if (other.gameObject.CompareTag("Weapon"))
        {
            GetComponent<AudioSource>().pitch = 0.8f;
            GetComponent<AudioSource>().Play();
            Animator animator = gameObject.GetComponent<Animator>();
            float damage = playerEquipment.currentWeapon.R1Damage;
            Instantiate(blood[(int)Random.Range(0, blood.Length - 1)], transform.position, Quaternion.identity);
            /*
            var newBloodParticle = Instantiate(bloodParticles, transform.position, transform.parent.rotation);
            newBloodParticle.transform.parent = transform.parent;
            var newBloodParticle1 = Instantiate(bloodParticles1, transform.position, transform.parent.rotation);
            newBloodParticle1.transform.parent = transform.parent;*/
            if (other.gameObject.GetComponentInParent<Animator>().GetBool("AttackR2"))
            {
                damage = playerEquipment.currentWeapon.R2Damage;
            }
            var playerStats = other.GetComponentInParent<PlayerStats>();
            var playerUpgradeHandler = other.gameObject.GetComponentInParent<PlayerUpgradeHandler>();
            var playerActionHandler = other.GetComponentInParent<PlayerActionHandler>();
            damage += playerActionHandler.extraDamageFromSoul;
            if (playerActionHandler.extraDamageFromSoul > 0)
                playerActionHandler.extraDamageFromSoul = 0;
            damage = ((damage * (playerUpgradeHandler.damageMultiplier) * (other.gameObject.GetComponentInParent<PlayerActionHandler>().attackMultiplier)));

            

            if (playerStats.staminaToDamage)
            {
                damage += (playerStats.maxStamina - playerStats.currentStamina) * playerStats.staminaToDamageMuliplier;
            }
            if (playerStats.helathLowAddDamage && (playerStats.currentHealth / playerStats.maxHealth) <= 0.25f)
            {
                damage *= playerUpgradeHandler.healthLowDamageMultiplier;
            }
            if (playerStats.useMoreStaminaToDamage)
            {
                damage *= playerUpgradeHandler.extraStaminaToDamageMultiplier;
            }
            if (playerStats.damageBuffOnDodge && playerActionHandler.damageAfterDodgeIsActive)
            {
                damage *= playerUpgradeHandler.dodgeDamageMultiplier;
            }
            if (playerStats.applyDotDebuff)
            {
                StartCoroutine(DOTDebuff.DOT_Debuff(this, damage, 3));
            }
            if (playerStats.extraDamageOnUndamaged && maxHealth == currentHealth)
            {
                damage *= 4;
            }
            if (playerStats.attackingReducesHealth)
            {
                damage *= 2f;
            }
            if (playerStats.glass)
            {
                damage *= 2;
            }
            print("Damage: " + damage /*+ " | Extra stamina damage: " + ((playerStats.maxStamina - playerStats.currentStamina) * playerStats.staminaToDamageMuliplier) + " | damage mulitplier from upgrades: " + other.gameObject.GetComponentInParent<PlayerUpgradeHandler>().damageMultiplier + " | Attack muliplier from spells: " + other.gameObject.GetComponentInParent<PlayerActionHandler>().attackMultiplier*/ );
            currentHealth -= damage;
            betterHealthBar.Damage(currentHealth, maxHealth);
            animator.SetBool("Hit", true);
            gameObject.GetComponent<EnemyAI>().rotated = false;
            if (playerStats.extraHealthOnHit)
            {
                playerStats.UpgradePlayerHealth((int)playerUpgradeHandler.flatExtraHealthOnHit);
            }
            if (playerStats.soulOnHit)
            {
                playerStats.currentSoul += 15;
            }
            if (currentHealth <= 0)
            {
                healthBar.gameObject.SetActive(false);
                GetComponent<EnemyManager>().enemyMode = EnemyManager.Mode.dead;
            }
            enemyAI.DisableCollider();
        }
    }

    public float countDown = 1;
    public void TakeImmolationHit()
    {
        countDown -= Time.deltaTime;
        if (countDown <= 0)
        {
           
            GetComponent<AudioSource>().pitch = 0.8f;
            GetComponent<AudioSource>().Play();
            Animator animator = gameObject.GetComponent<Animator>();
            var playerUpgradeHandler = playerStats.gameObject.GetComponentInParent<PlayerUpgradeHandler>();

            float damage = 25 * playerUpgradeHandler.spellDamageMuliplier;
            Instantiate(blood[(int)Random.Range(0, blood.Length - 1)], transform.position, Quaternion.identity);
            print("Damage from immolation: " + damage /*+ " | Extra stamina damage: " + ((playerStats.maxStamina - playerStats.currentStamina) * playerStats.staminaToDamageMuliplier) + " | damage mulitplier from upgrades: " + other.gameObject.GetComponentInParent<PlayerUpgradeHandler>().damageMultiplier + " | Attack muliplier from spells: " + other.gameObject.GetComponentInParent<PlayerActionHandler>().attackMultiplier*/ );
            currentHealth -= damage;
            betterHealthBar.Damage(currentHealth, maxHealth);
            animator.SetBool("Hit", true);
            gameObject.GetComponent<EnemyAI>().rotated = false;
            if (playerStats.extraHealthOnHit)
            {
                playerStats.UpgradePlayerHealth((int)playerUpgradeHandler.flatExtraHealthOnHit);
            }
            if (currentHealth <= 0)
            {
                healthBar.gameObject.SetActive(false);
                GetComponent<EnemyManager>().enemyMode = EnemyManager.Mode.dead;
            }
            countDown = 1;
            enemyAI.DisableCollider();
        }
    }

    public void TakeHit(int damage)
    {
        GetComponent<AudioSource>().pitch = 0.8f;
        GetComponent<AudioSource>().Play();
        Animator animator = gameObject.GetComponent<Animator>();
        Instantiate(blood[(int)Random.Range(0, blood.Length - 1)], transform.position, Quaternion.identity);
           
        currentHealth -= damage;
        betterHealthBar.Damage(currentHealth, maxHealth);
        animator.SetBool("Hit", true);
        gameObject.GetComponent<EnemyAI>().rotated = false;
        if (playerStats.extraHealthOnHit)
        {
            playerStats.UpgradePlayerHealth((int)playerUpgradeHandler.flatExtraHealthOnHit);
        }
        if (currentHealth <= 0)
        {
            healthBar.gameObject.SetActive(false);
            GetComponent<EnemyManager>().enemyMode = EnemyManager.Mode.dead;
        }
        enemyAI.DisableCollider();
    }

    public void TakeDOTHit(int damage)
    {
        
        Animator animator = gameObject.GetComponent<Animator>();
        Instantiate(blood[(int)Random.Range(0, blood.Length - 1)], transform.position, Quaternion.identity);

        currentHealth -= damage;
        betterHealthBar.Damage(currentHealth, maxHealth);
        //animator.SetBool("Hit", true);
        gameObject.GetComponent<EnemyAI>().rotated = false;
        if (currentHealth <= 0)
        {
            healthBar.gameObject.SetActive(false);
            GetComponent<EnemyManager>().enemyMode = EnemyManager.Mode.dead;
            animator.SetBool("Hit", true);
            animator.SetBool("Dead", true);
        }
        if (GetComponent<EnemyManager>().enemyMode == EnemyManager.Mode.dead) { return; }

        GetComponent<AudioSource>().pitch = 0.8f;
        GetComponent<AudioSource>().Play();
        enemyAI.DisableCollider();
    }

    public void LoseStamina(int amount)
    {
        currentStamina -= amount;
        regenTimer = 0;
    }

    private void RegnerateStamina()
    {
        if (currentStamina > maxStamina)
        {
            currentStamina = maxStamina;
        }
        else if (currentStamina < 0)
        {
            currentStamina = 0;
        }

       
        regenTimer += Time.deltaTime;
        if (regenTimer > 2f && currentStamina < maxStamina)
        {
            currentStamina += (staminaRegenRate * (1 + Time.fixedDeltaTime));
            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }
        }
        

    }

}

