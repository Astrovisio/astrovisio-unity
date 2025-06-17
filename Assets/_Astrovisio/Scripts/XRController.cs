using UnityEngine;


namespace Astrovisio
{
    public class XRController : MonoBehaviour
    {

        [SerializeField] private GameObject leftController;
        [SerializeField] private GameObject rightController;
        [SerializeField] private Transform rightPokePoint;

        private void Start()
        {

        }

        private void Update()
        {

        }

        public Transform GetPokePoint()
        {
            return rightPokePoint;
        }

    }

}
