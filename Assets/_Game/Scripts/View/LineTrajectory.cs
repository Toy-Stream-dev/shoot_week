using GeneralTools;
using UnityEngine;

namespace _Game.Scripts.View
{
    public class LineTrajectory : BaseBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        
        public static LineTrajectory Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
            ClearTrajectory();
        }

        public void ShowTrajectory(Vector3 origin, Vector3 target, float jumpPower)
        {
            var newPosition = Vector3.Lerp(origin, target, 0.5f) - origin;
            var newTarget = new Vector3(newPosition.x, newPosition.y + (jumpPower * 2), newPosition.z); 
            CalculateTrajectory(origin, newTarget);
        }
        
        public void ClearTrajectory()
        {
            _lineRenderer.positionCount = 0;
        }

        private void CalculateTrajectory(Vector3 origin, Vector3 target)
        {
            Vector3[] points = new Vector3[100];
            _lineRenderer.positionCount = points.Length;

            for (int i = 0; i < points.Length; i++)
            {
                float time = i * 0.05f;
                points[i] = origin + target * time + Physics.gravity * time * time / 8f;
                if (points[i].y < 0)
                {
                    _lineRenderer.positionCount = i;
                    break;
                }
            }
        
            _lineRenderer.SetPositions(points);
        }
    }
}