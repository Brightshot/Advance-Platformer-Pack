using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;


/// <summary>
/// Flame Simulation for Rocket
/// 
/// controls Rocket Logic
/// </summary>
public class Rocket : MonoBehaviour
{
    [SerializeField]private ParticleSystem flameParticle;
    public AdvanceJump jumpController;

    private bool boosting;
    private float duration;

    private void OnEnable()
    {
        AdvanceJump.boost += Boost;
        DashScript.dash_Event += Dash;
    }

    private void OnDisable()
    {
        AdvanceJump.boost -= Boost;
        DashScript.dash_Event -= Dash;
    }

    private void Boost()
    {
        boosting = true;
        if (jumpController.jumpType == Jump_Types.ExtendedJump)
        {
            InitiateFlameInLoop();
        }
        else if (jumpController.jumpType == Jump_Types.DoubleJump)
        {
            SetFlame(.8f);
        }
    }

    private bool stopped=true;
    private void Update()
    {
        if (jumpController == null) return;

        if (jumpController.jumpType == Jump_Types.ExtendedJump)
        {
            if (flameParticle.isPlaying && !stopped && !jumpController.isJumping)
            {
                flameParticle.Stop();
                stopped= true;
            }

            if (jumpController.isJumping)
            {
                stopped = false;
            }
        }

        var controller = jumpController.gameObject.GetComponent<PlatformerController>();

        //stop flame if the player is not dashing 
        if (flameParticle.isPlaying && !controller.performing_action && !boosting)
        {
            flameParticle.Stop();
        }

        //make boosting false when done playing
        if (!flameParticle.isPlaying)
        {
            boosting = false;
        }
    }

    private void Dash()
    {
        boosting = false;
        SetFlame(3);
    }

    public void SetFlame(float duration)
    {
        this.duration = duration;
        InitiateFlame();        
    }

    private void InitiateFlame()
    {
        if (!flameParticle.isPlaying)
        {
            var particle = flameParticle;
            var main = particle.main;

            main.duration = duration;

            flameParticle = particle;
        }

        flameParticle.Stop();
        flameParticle.Play();
    }

    private void InitiateFlameInLoop()
    {
        if (!flameParticle.isPlaying)
        {
            var particle = flameParticle;
            var main = particle.main;

            main.duration = 15;

            flameParticle.Play();
        }
    }
}
