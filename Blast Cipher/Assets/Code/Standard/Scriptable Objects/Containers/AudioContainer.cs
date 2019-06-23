﻿using UnityEngine;

[CreateAssetMenu(fileName = "AudioContainer", menuName = "Audio/AudioContainer", order = 0)]
public class AudioContainer : ScriptableObject
{
	public AudioClip[] TransitionTrack;
	public AudioClip[] tracks;
	public float[] bpmValues;
}
