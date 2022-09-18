using System;
using System.Text;
using UnityEngine;

public class Health : MonoBehaviour
{
    public bool Dead { get; private set; }
    
    [Tooltip("Health Points")]
    [SerializeField] private float health = 100f;

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    public void DealDamage(float damage)
    {
        if (health == 0)
            return;
        
        string gameObjectName = gameObject.name;
        
        if (health - damage <= 0f)
        {
            health = 0f;
            Dead = true;
            Debug.Log($"{gameObjectName} damaged for {damage}. {gameObjectName} has been killed!");
        }
        else
        {
            health -= damage; 
            Debug.Log($"{gameObjectName} damaged for {damage}; {health} remaining.");
        }
    }
}
