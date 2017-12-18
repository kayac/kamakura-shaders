using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SampleSceneController : MonoBehaviour
{

	private static readonly string PosePrevTrigger = "Pose_Prev";
	private static readonly string PoseNextTrigger = "Pose_Next";
	private static readonly string FacePrevTrigger = "Face_Prev";
	private static readonly string FaceNextTrigger = "Face_Next";

	public Animator animator;
	public Text faceLabel;
	public Text poseLabel;
	public Text sceneLabel;

	private int _currentSceneIndex;

	private SwipeController _swipeController;

	// Use this for initialization
	void Awake()
	{
		if (animator == null)
		{
			animator = GameObject.FindObjectOfType<Animator>();
		}

		if (animator == null)
		{
			return;
		}

		_swipeController = GetComponentInChildren<SwipeController>(true);
		if (_swipeController != null)
		{
			_swipeController.OnDragEvent -= OnDragEvent;
			_swipeController.OnDragEvent += OnDragEvent;
		}
	}

	void Start()
	{
		if (animator == null)
		{
			return;
		}

		_currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
		animator.Update(0.01f);
		UpdateClipLabel("Pose: ", poseLabel, "BaseLayer");
		UpdateClipLabel("Face: ", faceLabel, "FaceLayer");
		UpdateSceneLabel();
	}

	void OnDestroy()
	{
		if (_swipeController != null)
		{
			_swipeController.OnDragEvent -= OnDragEvent;
		}
	}

	void OnDragEvent(PointerEventData touchData, Rect rect)
	{
		animator.transform.Rotate(0, -180 * touchData.delta.x / rect.height, 0);
	}

	public void ChangeNextScene()
	{
		_currentSceneIndex = ++_currentSceneIndex >= SceneManager.sceneCountInBuildSettings ? 0 : _currentSceneIndex;
		SceneManager.LoadScene(_currentSceneIndex);
	}

	public void ChangePrevScene()
	{
		_currentSceneIndex = --_currentSceneIndex < 0 ? SceneManager.sceneCountInBuildSettings - 1 : _currentSceneIndex;
		SceneManager.LoadScene(_currentSceneIndex);
	}

	private void UpdateSceneLabel()
	{
		var activeScene = SceneManager.GetActiveScene();
		var sceneName = activeScene.name;
		if (sceneName.Contains("Dreamy") && activeScene.buildIndex >= 0)
		{
			sceneLabel.text = "Scene: " + SceneManager.GetActiveScene().name;
		}
	}

	public void SetNextPose()
	{
		if (animator == null)
		{
			return;
		}
		animator.SetTrigger(PoseNextTrigger);
		animator.Update(0.01f);
		UpdateClipLabel("Pose: ", poseLabel, "BaseLayer");
	}

	public void SetPrevPose()
	{
		if (animator == null)
		{
			return;
		}
		animator.SetTrigger(PosePrevTrigger);
		animator.Update(0.01f);
		UpdateClipLabel("Pose: ", poseLabel, "BaseLayer");
	}

	private void UpdateClipLabel(string label, Text text, string animationLayer)
	{
		if (animator == null)
		{
			return;
		}

		try
		{
			var layer = animator.GetLayerIndex(animationLayer);
			var clips = animator.GetNextAnimatorClipInfo(layer);
			if (clips.Length == 0)
			{
				clips = animator.GetCurrentAnimatorClipInfo(layer);
			}
			text.text = label + (clips.Length > 0 ?clips[0].clip.name.Replace("Dreamy_Facial_", "") : "");
		}
		catch (System.Exception e)
		{
			Debug.LogException(e);
			text.text = label;
		}
	}

	public void SetNextFace()
	{
		if (animator == null)
		{
			return;
		}
		animator.SetTrigger(FaceNextTrigger);
		animator.Update(0.01f);
		UpdateClipLabel("Face: ", faceLabel, "FaceLayer");
	}

	public void SetPrevFace()
	{
		if (animator == null)
		{
			return;
		}
		animator.SetTrigger(FacePrevTrigger);
		animator.Update(0.01f);
		UpdateClipLabel("Face: ", faceLabel, "FaceLayer");
	}
}
