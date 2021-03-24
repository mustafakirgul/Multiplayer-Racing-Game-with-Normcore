using UnityEngine;

[CreateAssetMenu]
public class ItemBase : ScriptableObject
{
    public ItemType _ItemType;

    /// <summary>
    /// Attack is the melee attack force, should be a percentage of effectiveness between 0 and 1
    /// </summary>
    //Read only data
    [SerializeField]
    private float f_attack = 0f;

    /// <summary>
    /// Defense % of damage decreased when hit by a weapon or melee, should be a percentage of effectiveness between 0 and 1
    ///  float calculatedDamage = (1 - armourDefenseModifier - tempDefenseModifier * 0.5f) * damage;
    /// </summary>
    [SerializeField] private float f_defense = 0f;

    /// <summary>
    /// Accleration of the car, in controller: moveInput *= ((1 + acceleration))
    /// Normal should be 0, any increase is a % for ex, 0.1 is 10% so on
    /// </summary>
    [SerializeField] private float f_acceleration = 0f;

    /// <summary>
    /// Extra health
    /// </summary>
    [SerializeField] private float f_health = 0f;


    /// <summary>
    /// Top Speed of the car, in controller:  if (CarRB.velocity.magnitude > (MaxSpeed * (1 + maxSpeedModifier)))
    /// Normal should be 0, any increase is a % for ex, 0.1 is 10% so on
    /// </summary>
    [SerializeField] private float f_TopSpeed = 0f;

    /// <summary>
    /// How fast is the car turning when pressing left or right, in controller:  (1 + tempSpeedModifier) * turnInput * turnSpd * Time.deltaTime;
    /// </summary>
    [SerializeField]
    private float f_turnSpd = 0f;

    /// <summary>
    /// FireRate of the in game weapon pick up only!!! (Primary or Secondary FireRates are set on the car controller
    /// and the projectile base of the secondary weapon which contains the weapon fire rate, the higher this number the faster the fire rate
    /// </summary>
    [SerializeField] private float f_fireRate = 0f;

    /// <summary>
    /// How many shots is given by a ammo pick up for this weapon by default this should always be 1
    /// </summary>
    [SerializeField] private float f_ammoPickUpEfficency = 1f;

    /// <summary>
    /// The Rigidbody Weight of the Car Controller, not sure if increasing this by alot would affect speed/acceleration
    /// </summary>
    [SerializeField] private float f_weight = 0f;

    /// <summary>
    /// How fast will the boost recharge, this is the number in seconds
    /// In controller: Time.deltaTime / (boostCooldownTime * (1 - tempBoostModifier));
    /// </summary>
    [SerializeField] private float f_boostTimer = 0f;

    /// <summary>
    /// This is the boost force during a boost, should balance this with the boost recharge timer
    /// In controller: CarRB.AddForce(transform.forward * (dashForce), ForceMode.VelocityChange);
    /// </summary>
    [SerializeField] private float f_boostForce = 0f;

    /// <summary>
    /// The visual Index of the equipment to be applied to the car prefab, need to implement for melee items
    /// </summary>
    [SerializeField] private float f_itemVisualIndex;

    [SerializeField] private string itemText = "";

    public Texture2D itemImage;

    public float m_Attack => f_attack;
    public float m_Defense => f_defense;
    public float m_acceleration => f_acceleration;
    public float m_health => f_health;
    public float m_TopSpeed => f_TopSpeed;
    public float m_turnSpd => f_turnSpd;
    public float m_ammoPickUpEfficiency => f_ammoPickUpEfficency;
    public float m_weight => f_weight;
    public float m_boostTimer => f_boostTimer;
    public float m_boostForce => f_boostForce;
    public float m_inGamePickUpWeaponFireRate => f_fireRate;
    public string m_text => itemText;
    public float m_itemVisualIndex => f_itemVisualIndex;

    public Texture2D m_image => itemImage;

    public GameObject WeaponProjectile;
    public GameObject GetProjectileForWeapon => WeaponProjectile;

    [SerializeField] private GameObject m_CosmeticModelToBeApplied;
}