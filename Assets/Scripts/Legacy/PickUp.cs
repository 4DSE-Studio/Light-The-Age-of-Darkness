using UnityEngine;

namespace Legacy
{
    public class PickUp : MonoBehaviour
    {
        [SerializeField] private AnimationCurve _moveCurve;
        [SerializeField] private float _pickupRadius = 2f;
        [SerializeField] private Transform _holdPosition;
        [SerializeField] private float _throwForce = 6f;

        private bool _isHoldingObject;
        private ExplosiveObject _pickedObject;
        private Camera _camera;

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (_isHoldingObject)
                    Throw();
                else
                    FindObject();
            }

            if (Input.GetMouseButtonDown(0) && _isHoldingObject)
            {
                Vector3 mouse = Input.mousePosition;
                Ray castPoint = _camera.ScreenPointToRay(mouse);

                if (Physics.Raycast(castPoint, out RaycastHit hit, Mathf.Infinity))
                    ThrowObjectToPosition(hit.point);
            }
        }

        private void FindObject()
        {
            const int MaxColliders = 30;
            Collider[] hitColliders = new Collider[MaxColliders];

            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, _pickupRadius, hitColliders);

            float minDistance = Mathf.Infinity;
            ExplosiveObject closestObject = null;

            for (int i = 0; i < numColliders; i++)
            {
                if (hitColliders[i].TryGetComponent(out ExplosiveObject explosiveObject) == false || explosiveObject.IsThrowing)
                    continue;

                if (explosiveObject.transform.parent is not null)
                    continue;

                float distance = Vector3.Distance(transform.position, explosiveObject.transform.position);

                if (distance >= minDistance)
                    continue;

                minDistance = distance;
                closestObject = explosiveObject;
            }

            if (closestObject == null)
                return;

            PickupObject(closestObject);
        }

        private void PickupObject(ExplosiveObject explosiveObject)
        {
            _isHoldingObject = true;
            _pickedObject = explosiveObject;

            _pickedObject.ObjectRigidbody.isKinematic = true;
            _pickedObject.ObjectRigidbody.useGravity = false;
            _pickedObject.ObjectRigidbody.drag = 0;
            _pickedObject.ObjectRigidbody.angularDrag = 0;

            Transform transform1 = _pickedObject.transform;
            StartCoroutine(Utils.MoveToTarget(transform1, _holdPosition, 0.2f));
            // StartCoroutine(Utils.MoveToPosition(transform1, _holdPosition.position, 1f, _moveCurve));
            transform1.SetParent(_holdPosition);

            transform1.localEulerAngles = Vector3.zero;

            _pickedObject.Exploded += PickedObjectOnExploded;
        }

        private void ThrowObjectToPosition(Vector3 targetPosition)
        {
            if (_pickedObject is null)
                return;

            Drop();

            StartCoroutine(_pickedObject.Throw(targetPosition));

            ForgetObject();
        }

        private void Throw()
        {
            Drop();

            _pickedObject.ObjectRigidbody.AddForce(Vector3.up + _pickedObject.transform.forward * _throwForce, ForceMode.VelocityChange);

            ForgetObject();
        }

        private void Drop()
        {
            _pickedObject.transform.SetParent(null);
            _pickedObject.ObjectRigidbody.isKinematic = false;
            _pickedObject.ObjectRigidbody.useGravity = true;
            _pickedObject.ObjectRigidbody.drag = 0.2f;
            _pickedObject.ObjectRigidbody.angularDrag = 0.5f;
        }

        private void ForgetObject()
        {
            _pickedObject.Exploded -= PickedObjectOnExploded;

            _pickedObject = null;
            _isHoldingObject = false;
        }

        private void PickedObjectOnExploded()
        {
            ForgetObject();
        }
    }
}