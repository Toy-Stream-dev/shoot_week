using _Game.Scripts.Balance;
using Cinemachine;
using DG.Tweening;
using GeneralTools;
using UnityEngine;

namespace _Game.Scripts
{
	public class GameCamera : BaseBehaviour
	{
		[SerializeField] private CinemachineVirtualCamera _cinemachineVirtualCamera;
		private Sequence _shakeSequence;
		
		public static GameCamera Instance { get; private set; }
		public static Camera UnityCam { get; private set; }
		public Tween _fovTween;

		private void Awake()
		{
			Instance = this;
			UnityCam = GetComponentInChildren<Camera>();
		}
		
		public void Follow(Transform target, bool lookAt = true)
		{
			_cinemachineVirtualCamera.Follow = target;
		
			if (lookAt)
			{
				LookAt(target);
			}
		}

		public void ChangeFOV(float attackRange)
		{
			var fov = (attackRange) * 10;
			if(fov == _cinemachineVirtualCamera.m_Lens.FieldOfView) return;
			_fovTween?.Kill();
			if (fov < GameBalance.Instance.MinFOV)
			{
				_fovTween = DOTween.To(x => _cinemachineVirtualCamera.m_Lens.FieldOfView = x,
					_cinemachineVirtualCamera.m_Lens.FieldOfView, GameBalance.Instance.MinFOV, GameBalance.Instance.ChangeFOVDuration);
			}
			else
			{
				_fovTween = DOTween.To(x => _cinemachineVirtualCamera.m_Lens.FieldOfView = x,
					_cinemachineVirtualCamera.m_Lens.FieldOfView, fov, GameBalance.Instance.ChangeFOVDuration);
			}
		}

		public void ShakeCamera(float intensity, float time)
		{
			_shakeSequence?.Kill();
			CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
				_cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
			cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
			_shakeSequence = DOTween.Sequence().AppendInterval(time).OnComplete(() =>
			{
				cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0;
			});
		}

		public void LookAt(Transform target)
		{
			_cinemachineVirtualCamera.LookAt = target;
		}
	}
}