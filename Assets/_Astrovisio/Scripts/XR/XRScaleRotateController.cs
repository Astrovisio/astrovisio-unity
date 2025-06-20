using UnityEngine;

namespace Astrovisio
{
    public class XRScaleRotateController
    {
        private readonly float _rotationCutoff;
        private readonly bool _enableScaling;
        private readonly bool _inPlace;

        private Transform _target;

        private Vector3 _startSeparation;
        private Vector3 _startCenter;
        private Vector3 _startForwardAxis;
        private float _startScale;
        private float _yawAccum;
        private float _rollAccum;

        private enum RotationAxes { None = 0, Roll = 1, Yaw = 2 }
        private RotationAxes _activeAxes;

        public XRScaleRotateController(Transform target, float initialScale, float rotationCutoff = 5f, bool inPlace = true, bool enableScaling = true)
        {
            _target = target;
            _startScale = initialScale;
            _rotationCutoff = rotationCutoff;
            _inPlace = inPlace;
            _enableScaling = enableScaling;
            _activeAxes = RotationAxes.Yaw | RotationAxes.Roll;
        }

        public void Begin(Transform left, Transform right)
        {
            _startSeparation = left.position - right.position;
            _startCenter = (left.position + right.position) / 2f;
            _startForwardAxis = Vector3.Cross(Vector3.up, _startSeparation.normalized).normalized;
            _yawAccum = 0;
            _rollAccum = 0;
            _activeAxes = RotationAxes.Yaw | RotationAxes.Roll;
        }

        public void Update(Transform left, Transform right)
        {
            Vector3 currentSeparation = left.position - right.position;
            Vector3 currentCenter = (left.position + right.position) / 2f;

            float scaleFactor = currentSeparation.magnitude / Mathf.Max(_startSeparation.magnitude, 1e-6f);
            float newScale = Mathf.Max(1e-6f, _startScale * scaleFactor);
            float currentScale = _target.localScale.magnitude;

            // Yaw
            Vector3 oldDirXZ = new Vector3(_startSeparation.x, 0, _startSeparation.z).normalized;
            Vector3 newDirXZ = new Vector3(currentSeparation.x, 0, currentSeparation.z).normalized;
            float angleYaw = Mathf.Asin(Vector3.Cross(oldDirXZ, newDirXZ).y);

            // Roll
            Vector3 sideAxis = Vector3.Cross(_startForwardAxis, Vector3.up);
            Vector3 oldAxis = new Vector3(Vector3.Dot(sideAxis, _startSeparation), Vector3.Dot(Vector3.up, _startSeparation), 0).normalized;
            Vector3 newAxis = new Vector3(Vector3.Dot(sideAxis, currentSeparation), Vector3.Dot(Vector3.up, currentSeparation), 0).normalized;
            float angleRoll = Mathf.Asin(-Vector3.Cross(oldAxis, newAxis).z);

            if ((_activeAxes & RotationAxes.Yaw) == RotationAxes.Yaw)
            {
                _yawAccum += angleYaw * Mathf.Rad2Deg;
                if (Mathf.Abs(_yawAccum) >= _rotationCutoff)
                    _activeAxes = RotationAxes.Yaw;
            }

            if ((_activeAxes & RotationAxes.Roll) == RotationAxes.Roll)
            {
                _rollAccum += angleRoll * Mathf.Rad2Deg;
                if (Mathf.Abs(_rollAccum) >= _rotationCutoff)
                    _activeAxes = RotationAxes.Roll;
            }

            var applyYaw = (_activeAxes & RotationAxes.Yaw) == RotationAxes.Yaw;
            var applyRoll = (_activeAxes & RotationAxes.Roll) == RotationAxes.Roll;

            if (_inPlace)
            {
                if (_enableScaling)
                {
                    Vector3 offset = _target.position - _startCenter;
                    float ratio = newScale / currentScale;
                    _target.localScale = _target.localScale.normalized * newScale;
                    _target.position = _startCenter + offset * ratio;
                }

                Vector3 pivot = _target.InverseTransformPoint(_startCenter);

                if (applyYaw)
                    _target.RotateAround(_startCenter, Vector3.up, angleYaw * Mathf.Rad2Deg);
                if (applyRoll)
                    _target.RotateAround(_startCenter, _startForwardAxis, angleRoll * Mathf.Rad2Deg);

                _target.position -= _target.TransformPoint(pivot) - _startCenter;
            }
            else
            {
                if (_enableScaling)
                    _target.localScale = _target.localScale.normalized * newScale;
                if (applyYaw)
                    _target.Rotate(Vector3.up, angleYaw * Mathf.Rad2Deg);
                if (applyRoll)
                    _target.Rotate(_startForwardAxis, angleRoll * Mathf.Rad2Deg);
            }
        }
    }
}
