/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meta.XR.Samples;
using UnityEngine;



public class EnvironmentPanelPlacement : MonoBehaviour
{
    //private EnvironmentRaycastManager _raycastManager;
    private Transform _centerEyeAnchor;
    private Transform _raycastAnchor;
    [SerializeField] private OVRInput.RawButton _grabButton = OVRInput.RawButton.RIndexTrigger | OVRInput.RawButton.RHandTrigger;
    [SerializeField] private OVRInput.RawAxis2D _scaleAxis = OVRInput.RawAxis2D.RThumbstick;
    [SerializeField] private OVRInput.RawAxis2D _moveAxis = OVRInput.RawAxis2D.RThumbstick;
    [SerializeField] private Transform _panel;
    [SerializeField] private float _panelAspectRatio = 0.823f;
    [SerializeField] private GameObject _panelGlow;
    [SerializeField] private LineRenderer _raycastVisualizationLine;
    [SerializeField] private Transform _raycastVisualizationNormal;

    private readonly RollingAverage _rollingAverageFilter = new RollingAverage();
    private Vector3 _targetPose;
    private Quaternion _targetRotation;
    private Vector3 _positionVelocity;
    private float _rotationVelocity;
    public bool _isGrabbing;
    private float _distanceFromController;
    private Pose? _environmentPose;
    private LayerMask layerMask;
    private LayerMask ignoreLayer;
    private LayerMask layerMask2;
    private LayerMask ignoreLayer2;

