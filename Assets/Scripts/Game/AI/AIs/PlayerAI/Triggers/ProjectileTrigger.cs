using Data.GameManagement;
using Enums;
using Game.Spells;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileTrigger : Sensor
{
    #region Members


    #endregion


    #region Update

    private void Update()
    {
        IsTriggered = false;
        List<Spell> spellsToRemove = new List<Spell>();

        foreach (Spell spell in m_Spells)
        {
            if (spell.IsDestroyed())
            {
                spellsToRemove.Add(spell);
                continue;
            }

            if (spell.SpellData.SpellType != ESpellType.Projectile && spell.SpellData.SpellType != ESpellType.MultiProjectiles)
                continue;

            if (IsCharacterInProjectilePath((Projectile)spell, m_Controller))
                IsTriggered = true;
        }

        foreach (var spell in spellsToRemove)
        {
            m_Spells.Remove(spell);
        }

        UpdateColor();
    }

    #endregion


    #region Check collision

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // check if is spell
        var spell = collision.GetComponent<Spell>();
        if (spell == null)
            return;

        // check is enemy
        if (spell.Controller.Team == m_Controller.Team)
            return;

        m_Spells.Add(spell);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // check if is spell
        var spell = collision.GetComponent<Spell>();
        if (spell == null)
            return;

        // check is enemy
        if (spell.Controller.Team == m_Controller.Team)
            return;

        m_Spells.Remove(spell);
    }



    #endregion


    #region Projectiles

    public static bool IsCharacterInProjectilePath(Projectile projectile, Controller controller)
    {
        // projectile not an issue if hasn't reached max hight
        if (projectile.IsDestroyed() || projectile.transform.rotation.z > 0)
        {
            return false;
        }

        RaycastHit2D hit = Physics2D.BoxCast(
            origin:     projectile.transform.position, 
            size:       new Vector2(1f, Settings.SpellSizeFactor * projectile.SpellData.Size), 
            angle:      0f, 
            direction:  (projectile.Target - projectile.transform.position).normalized, 
            distance:   10f, 
            layerMask:  LayerMask.GetMask("Player")
        );

        return hit.collider != null && hit.collider.GetComponent<Controller>() == controller;
    }

    public static bool CheckControllerInBetweenPos(Vector2 posStart, Vector2 posEnd, Controller controller, float offset = 0)
    {
        RaycastHit2D hit = Physics2D.Raycast(posStart + new Vector2(0, offset), (posEnd + new Vector2(offset, 0) - posStart).normalized, 10f, LayerMask.GetMask("Player"));
        if (hit.collider != null && hit.collider.GetComponent<Controller>() == controller)
        {
            Debug.DrawLine(posStart, posStart, Color.red, 0.5f);
            return true;
        }

        return false;
    }

    #endregion
}
