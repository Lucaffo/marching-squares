using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BornAsFlame.Managers.Cameras
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform followTarget;
        [SerializeField] private float followSpeed;
        [SerializeField] private float followOrtographicSize;
        [SerializeField] private BoxCollider2D confiner;

        private Coroutine followCoroutine;
        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            StartFollow();
        }

        private void OnDisable()
        {
            StopFollow();
        }

        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
        }

        public void SetCameraOrthographicSize(float cameraOrthographicSize)
        {
            followOrtographicSize = cameraOrthographicSize;
        }

        public void SetCameraSpeed(float speed)
        {
            followSpeed = Mathf.Max(0, speed);
        }

        public void SetCameraConfiner(BoxCollider2D confiner)
        {
            this.confiner = confiner;
        }

        private void StartFollow()
        {
            if (followCoroutine != null)
            {
                StopFollow();
            }

            followCoroutine = StartCoroutine(FollowTarget());
        }

        private void StopFollow()
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }

        IEnumerator FollowTarget()
        {
            while (true)
            {
                // Virtual target
                Vector3 target = new Vector3(followTarget.position.x, followTarget.position.y, -followOrtographicSize);

                // Confine the camera position
                if (confiner)
                {
                    target.x = Mathf.Clamp(target.x, confiner.bounds.min.x + cam.Extents().x, confiner.bounds.max.x - cam.Extents().x);
                    target.y = Mathf.Clamp(target.y, confiner.bounds.min.y + cam.Extents().y, confiner.bounds.max.y - cam.Extents().y);
                }

                // Position lerping
                if (followTarget != null && Vector2.Distance(target, transform.position) > 0.01f)
                {
                    transform.position = (Vector3)Vector2.Lerp(transform.position, target, followSpeed * Time.deltaTime) - Vector3.forward * followOrtographicSize;
                }
                else
                {
                    transform.position = target;
                }

                // Size lerping
                if (Mathf.Abs(cam.orthographicSize - followOrtographicSize) > 0.01f)
                {
                    cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, followOrtographicSize, followSpeed * Time.deltaTime);
                }
                else
                {
                    cam.orthographicSize = followOrtographicSize;
                }

                yield return new WaitForFixedUpdate();
            }
        }

        #region Save/Load
        const string CAMERA_POSITION = "camera_position";
        const string CAMERA_TARGET = "camera_target";
        const string CAMERA_ORTHOGRAPHIC_SIZE = "camera_orthographic_size";

        public Dictionary<string, object> SetCheckpointData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();

            data.Add(CAMERA_POSITION, transform.position);
            data.Add(CAMERA_TARGET, followTarget);
            data.Add(CAMERA_ORTHOGRAPHIC_SIZE, followOrtographicSize);

            return data;
        }

        public void LoadCheckpointData(Dictionary<string, object> save)
        {
            transform.position = (Vector3)save[CAMERA_POSITION];
            followOrtographicSize = (float)save[CAMERA_ORTHOGRAPHIC_SIZE];
            followTarget = (Transform)save[CAMERA_TARGET];
        }

        public string GetID()
        {
            return gameObject.GetInstanceID().ToString();
        }
        #endregion
    }

    public static class CameraExtensions
    {
        public static Vector2 Extents(this Camera camera)
        {
            if (camera.orthographic)
                return new Vector2(camera.orthographicSize * Screen.width / Screen.height, camera.orthographicSize);
            else
            {
                Debug.LogError("Camera is not orthographic!", camera);
                return new Vector2();
            }
        }
    }

}