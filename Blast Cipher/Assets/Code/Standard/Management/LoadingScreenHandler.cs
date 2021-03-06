﻿using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenHandler : MonoBehaviour
{
	public class LoadingScreenProgressToken
	{
		public bool ScreenFullyShown;
		public bool TransitionComplete;
		public bool LoadingScreenGone;
	}

	private static LoadingScreenHandler Instance;

	[SerializeField, TextArea] private string[] loadingMessages;
	[SerializeField] private Image[] miscImages;
	[SerializeField] private Image loadingImage;
	[SerializeField] private TextMeshProUGUI text;
	[SerializeField] private float transitionInDuration;
	[SerializeField] private float transitionMainDuration;
	[SerializeField] private float transitionOutDuration;

	private float startHeightY;
	private float startPosX;
	[SerializeField] private float endPosX;

	private void Awake()
	{
		Instance = this;
		startPosX = text.rectTransform.anchoredPosition.x;
		startHeightY = miscImages[0].rectTransform.sizeDelta.y;
		SetOpacities(0f);
	}

	public static void ShowLoadingScreen(LoadingScreenProgressToken token)
	{
		Instance.StartCoroutine(Instance.HandleLoadingScreen(token));
		Instance.StartCoroutine(Instance.TextMover());
	}

	private IEnumerator TextMover()
	{
		Vector2 anchPos = text.rectTransform.anchoredPosition;

		text.SetText(loadingMessages[Random.Range(0, loadingMessages.Length)]);

		float target = transitionInDuration + transitionOutDuration + transitionMainDuration;
		float lerpVal, intensity, timer = 0f;
		while(timer < target)
		{
			lerpVal = timer / target;
			intensity = Mathf.Lerp(startPosX, endPosX, lerpVal);

			anchPos = text.rectTransform.anchoredPosition;
			anchPos.x = intensity;
			text.rectTransform.anchoredPosition = anchPos;

			timer += Time.unscaledDeltaTime;
			yield return null;
		}
	}

	private IEnumerator HandleLoadingScreen(LoadingScreenProgressToken token)
	{
		MusicManager.Instance.LoadingScreenTransitionEffect(transitionInDuration, true);
		float timer = 0f;
		while(timer < transitionInDuration)
		{
			SetOpacities(timer / transitionInDuration);

			timer += Time.unscaledDeltaTime;
			yield return null;
		}

		SetOpacities(1f);

		yield return new WaitForSecondsRealtime(transitionMainDuration * .5f);
		token.ScreenFullyShown = true;
		yield return new WaitForSecondsRealtime(transitionMainDuration * .5f);
		token.TransitionComplete = true;

		MusicManager.Instance.LoadingScreenTransitionEffect(transitionOutDuration, false);
		timer = 0f;
		while (timer < transitionOutDuration)
		{
			SetOpacities(1f - timer / transitionInDuration);

			timer += Time.unscaledDeltaTime;
			yield return null;
		}

		SetOpacities(0f);
		token.LoadingScreenGone = true;
	}

	private void SetOpacities(float opacity)
	{
		Color color;
		Vector2 sizeDelta;
		for (int i = 0; i < miscImages.Length; i++)
		{
			color = miscImages[i].color;
			color.a = opacity;
			miscImages[i].color = color;

			// slide in borders based on opacity
			sizeDelta = miscImages[i].rectTransform.sizeDelta;
			sizeDelta.y = startHeightY * opacity;
			miscImages[i].rectTransform.sizeDelta = sizeDelta;
		}

		color = loadingImage.color;
		color.a = opacity;
		loadingImage.color = color;

		color = text.color;
		color.a = opacity;
		text.color = color;

	}
}
