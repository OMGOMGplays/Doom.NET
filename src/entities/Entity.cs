﻿using System;
using System.Collections.Generic;

using DoomNET.Resources;

namespace DoomNET.Entities;

/// <summary>
/// An entity, usually living and with health, also moving with velocities and gravity applied to it
/// </summary>
public class Entity
{
    public Vector3 position; // This entity's current position
    public Quaternion rotation; // This entity's current rotation
    public BBox bbox; // This entity's bounding box
    public string id; // This entity's identity
    public float health => _health;

    public virtual EntityType type { get; set; } // This entity's type, e.g. brush entity or other
    
    protected float _health; // The amount of health this entity has

    private Vector3 velocity; // This entity's current velocity
    private bool alive; // Is this entity alive?
    private Entity target; // The entity this entity's targeting
    private Entity lastAttacker; // The last entity to attack this entity

    public Entity() { }

    public Entity( Vector3 position )
    {
        this.position = position;
    }

    /// <summary>
    /// A way to initialize this entity, default for all entities
    /// </summary>
    public void Spawn()
    {
        // Null the velocity
        velocity = Vector3.Zero;

        // This entity is now alive
        alive = true;

        // Subscribe to the OnUpdate event
        Game.OnUpdate += Update;

        // Call the OnSpawn event
        OnSpawn();
    }

    /// <summary>
    /// Things to do every frame
    /// </summary>
    protected virtual void Update()
    {

    }

    /// <summary>
    /// Handle movement, caused by velocity
    /// </summary>
    protected void HandleMovement()
    {
        // Position is affected by velocity
        position += velocity * Game.deltaTime;

        // Velocity decreases with time (effectively drag)
        velocity *= ( 1 - 0.1f * Game.deltaTime );

        // If the velocity's magnitude <= 0.5, it's effectively zero, so zero it out for the sake of ease
        if (velocity.Magnitude() <= 0.5f)
        {
            velocity = Vector3.Zero;
        }
    }

    /// <summary>
    /// Get the current health of this entity
    /// </summary>
    public float GetHealth()
    {
        return _health;
    }

    /// <summary>
    /// Get the ID of this entity
    /// </summary>
    public string GetID()
    {
        return id;
    }

    /// <summary>
    /// Gets this entity's position
    /// </summary>
    public Vector3 GetPosition()
    {
        return position;
    }

    /// <summary>
    /// Gets this entity's velocity
    /// </summary>
    public Vector3 GetVelocity()
    {
        return velocity;
    }

    public Quaternion GetRotation()
    {
        return rotation;
    }
    
    /// <summary>
    /// Gets this entity's bounding box
    /// </summary>
    public BBox GetBBox()
    {
        return bbox;
    }

    /// <summary>
    /// Is this entity alive?
    /// </summary>
    /// <returns><see langword="true"/> if alive, <see langword="false"/> if dead</returns>
    public bool IsAlive()
    {
        return alive;
    }

    /// <summary>
    /// Sets this entity's ID
    /// </summary>
    public void SetID( string id )
    {
        this.id = id;
    }

    /// <summary>
    /// Set the target of this entity, e.g. an enemy should target the <see cref="Player"/>
    /// </summary>
    /// <param name="target">The specific entity we wish to target, 0 should always be the <see cref="Player"/></param>
    public void SetTarget( Entity target )
    {
        // Can't target a dead entity
        if (!target.IsAlive())
        {
            return;
        }

        this.target = target;
    }

    /// <summary>
    /// Set the target of this entity by an ID, e.g. an enemy should target the <see cref="Player"/>
    /// </summary>
    /// <param name="targetID">The ID of the entity we wish to target, 0 should always be the <see cref="Player"/></param>
    public void SetTarget( string targetID )
    {
        target = Game.currentScene?.FindEntity( targetID );
    }

    /// <summary>
    /// Set this entity's position by a <see cref="Vector3"/>
    /// </summary>
    /// <param name="position">The new, desired position of this entity</param>
    public void SetPosition( Vector3 position )
    {
        this.position = position;
    }

