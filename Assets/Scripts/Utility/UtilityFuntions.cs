using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilityFuntions
{
    public static void CopyCompleteParticleSystem(ParticleSystem source, ref ParticleSystem target)
    {
        var sourceMain = source.main;
        var targetMain = target.main;

        targetMain.duration = sourceMain.duration;
        targetMain.loop = sourceMain.loop;
        targetMain.startLifetime = sourceMain.startLifetime;
        targetMain.startSpeed = sourceMain.startSpeed;
        targetMain.startSize = sourceMain.startSize;
        targetMain.startColor = sourceMain.startColor;
        targetMain.gravityModifier = sourceMain.gravityModifier;

        // Copiar Emission Module
        var sourceEmission = source.emission;
        var targetEmission = target.emission;
        targetEmission.rateOverTime = sourceEmission.rateOverTime;
        targetEmission.rateOverDistance = sourceEmission.rateOverDistance;

        // Copiar Shape Module
        var sourceShape = source.shape;
        var targetShape = target.shape;
        targetShape.shapeType = sourceShape.shapeType;
        targetShape.radius = sourceShape.radius;

        // Copiar Color over Lifetime
        var sourceColorOverLifetime = source.colorOverLifetime;
        var targetColorOverLifetime = target.colorOverLifetime;
        targetColorOverLifetime.enabled = sourceColorOverLifetime.enabled;
        targetColorOverLifetime.color = sourceColorOverLifetime.color;

        target.transform.position = source.transform.position;
        target.transform.rotation = source.transform.rotation;
    }
}
