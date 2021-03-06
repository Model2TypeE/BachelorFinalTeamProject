﻿using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicManagerBootstrapper : Bootstrapper
{
	public delegate void OnBeatCallback();

	[SerializeField] internal Image debugImage;
	[SerializeField] internal Image debugImageTwo;
	[SerializeField] internal AudioMixer musicMixer;
	[SerializeField] internal AudioContainer cont;

	[Space]
	[SerializeField] internal AnimationCurve fadeInCurve;
	[SerializeField] internal AnimationCurve fadeOutCurve;

	private MusicManager manager;
	
	private void Awake()
	{
		manager = MusicManager.Instance;
		manager.RegisterBootstrapper(this);

		//manager.RegisterCallOnNextBeat(DEBUGCALL, 2, false);
		//manager.RegisterCallOnNextBeat(DEBUGCALL, 2, true);
		//manager.RegisterCallOnNextBeat(DEBUGCALL, 12);

		//manager.PlayMusic(cont);
	}
}