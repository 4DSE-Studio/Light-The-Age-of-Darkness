using UnityEngine;

namespace Legacy
{
    public class Parallax : MonoBehaviour
    {
        public GameObject cam;
        public float parallaxEffectX, parallaxEffectY, speedScrollingX, speedScrollingY;
        public Vector2 vec;
        private float startposX, startposY;

        private void Awake()
        {
            startposX = transform.position.x;

            startposY = transform.position.y;
        }

        private void Update()
        {
            float temp = cam.transform.position.x * (1 - parallaxEffectX) - speedScrollingX * Time.time;
            float dist = cam.transform.position.x * parallaxEffectX;

            if (temp > startposX + vec.x * Mathf.Abs(transform.localScale.x))
                startposX += vec.x * Mathf.Abs(transform.localScale.x);
            else if (temp < startposX - vec.x * Mathf.Abs(transform.localScale.x))
                startposX -= vec.x * Mathf.Abs(transform.localScale.x);

            float tempY = cam.transform.position.y * (1 - parallaxEffectY) - speedScrollingY * Time.time;
            float distY = cam.transform.position.y * parallaxEffectY;

            transform.position = new Vector3(startposX + dist + speedScrollingX * Time.time, startposY + distY + speedScrollingY * Time.time, transform.position.z);

            if (tempY > startposY + vec.y * Mathf.Abs(transform.localScale.y))
                startposY += vec.y * Mathf.Abs(transform.localScale.y);
            else if (tempY < startposY - vec.y * Mathf.Abs(transform.localScale.y))
                startposY -= vec.y * Mathf.Abs(transform.localScale.y);
        }
    }
}