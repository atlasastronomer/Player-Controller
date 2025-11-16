using Core.Movement;
using UnityEngine;

namespace Entities.Nuri
{
    [RequireComponent(typeof(Controller2D))]
    public class NuriMovement : MonoBehaviour
    {
        [SerializeField] GameObject player;

        [Header("Orbit Variables")] [SerializeField]
        float orbitRadiusX = 1f;

        [SerializeField] float orbitRadiusY = 0.5f;
        [SerializeField] float orbitSpeed = 2f;

        [Header("Floating Variables")] [SerializeField]
        float floatAmplitude = 0.15f;

        [SerializeField] float floatFrequency = 2f;

        [Header("Catch-up Variables")] [SerializeField]
        float minCatchUpSpeed = 3f;

        [SerializeField] float maxCatchUpSpeed = 15f;
        [SerializeField] float catchUpDistanceThreshold = 2f;

        private float _time;
        private Vector3 _smoothedCenter;

        private Controller2D _controller;

        void Start()
        {
            _smoothedCenter = player.transform.position;
            _controller = GetComponent<Controller2D>();
        }

        void Update()
        {
            float distance = Vector3.Distance(_smoothedCenter, player.transform.position);

            float targetCatchUpSpeed = Mathf.Lerp(minCatchUpSpeed, maxCatchUpSpeed,
                Mathf.Clamp01(distance / catchUpDistanceThreshold));

            _smoothedCenter = Vector3.MoveTowards(_smoothedCenter, player.transform.position,
                targetCatchUpSpeed * Time.deltaTime);

            _time += orbitSpeed * Time.deltaTime;

            float x = Mathf.Sin(_time) * orbitRadiusX;
            float y = Mathf.Sin(2 * _time) * orbitRadiusY;

            float bob = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

            Vector3 orbitOffset = new Vector3(x, y + bob + 0.75f, 0f);

            Vector3 targetPos = _smoothedCenter + orbitOffset;

            Vector3 displacement = targetPos - transform.position;

            _controller.Move(displacement);
        }
    }
}