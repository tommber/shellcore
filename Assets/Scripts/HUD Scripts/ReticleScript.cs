﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// GUI Reticle to display the target of the player core
/// </summary>
public class ReticleScript : MonoBehaviour {

    private PlayerCore craft; // the player the reticle is assigned to
    private TargetingSystem targSys; // the targeting system of the player
    private bool initialized; // if the reticle has been initialized
    private Transform shellimage; // the image representations of the target's shell and core health
    private Transform coreimage;
    public EventSystem system;

    /// <summary>
    /// Initializes the reticle
    /// </summary>
    public void Initialize(PlayerCore player)
    {
        craft = player;
        targSys = craft.GetTargetingSystem(); // grab the targeting system
        shellimage = transform.Find("Target Shell"); // grab the sprites
        coreimage = transform.Find("Target Core");
        initialized = true; // initialization complete
    }

    /// <summary>
    /// Finds a target to assign to the player at the given mouse position
    /// </summary>
    private void FindTarget() {

        // TODO: To say this needs despaghettification would be an understatement...
        // despaghettified a little :)
        /*
         * IInteractable
         * - bool Interact() - returns whether the interaction was successful
         * - int getPriority() - returns interaction priority, for example: 
         *   a component with quest dialogue would return high priority, while 
         *   something that acts only as a target would return low priority
         * 
         * 
         * This method:
         *   Find all interactable objects in the pointer position, add to 'List'
         *   compare GameObjects int List to those found in quest interaction overrides list
         *      if there's a match : 
         *          call the quest node's Calculate()
         *          return;
         *   Sort List by priority
         *   Foreach (IInteractable interactable in List)
         *      if(interactable.Interact())
         *         break;
         */

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // create a ray
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity); // get an array of all hits

