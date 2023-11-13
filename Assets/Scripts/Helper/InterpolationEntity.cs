using System;
using Fusion;
using UnityEngine;

namespace Helper
{
    public class InterpolationEntity : MonoBehaviour
    {
        [SerializeField] private float _interpolationWindow = 0.04f;
        public float BufferMemory = 1;

        private readonly Memory<Timed<Vector2>> _positionBuffer = new();

        public int BufferCount => _positionBuffer.Count;

        public void BufferPosition(float time, Vector2 position)
        {
            _positionBuffer.Add(new Timed<Vector2>(position, time), DateTime.Now.AddSeconds(BufferMemory));
        }

        private Vector3 _position;

        private void Update()
        {
            _position = transform.position;
            var renderTime = Time.time - _interpolationWindow;
            if (TryFindSnapshots(renderTime, out var prev, out var next))
            {
                var interpolationPercentage = Mathf.InverseLerp(prev.Time, next.Time, renderTime);
                transform.position = Vector2.Lerp(prev.Value, next.Value, interpolationPercentage);
            }
            else
            {
                Debug.LogWarning($"Cannot find snapshots before and after {renderTime}, " +
                                 $"probably the value of interpolation window is higher than the buffer memory.");
            }
        }

        private void OnPostRender()
        {
            transform.position = _position;
        }

        private bool TryFindSnapshots(float time, out Timed<Vector2> prev, out Timed<Vector2> next)
        {
            _positionBuffer.RemoveExpiredItems();

            for (int i = 0; i < _positionBuffer.Count - 1; i++)
            {
                if (_positionBuffer[i].Time < time && time <= _positionBuffer[i + 1].Time)
                {
                    prev = _positionBuffer[i];
                    next = _positionBuffer[i + 1];
                    return true;
                }
            }

            prev = next = default;
            return false;
        }
    }
}