    /// <summary>
    /// Set this entity's velocity by a <see cref="Vector3"/>
    /// </summary>
    /// <param name="velocity">The new, desired velocity of this entity</param>
    public void SetVelocity( Vector3 velocity )
    {
        this.velocity = velocity;
    }

    /// <summary>
    /// Set this entity's rotation from a <see cref="Quaternion"/>
    /// </summary>
    /// <param name="rotation">The new, desired rotation of this entity</param>
    public void SetRotation( Quaternion rotation )
    {
        this.rotation = rotation;
    }

    /// <summary>
    /// Main use for this is for when a brush is turned into an entity
    /// </summary>
    /// <param name="bbox">The new bounding box of this entity</param>
    public void SetBBox( BBox bbox )
    {
        this.bbox = bbox;
    }

    /// <summary>
    /// Face the current entity towards another, e.g. the player
    /// </summary>
    /// <param name="entity">The desired entity we wish to look at</param>
    public void LookAtEntity( Entity entity = null )
    {
        // Do math
    }

    /// <summary>
    /// Subtract this entity's health by the parameter and trigger related events
    /// </summary>
    /// <param name="damage">The amount of damage this entity should take</param>
    /// <param name="source">The source entity of the damage</param>
    public virtual void TakeDamage( float damage, Entity source = null )
    {
        // We've been damaged by someone or something!
        // How queer! We must log this to the console immediately!!
        Console.WriteLine( $"Entity {this} took damage.\n" +
                          $"\tDamage: {damage}\n" +
                          $"\tSource: {( source != null ? source : "N/A" )}\n" +
                          $"\tNew health: {health - damage}\n" );

        //
        // I guess we're taking damage now
        //

        // Set the last attacker variable appropriately
        lastAttacker = source;

        // Lower this entity's health by the set amount of damage
        _health -= damage;

        // This entity has taken damage! Call the relevant event
        OnDamage();
    }

    /// <summary>
    /// Generate an ID for this entity.
    /// </summary>
    public void CreateID()
    {
        // Easier to use variable for the list of entities in the current scene
        List<Entity> entities = Game.currentScene?.GetEntities();
        
        // For every entity...
        for (int i = 0; i < entities?.Count; i++)
        {
            // If the entity we're currently checking is us
            if (entities[i] == this)
            {
                // If we are a player
                if (this is Player)
                {
                    // We're lucky! We have that designation on us now
                    SetID("player");
                    return; // We don't want to overwrite that special ID
                }
                
                // We're just some regular joe, set our entity ID appropriately
                SetID($"entity {i}");
            }
        }
    }

    #region ONEVENTS
    /// <summary>
    /// Call non-input-taking event
    /// </summary>
    /// <param name="eEvent">Desired event to do to this entity</param>
    public void OnEvent( EntityEvent eEvent, Entity source = null )
    {
        switch (eEvent)
        {
            case EntityEvent.None: // Do nothing (why'd you want this?)
                break;

            case EntityEvent.Kill: // Kill this entity
                OnDeath();
                break;

            case EntityEvent.Delete: // Delete this entity
                // Remove this entity from the current scene
                Game.currentScene?.RemoveEntity(this);
                
                // Also kill this entity, for good measure
                OnDeath();
                break;

            default: // Most likely happens when an invalid event was attempted on this entity
                return;
        }
    }

    /// <summary>
    /// Call an event that takes an <see langword="int"/> for a value.
    /// </summary>
    /// <param name="eEvent">Desired event to do to this entity.</param>
    /// <param name="iValue">Value as <see langword="int"/>.</param>
    public void OnEvent( EntityEvent eEvent, int iValue, Entity source = null )
    {
        switch (eEvent)
        {
            case EntityEvent.TakeDamage: // This entity should take iValue damage
                TakeDamage( iValue, source );
                break;
        }
    }

    /// <summary>
    /// Call an event that takes a <see langword="float"/> for a value.
    /// </summary>
    /// <param name="eEvent">Desired event to do to this entity.</param>
    /// <param name="fValue">Value as <see langword="float"/>.</param>
    public void OnEvent( EntityEvent eEvent, float fValue, Entity source = null )
    {
        switch (eEvent)
        {
            case EntityEvent.TakeDamage: // This entity should take fValue damage
                TakeDamage( fValue, source );
                break;
        }
    }

