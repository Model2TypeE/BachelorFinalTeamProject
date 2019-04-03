﻿using System;
using UnityEngine;
using UnityEngine.Experimental.Input;
using Random = UnityEngine.Random;

public abstract class InputSystemMonoBehaviour : MonoBehaviour
{
	private void OnEnable() => RegisterActions();
	private void OnDisable() => UnregisterActions();

	protected abstract void RegisterActions();
	protected abstract void UnregisterActions();
}

public class PlayerCharacter : InputSystemMonoBehaviour
{
	private enum ControlType { Mouse, Controller }

	[SerializeField] private InputMaster inputMaster;
	[SerializeField] private Transform projectileOrbitCenter;
	[SerializeField] private GameObject loadedProjectile;
	[SerializeField] private int controlDeviceIndex;
	[SerializeField] private ControlType controlType;

	[Space]
	[SerializeField] private float movespeedMod;

	[Header("Projectile Settings")]
	[SerializeField] private float projectileOrbitSpeed;
	[SerializeField] private float projectileOrbitDist;
	[SerializeField] private float projectileOrbitHeightMod;
	[SerializeField] private float projectileRotationMin;
	[SerializeField] private float projectileRotationMax;

	private GameManager gameManager;
	private CharacterController charController;
	private InputMaster input;

	private Vector2 movementInput;
	private Vector2 aimInput;

	#region ignore this for now
	protected override void RegisterActions()
	{
		input.Player.Movement.performed += UpdateMovementControlled;
		input.Player.Aim.performed += UpdateLookRotationControlled;
		input.Player.Shoot.performed += TriggerShotControlled;
	}

	protected override void UnregisterActions()
	{
		input.Player.Movement.performed -= UpdateMovementControlled;
		input.Player.Aim.performed -= UpdateLookRotationControlled;
		input.Player.Shoot.performed -= TriggerShotControlled;
	}

	private void Awake()
	{
		gameManager = GameManager.Instance;
		input = gameManager.InputMaster;
		charController = GetComponent<CharacterController>();
	}

	private void UpdateMovementControlled(InputAction.CallbackContext ctx)
	{
		if (gameManager.registeredControlDeviceIDs.Count > controlDeviceIndex
			&& ctx.control.device.id == gameManager.registeredControlDeviceIDs[controlDeviceIndex])
		{
			movementInput = ctx.ReadValue<Vector2>();
		}
	}

	private void UpdateLookRotationControlled(InputAction.CallbackContext ctx)
	{
		aimInput = ctx.ReadValue<Vector2>();
		//if (gameManager.registeredControlDeviceIDs.Count > controlDeviceIndex
		//	&& ctx.control.device.id == gameManager.registeredControlDeviceIDs[controlDeviceIndex])
		//{
		//}
	}

	private void TriggerShotControlled(InputAction.CallbackContext ctx)
	{
		//Debug.Log(ctx.control.device.id);
		Debug.Log("Shot");
	}

	private void Update()
	{
		UpdateMovement();
		UpdateLookRotation();

		UpdateProjectileOrbit();
		UpdateProjectileRotation();
	}

	private void UpdateMovement()
	{
		Camera mainCam = gameManager.MainCam; // dont save reference to main cam to avoid static MonoBehavior ref

		var camHorizontal = new Vector3(mainCam.transform.right.x, 0f, mainCam.transform.right.z).normalized;
		var camVertical = new Vector3(mainCam.transform.forward.x, 0f, mainCam.transform.forward.z).normalized;

		float mod = Time.deltaTime * movespeedMod;
		var movement = Vector3.ClampMagnitude
			(camHorizontal * movementInput.x 
			+ camVertical * movementInput.y, 1f) * mod; // move player relative to camera
		if (!charController.isGrounded) movement.y = Physics.gravity.y * Time.deltaTime; // apply gravity if not grounded
		
		charController.Move(movement);
	}

	private void UpdateLookRotation()
	{
		Vector3 lookDir = default;
		switch (controlType)
		{
			default:
			case ControlType.Mouse:
				if (aimInput.sqrMagnitude < 1.1f) return;
				var objectPos = gameManager.MainCam.WorldToScreenPoint(transform.position, Camera.MonoOrStereoscopicEye.Mono);
				objectPos.z = 0f;
				lookDir = new Vector3(aimInput.x, aimInput.y, 0f) - objectPos;
				break;

			case ControlType.Controller:
				if (aimInput.sqrMagnitude > .1f) lookDir = aimInput;
				else return;
				break;
		}

		transform.rotation = Quaternion.LookRotation(new Vector3(lookDir.x, 0f, lookDir.y), transform.up);
	}
	#endregion

	private void UpdateProjectileOrbit()
	{
		var rotation = Quaternion.Euler(0f, projectileOrbitSpeed * Time.deltaTime, 0f);

		var resultingOffset = rotation * ((loadedProjectile.transform.position - projectileOrbitCenter.position).normalized * projectileOrbitDist);
		resultingOffset.y = (Mathf.PerlinNoise(Time.time, 0f) - .5f) * projectileOrbitHeightMod;
		loadedProjectile.transform.position = projectileOrbitCenter.position + resultingOffset;
	}

	private void UpdateProjectileRotation()
	{
		loadedProjectile.transform.rotation *= 
			Quaternion.Euler
			(Random.Range(projectileRotationMin, projectileRotationMax) * Time.deltaTime, 
			Random.Range(projectileRotationMin, projectileRotationMax) * Time.deltaTime, 
			Random.Range(projectileRotationMin, projectileRotationMax) * Time.deltaTime);
	}
}
