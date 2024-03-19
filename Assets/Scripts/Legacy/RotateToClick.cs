using UnityEngine;

namespace Legacy
{
    public class RotateToClick : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0f;
                transform.LookAt(mousePos);
            }
        }
    }
}