﻿using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSettings", menuName = "Settings/Projectile", order = 1)]
public class ProjectileSettings : ReadonlySettings
{
	public string PlayerTag = "Player";
	public string WallTag = "Wall";

	public float MaxRadiansOnAutoTarget = 7f;
	public float VelocityChangeOnWallHit = .1f;

	public Material StandardProjectileMaterial;
	public Material BounceCubeMaterial;
	public Material ExplosionCubeMaterial;
	public Material AutoAimCubeMaterial;
}