    private void Start()
    {
            
        ignoreLayer = 1 << LayerMask.NameToLayer("Decoration");
        layerMask = ~ignoreLayer;
        ignoreLayer2 = 1 << LayerMask.NameToLayer("GlobalMesh");
        layerMask2 = ~ignoreLayer2;
        // This includes all layers *except* IgnoreMe
        //_raycastManager = FindFirstObjectByType<EnvironmentRaycastManager>();
        _centerEyeAnchor = OVRManager.instance.GetComponent<OVRCameraRig>().centerEyeAnchor;
        _raycastAnchor = OVRManager.instance.GetComponent<OVRCameraRig>().rightControllerAnchor;
            

        // Place the panel in front of the user
        var position = _centerEyeAnchor.position + _centerEyeAnchor.forward;
        //var forward = Vector3.ProjectOnPlane(_centerEyeAnchor.position - position, Vector3.up).normalized;
        //_panel.position = position;
        _targetPose = _panel.position;
        _targetRotation = _panel.rotation;
        //_panel.rotation = Quaternion.LookRotation(forward);

        // Create the OVRSpatialAnchor and make it a parent of the panel.
        // This will prevent the panel front drifting after headset lock/unlock.
            
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            _isGrabbing = false;
        }
    }

    private void Update()
    {
           

        VisualizeRaycast();
        if (_isGrabbing)
        {
            UpdateTargetPose();
            if (OVRInput.GetUp(_grabButton))
            {
                _panelGlow.SetActive(false);
                _isGrabbing = false;
                _environmentPose = null;

                    
            }
        }
        else
        {
            // Animate scale with right thumbstick
            const float scaleSpeed = 1.5f;
            var panelScale = _panel.localScale.x;
            panelScale *= 1f + OVRInput.Get(_scaleAxis).y * scaleSpeed * Time.deltaTime;
            panelScale = Mathf.Clamp(panelScale, 0.2f, 1.5f);
            _panel.localScale = new Vector3(panelScale, panelScale * _panelAspectRatio, 1f);

            // Detect grab gesture and update grab indicator
            bool didHitPanel = Physics.Raycast(_raycastAnchor.position, _raycastAnchor.forward, out var hit, ignoreLayer2) && hit.transform == _panel;
            _panelGlow.SetActive(didHitPanel);
            if (didHitPanel && OVRInput.GetDown(_grabButton))
            {
                _isGrabbing = true;
                _distanceFromController = Vector3.Distance(_raycastAnchor.position, _panel.position);
            }
        }
        AnimatePanelPose();
    }

    private Ray GetRaycastRay()
    {
        return new Ray(_raycastAnchor.position + _raycastAnchor.forward * 0.1f, _raycastAnchor.forward);
    }

    private void UpdateTargetPose()
    {
        // Animate manual placement position with right thumbstick
        const float moveSpeed = 2.5f;
        _distanceFromController += OVRInput.Get(_moveAxis).y * moveSpeed * Time.deltaTime;
        _distanceFromController = Mathf.Clamp(_distanceFromController, 0.3f, 3f);
        // Try place the panel onto environment
        if (Physics.Raycast(_raycastAnchor.position + _raycastAnchor.forward * 0.1f, _raycastAnchor.forward, out var result, 10, layerMask))
        {
            var smoothedNormal = _rollingAverageFilter.UpdateRollingAverage(result.normal);
            var manualPlacementPosition = _raycastAnchor.position + _raycastAnchor.forward * _distanceFromController;

            var manualPlacementPose = manualPlacementPosition;
            // If environment pose is available and the panel is closer to it than to the user, place the panel onto environment to create a magnetism effect
            bool chooseEnvPose = Vector3.Distance(manualPlacementPose, result.point) / Vector3.Distance(manualPlacementPose, _centerEyeAnchor.position) < 0.5 && OVRInput.Get(_moveAxis).y > -0.7;

            if (Mathf.Abs(Vector3.Dot(result.normal, Vector3.up)) < 0.3f && chooseEnvPose == true)
            {

                _targetPose = result.point;
                _targetRotation = Quaternion.LookRotation(smoothedNormal, Vector3.up);
                _distanceFromController = Vector3.Distance(_raycastAnchor.position, _panel.position);


            }
            else
            {
                var manualPlacementPosition2 = _raycastAnchor.position + _raycastAnchor.forward * _distanceFromController;
                    
                var forward = Vector3.ProjectOnPlane(_centerEyeAnchor.position - manualPlacementPosition2, Vector3.up).normalized;
                    
                    

                _targetPose = manualPlacementPosition2;
                _targetRotation = Quaternion.LookRotation(forward);
            }
                
        }
            
            
    }

       

    private void AnimatePanelPose()
    {
            

        const float smoothTime = 0.13f;
        _panel.position = Vector3.SmoothDamp(_panel.position, _targetPose, ref _positionVelocity, smoothTime);

        float angle = Quaternion.Angle(_panel.rotation, _targetRotation);
        if (angle > 0f)
        {
            float dampedAngle = Mathf.SmoothDampAngle(angle, 0f, ref _rotationVelocity, smoothTime);
            float t = 1f - dampedAngle / angle;
            _panel.rotation = Quaternion.SlerpUnclamped(_panel.rotation, _targetRotation, t);
        }
    }

    private void VisualizeRaycast()
    {
        var ray = GetRaycastRay();
        bool hasHit = Physics.Raycast(ray, out var hit);

        bool hasNormal = true;
        _raycastVisualizationLine.enabled = hasHit;
        _raycastVisualizationNormal.gameObject.SetActive(hasHit && hasNormal);
        if (hasHit)
        {
            _raycastVisualizationLine.SetPosition(0, ray.origin);
            _raycastVisualizationLine.SetPosition(1, hit.point);

            if (hasNormal)
            {
                _raycastVisualizationNormal.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }

        
    private class RollingAverage
    {
        private List<Vector3> _normals;
        private int _currentRollingAverageIndex;

        public Vector3 UpdateRollingAverage(Vector3 current)
        {
            if (_normals == null)
            {
                const int filterSize = 10;
                _normals = Enumerable.Repeat(current, filterSize).ToList();
            }
            _currentRollingAverageIndex++;
            _normals[_currentRollingAverageIndex % _normals.Count] = current;
            Vector3 result = default;
            foreach (var normal in _normals)
            {
                result += normal;
            }
            return result.normalized;
        }
    }
}