    /// <summary>
    /// Call an event hat takes a <see langword="bool"/> for a value.
    /// </summary>
    /// <param name="eEvent">Desired event to do to this entity.</param>
    /// <param name="bValue">Value as <see langword="bool"/>.</param>
    public void OnEvent( EntityEvent eEvent, bool bValue, Entity source = null )
    {
        switch (eEvent)
        {
            default:
                break;
        }
    }

    /// <summary>
    /// Call an event that takes a <see cref="Vector3"/> for a value.
    /// </summary>
    /// <param name="eEvent">Desired event to do to this entity.</param>
    /// <param name="vValue">Value as <see cref="Vector3"/>.</param>
    public void OnEvent( EntityEvent eEvent, Vector3 vValue, Entity source = null )
    {
        switch (eEvent)
        {
            case EntityEvent.SetPosition: // Set this entity's position according to vValue
                SetPosition( vValue );
                break;
        }
    }

    /// <summary>
    /// Call an event that takes a <see cref="Quaternion"/> for a value.
    /// </summary>
    /// <param name="eEvent">Desired event to do to this entity.</param>
    /// <param name="qValue">Value as <see cref="Quaternion"/>.</param>
    public void OnEvent( EntityEvent eEvent, Quaternion qValue, Entity source = null )
    {
        switch (eEvent)
        {
            case EntityEvent.SetRotation:
                SetRotation( qValue );
                break;
        }
    }

    /// <summary>
    /// Call an event that takes a <see cref="Entity"/> for a value.
    /// </summary>
    /// <param name="eEvent">Desired event to do to this entity.</param>
    /// <param name="eValue">Value as <see cref="Entity"/>.</param>
    public void OnEvent(EntityEvent eEvent, Entity eValue, Entity source = null)
    {
        switch (eEvent)
        {
            case EntityEvent.SpawnEntity:
                (this as EntitySpawner)?.SpawnEntity(eValue);
                break;
        }
    }

    /// <summary>
    /// Call an event that takes a <see cref="BBox"/> for a value.
    /// </summary>
    /// <param name="eEvent">Desired event to do to this entity.</param>
    /// <param name="bbValue">Value as <see cref="BBox"/>.</param>
    public void OnEvent( EntityEvent eEvent, BBox bbValue, Entity source = null )
    {
        switch (eEvent)
        {
            case EntityEvent.SetBBox: // Set this entity's BBox according to bValue
                SetBBox( bbValue );
                break;
        }
    }
    #endregion // ONEVENTS

    /// <summary>
    /// Per entity definition of what to do when they've spawned
    /// </summary>
    protected virtual void OnSpawn()
    {
    }

    /// <summary>
    /// Things to do when this entity takes damage
    /// </summary>
    protected virtual void OnDamage()
    {
        if (health <= 0) // Is this entity now considered dead?
        {
            // Call the OnDeath event
            OnDeath();
        }
        else if (health <= -25.0f) // Should they gib?
        {
            // Call the OnXDeath event
            OnXDeath();
        }
    }

    /// <summary>
    /// Things to do when this entity dies
    /// </summary>
    protected virtual void OnDeath()
    {
        // This entity is no longer alive
        alive = false;

        // Remove this entity from the update list
        Game.OnUpdate -= Update;

        // Log to the console that this entity has died!
        Console.WriteLine( $"Entity {this} has died.\n" +
                          $"\tLast attacker: {lastAttacker}" );
        
        // Remove the entity from the current scene... Maybe...
        Game.currentScene?.RemoveEntity(this);
    }

    /// <summary>
    /// Things to do when this entity dies a gory death
    /// </summary>
    protected virtual void OnXDeath()
    {
        // Also trigger OnDeath, but replace their sprite with a gory version
        OnDeath();
    }

    public override string ToString()
    {
        return $"{GetType()} (\"{(string.IsNullOrEmpty(GetID()) ? "N/A" : GetID())}\")";
    }
}