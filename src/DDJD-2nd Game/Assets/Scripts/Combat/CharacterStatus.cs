using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;

public class CharacterStatus : MonoBehaviour
{
    [SerializeField]
    protected int _health;

    [SerializeField]
    protected int _maxHealth = 100;

    [SerializeField]
    protected bool _useMana = false;
    public bool UseMana
    {
        get { return _useMana; }
    }

    [SerializeField]
    [ConditionalField(nameof(_useMana), false, true)]
    protected int _mana;
    public int Mana
    {
        get { return _mana; }
        set { _mana = value; }
    }

    [SerializeField]
    [ConditionalField(nameof(_useMana), false, true)]
    protected int _maxMana = 100;

    [SerializeField]
    [ConditionalField(nameof(_useMana), false, true)]
    protected SerializedDictionary<Element, int> _elementMana =
        new SerializedDictionary<Element, int>();

    protected Action<int, Vector3, Vector3> _onDeath;
    public Action<int, Vector3, Vector3> OnDeath
    {
        get { return _onDeath; }
        set { _onDeath = value; }
    }

    protected virtual void Awake()
    {
        _health = _maxHealth;
        _mana = _maxMana;
    }

    // Update is called once per frame
    void Update() { }

    public void Reset()
    {
        _health = 0;
        RestoreHealth(_maxHealth);

        //Update _elementMana


        //Iterate through all elements
        List<Element> elements = new List<Element>(_elementMana.Keys);
        foreach (Element element in elements)
        {
            _elementMana[element] = 0;
            RestoreMana(element, _maxMana);
        }
    }

    public virtual void TakeDamage(
        GameObject damager,
        int damage,
        Vector3 hitPoint = default(Vector3),
        Vector3 hitDirection = default(Vector3)
    )
    {
        _health -= damage;
        if (_health <= 0)
        {
            _onDeath?.Invoke(damage, hitPoint, hitDirection);
        }
    }

    public bool HasEnoughMana(int manaCost)
    {
        return _mana >= manaCost;
    }

    public virtual bool ConsumeMana(Element element, int manaCost)
    {
        if (!_useMana)
            return true;
        int mana = _elementMana[element];
        if (mana < manaCost)
        {
            return false;
        }

        mana -= manaCost;
        _elementMana[element] = mana;
        return true;
    }

    public virtual void RestoreMana(Element element, int manaQuantity)
    {
        int mana = _elementMana[element];
        mana = Mathf.Min(mana + manaQuantity, _maxMana);
        _elementMana[element] = mana;
    }

    public int Health
    {
        get { return _health; }
        set { _health = value; }
    }

    public virtual void RestoreHealth(int healthQuantity)
    {
        _health = Mathf.Min(_health + healthQuantity, _maxHealth);
    }

    public bool HasMaxHealth()
    {
        return _health == _maxHealth;
    }

    public int MaxHealth
    {
        get { return _maxHealth; }
        set { _maxHealth = value; }
    }
}