        if (!DroneCheck(targSys.GetTarget(), hits) && hits.Length != 0) // check if there are actually any hits
        {
            Draggable draggableTarget = hits[0].transform.gameObject.GetComponent<Draggable>();

            if (draggableTarget)
            {
                if (targSys.GetTarget() == draggableTarget.transform 
                && (!targSys.GetTarget().GetComponent<Entity>() 
                || targSys.GetTarget().GetComponent<Entity>().faction == craft.faction))
                {
                    PlayerCore player = craft.GetComponent<PlayerCore>();
                    player.SetTractorTarget((player.GetTractorTarget() == draggableTarget) ? null : draggableTarget);
                }
                targSys.SetTarget(draggableTarget.transform); // set the target to the clicked craft's transform
                AdjustReticleBounds(GetComponent<SpriteRenderer>(), draggableTarget.transform);
                return; // Return so that the next check doesn't happen
            }


            ITargetable curTarg = hits[0].transform.gameObject.GetComponent<ITargetable>();
            // grab the first one's craft component, others don't matter
            if (curTarg != null && !curTarg.GetIsDead() && curTarg as Entity != craft) 
                // if it is not null, dead or the player itself
            {
                // TODO: synchronize this with the proximity script
                if (!craft.GetIsInteracting() && targSys.GetTarget() == curTarg.GetTransform()
                    && (curTarg.GetTransform().position - craft.transform.position).sqrMagnitude < 100) //Interact with entity
                {
                    if(TaskManager.interactionOverrides.ContainsKey(curTarg.GetName())) //If there's a task overriding the default dialogue, use that
                    {
                        TaskManager.interactionOverrides[curTarg.GetName()].Invoke();
                    }
                    else if (curTarg.GetDialogue() as Dialogue)
                        DialogueSystem.StartDialogue(curTarg.GetDialogue() as Dialogue, curTarg as Entity);
                }

                targSys.SetTarget(curTarg.GetTransform()); // set the target to the clicked craft's transform
                AdjustReticleBounds(GetComponent<SpriteRenderer>(), curTarg.GetTransform());
                return; // Return so that the next check doesn't happen
            }
            targSys.SetTarget(null); // otherwise set the target to null
        }
        else {
            targSys.SetTarget(null); // otherwise set the target to null
        }
    }

    /// <summary>
    /// Used to update the reticle representation
    /// </summary>
    private void SetTransform() {
        Transform target = targSys.GetTarget(); // get the target
        if(target != null)
        {
            transform.position = target.position; // update reticle position
            GetComponent<SpriteRenderer>().enabled = true; // enable the sprite renderers
        }
        else GetComponent<SpriteRenderer>().enabled = false;

        ITargetable targetCraft = target ? target.GetComponent<ITargetable>() : null; // if target is an entity
        UpdateReticleHealths(shellimage, coreimage, targetCraft);
    }

	// Update is called once per frame
	void Update () {
        if (initialized) // check if it is safe to update
        {
            foreach(var tuple in secondariesByObject)
            {
                SetSecondaryReticleTransform(tuple.Item1, tuple.Item2);
            }

            if (Input.GetMouseButtonDown(0) && !system.IsPointerOverGameObject()) // mouse click, scan for target
            {
                FindTarget(); // find target
            }
            if (targSys.GetTarget() != null) // check if the reticle should update
            {
                ITargetable targetCraft = targSys.GetTarget().GetComponent<ITargetable>();

                if (targetCraft != null && targetCraft.GetIsDead()) { 
                    // check if the target craft is dead
                    targSys.SetTarget(null); // if so remove the target lock
                }
            }
            SetTransform(); // update the transform of the reticle accordingly

            // Toggle tractor beam
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (targSys.GetTarget())
                {
                    Draggable draggable = targSys.GetTarget().GetComponent<Draggable>();

                    // it's draggable if it's not an entity or it's a draggable entity with the same faction
                    if (draggable && (targSys.GetTarget().position - craft.transform.position).sqrMagnitude <= 400
                    && (!targSys.GetTarget().GetComponent<Entity>() 
                    || targSys.GetTarget().GetComponent<Entity>().faction == craft.faction))
                    {
                        craft.SetTractorTarget((craft.GetTractorTarget() == draggable) ? null : draggable);
                    } else craft.SetTractorTarget(null);
                }
                else craft.SetTractorTarget(null);
            }
        }
	}

    private void SetSecondaryReticleTransform(Entity ent, Transform reticle)
    {
        if(ent != null)
        {
            reticle.transform.position = ent.transform.position; // update reticle position
            reticle.GetComponent<SpriteRenderer>().enabled = true; // enable the sprite renderers
        }
        else RemoveSecondaryTarget(ent);

        var shellimage = reticle.Find("Target Shell");
        var coreimage = reticle.Find("Target Core");

        UpdateReticleHealths(shellimage, coreimage, ent);
    }

    private bool DroneCheck(Transform possibleDrone, RaycastHit2D[] hits)
    {
        var check = possibleDrone && possibleDrone.GetComponent<Drone>() &&
            possibleDrone.GetComponent<Drone>().GetOwner() != null
                 && possibleDrone.GetComponent<Drone>().GetOwner().Equals(craft)
                    && (hits.Length == 0 || hits[0].transform != possibleDrone);
        if(check)
        {
            if (hits.Length == 0 ||  hits[0].transform != craft.transform) {
                var pos = Input.mousePosition;
                pos.z = 10;
                possibleDrone.GetComponent<Drone>().CommandMovement(Camera.main.ScreenToWorldPoint(pos));
                targSys.SetTarget(null);
            } else if (hits[0].transform == craft.transform)
            {
                possibleDrone.GetComponent<AirCraftAI>().follow(craft.transform);
                targSys.SetTarget(null);
            }
        }
        return check;
    }

    private void AdjustReticleBounds(SpriteRenderer renderer, Transform ent)
    {
        Vector3 targSize = ent.GetComponent<SpriteRenderer>().bounds.size; // adjust the size of the reticle
        float followedSize = Mathf.Max(targSize.x + 1.5F, targSize.y + 1.5F); // grab the maximum bounded size of the target
        renderer.size = new Vector2(followedSize, followedSize); // set the scale to match the size of the target
    }

    private void UpdateReticleHealths(Transform shellHealth, Transform coreHealth, ITargetable targetCraft)
    {
        if(targetCraft != null)
        {
            // show craft related information

            shellHealth.GetComponentInChildren<SpriteRenderer>().enabled = true;
            shellHealth.GetComponentInChildren<SpriteRenderer>().color = 
                FactionColors.colors[targetCraft.GetFaction()];
            coreHealth.GetComponentInChildren<SpriteRenderer>().enabled = true;

            float[] targHealth = targetCraft.GetHealth(); // get the target current health
            float[] targMax = targetCraft.GetMaxHealth(); // get the target max health

            // adjust the image scales according to the health ratios
            Vector3 scale = shellHealth.localScale;

            scale.x = targHealth[0] / targMax[0];

            shellHealth.localScale = scale;

            scale = coreHealth.localScale;
            scale.x = targHealth[1] / targMax[1];

            coreHealth.localScale = scale;
        }
        else
        {
            // disable the craft related info
            shellHealth.GetComponentInChildren<SpriteRenderer>().enabled = false;
            coreHealth.GetComponentInChildren<SpriteRenderer>().enabled = false;
        }
    }

    public GameObject secondaryReticlePrefab;
    private List<(Entity, Transform)> secondariesByObject = new List<(Entity, Transform)>();

    public void AddSecondaryTarget(Entity ent)
    {
        var reticle = Instantiate(secondaryReticlePrefab, ent.transform.position, Quaternion.identity, transform);
        AdjustReticleBounds(reticle.GetComponent<SpriteRenderer>(), ent.transform);
        secondariesByObject.Add((ent, reticle.transform));
        targSys.AddSecondaryTarget(ent);
    }

    public void RemoveSecondaryTarget(Entity ent)
    {
        targSys.RemoveSecondaryTarget(ent);
    }

    public void RemoveSecondaryTarget((Entity, Transform) tuple)
    {
        if(secondariesByObject.Contains(tuple)) 
            secondariesByObject.Remove(tuple);
        targSys.RemoveSecondaryTarget(tuple.Item1);
        Destroy(tuple.Item2.gameObject);

    }

    public void ClearSecondaryTargets()
    {
        while(secondariesByObject.Count > 0)
        {
            RemoveSecondaryTarget(secondariesByObject[0]);
        }

        targSys.ClearSecondaryTargets();
    }
}
