using UnityEngine;

namespace Legacy
{
    public class Look : MonoBehaviour
    {
        public Vector2 maxAnglesVertical, maxAnglesHorizontal;
        public Transform target;
        public Vector2 distances;

        private void Update()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            Vector2 difference = target.position - mousePos;

            float x = Mathf.InverseLerp(distances.x, distances.y, difference.x);
            float y = Mathf.InverseLerp(distances.x, distances.y, difference.y);

            target.rotation = Quaternion.Euler(new Vector3(Mathf.Lerp(maxAnglesVertical.x, maxAnglesVertical.y, y), Mathf.Lerp(maxAnglesHorizontal.x, maxAnglesHorizontal.y, x), 0));
        }
    }
}