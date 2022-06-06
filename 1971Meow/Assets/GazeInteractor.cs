/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeInteractor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
*/

#region Includes
using UnityEngine;
#endregion

namespace TS.GazeInteraction
{
    public class GazeInteractor : MonoBehaviour
    {
        #region Variables

        [Header("Configuration")]
        [SerializeField] private float _maxDetectionDistance;
        [SerializeField] private float _minDetectionDistance;
        [SerializeField] private float _timeToActivate = 1.0f;
        [SerializeField] private LayerMask _layerMask;

        private Ray _ray;
        private RaycastHit _hit;

        private GazeReticle _reticle;
        private GazeInteractor _interactable;   //

        private float _enterStartTime;

        #endregion

        private void Start()
        {
            var instance = ResourcesManager.GetPrefab(ResourcesManager.FILE_PREFAB_RETICLE);
            var reticle = instance.GetComponent<GazeReticle>();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (reticle == null) { throw new System.Exception("Missing GazeReticle"); }
#endif

            _reticle = Instantiate(reticle);
            _reticle.SetInteractor(this);
        }
        private void Update()
        {
            _ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(_ray, out _hit, _maxDetectionDistance, _layerMask))
            {
                var distance = Vector3.Distance(transform.position, _hit.transform.position);
                if (distance < _minDetectionDistance)
                {
                    _reticle.Enable(false);
                    Reset();
                    return;
                }

                _reticle.SetTarget(_hit);
                _reticle.Enable(true);

                var interactable = _hit.collider.transform.GetComponent<GazeInteractor>(); //
                if (interactable == null)
                {
                    Reset();
                    return;
                }

                if (interactable != _interactable)
                {
                    Reset();

                    _enterStartTime = Time.time;

                    _interactable = interactable;
                    _interactable.GazeEnter(this, _hit.point);
                }

                _interactable.GazeStay(this, _hit.point);

                if (_interactable.IsActivable && !_interactable.IsActivated)
                {
                    var timeToActivate = (_enterStartTime + _timeToActivate) - Time.time;
                    var progress = 1 - (timeToActivate / _timeToActivate);
                    progress = Mathf.Clamp(progress, 0, 1);

                    _reticle.SetProgress(progress);

                    if (progress == 1)
                    {
                        _reticle.Enable(false);
                        _interactable.Activate();
                    }
                }

                return;
            }

            _reticle.Enable(false);
            Reset();
        }

        private void Reset()
        {
            _reticle.SetProgress(0);

            if (_interactable == null) { return; }
            _interactable.GazeExit(this);
            _interactable = null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward * _maxDetectionDistance);
        }
#endif
    }
}