﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All "human-like" craft are considered ShellCores. These crafts are intelligent and all air-borne. This includes player ShellCores.
/// </summary>
public class ShellCore : AirCraft {

    protected LineRenderer lineRenderer;
    public GameObject glowPrefab;
    public Material tractorMaterial;
    Transform coreGlow;
    Transform targetGlow;
    Draggable target;

    protected override void OnDeath()
    {
        SetTractorTarget(null);
        base.OnDeath();
    }

    private void OnDestroy()
    {
        if(coreGlow)
            Destroy(coreGlow.gameObject);
        if (targetGlow)
            Destroy(targetGlow.gameObject);
    }

    // TODO: these will be either enemies or allies, most allies and a few enemies can be interacted with.
    protected override void Start()
    {
        base.Start(); // base start
        transform.position = spawnPoint;
        // initialize instance fields


        if(!coreGlow)
            coreGlow = Instantiate(glowPrefab, null, true).transform;
        if (!targetGlow)
            targetGlow = Instantiate(glowPrefab, null, true).transform;

        coreGlow.gameObject.SetActive(false);
        targetGlow.gameObject.SetActive(false);
    }

    protected override void BuildEntity()
    {
        if(!transform.Find("TractorBeam"))
        {
            GameObject childObject = new GameObject();
            childObject.transform.SetParent(transform, false);
            lineRenderer = childObject.AddComponent<LineRenderer>();
            lineRenderer.material = tractorMaterial;
            lineRenderer.startWidth = 0.1F;
            lineRenderer.endWidth = 0.1F;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            childObject.name = "TractorBeam";
        }
        base.BuildEntity();
    }

    protected override void Awake()
    {
        respawns = true;
        base.Awake(); // base awake
    }

    protected void FixedUpdate()
    {
        if (target && !isDead) // Update tractor beam physics
        {
            Rigidbody2D rigidbody = target.GetComponent<Rigidbody2D>();
            if (rigidbody)
            {
                //get direction
                Vector3 dir = transform.position - target.transform.position;
                //get distance
                float dist = dir.magnitude;
                //DebugMeter.AddDataPoint((dir.normalized * (dist - 2F) * 10000f * Time.fixedDeltaTime).magnitude);

                if (target.GetComponent<EnergySphereScript>())
                {
                    rigidbody.AddForce(dir.normalized * 100f);
                }
                else if (dist > 2f)
                {
                    rigidbody.AddForce(dir.normalized * (dist - 2F) * 4000f * Time.fixedDeltaTime);
                }
            }
        }
    }

    protected void TractorBeamUpdate()
    {
        if (target && (target.transform.position - transform.position).magnitude > 100)
        {
            SetTractorTarget(null);
        }
        if (!target) // Don't grab energy when the craft is pulling something more important
        {

            EnergySphereScript[] energies = FindObjectsOfType<EnergySphereScript>();

            Transform closest = null;
            float closestD = float.MaxValue;

            for (int i = 0; i < energies.Length; i++)
            {
                float sqrD = Vector3.SqrMagnitude(transform.position - energies[i].transform.position);
                if (closest == null || sqrD < closestD)
                {
                    closestD = sqrD;
                    closest = energies[i].transform;
                }
            }
            if (closest && closestD < 160 && GetTractorTarget() == null)
                SetTractorTarget(closest.gameObject.GetComponent<Draggable>());
        }

        if (target && !isDead) // Update tractor beam graphics
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new Vector3[] { transform.position, target.transform.position });

            coreGlow.gameObject.SetActive(true);
            targetGlow.gameObject.SetActive(true);

            coreGlow.transform.position = transform.position;
            targetGlow.transform.position = target.transform.position;
        }
        else
        {
            lineRenderer.positionCount = 0;
            coreGlow.gameObject.SetActive(false);
            targetGlow.gameObject.SetActive(false);
        }
    }
    protected override void Update() {
        base.Update(); // base update
        TractorBeamUpdate();
    }

    public void SetTractorTarget(Draggable newTarget)
    {
        lineRenderer.enabled = (newTarget != null);
        target = newTarget;
    }

    public Draggable GetTractorTarget()
    {
        return target;
    }
}
