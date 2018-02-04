using UnityEngine;

public class CameraController : MonoBehaviour
{
    public bool doMovement = true;

    public float panSpeed = 30f;
    public float panBorderThickness = 10f;

    public float scrollSpeed = 5f;
    public float minY = 10f;
    public float maxY = 80f;

    public Camera mainCamera;

    public float SensitivityX;
    public float SensitivityY;
    public float TargetAngleX; // Debug VALUE
    public float TargetAngleY; // Debug VALUE

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        Vector3 pos = transform.position;
        Vector3 v = transform.rotation.eulerAngles;
        Quaternion cameraRotation = Quaternion.Euler(0, v.y, v.z); // Keeps WASD directions correct when roation camera


        /* //perspective zoom
        pos.y -= scroll * 1000 * scrollSpeed * Time.deltaTime;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
        */

        if (Input.GetMouseButton(2))
        {
            TargetAngleX += Input.GetAxis("Mouse X") * SensitivityX * Time.deltaTime; //Yeah I know
            TargetAngleY += Input.GetAxis("Mouse Y") * SensitivityY * Time.deltaTime; //Yeah I know

            // Make limits on vertical rotation
            TargetAngleY = Mathf.Clamp(TargetAngleY, -35, 50);

            transform.eulerAngles = new Vector3(TargetAngleY, TargetAngleX, 0); //Yeah I know
        }

        //orthographic zoom
        if (scroll != 0)
        {
            if (scroll < 0) scroll = 1;
            else scroll = -1;
            scroll = Mathf.Clamp(mainCamera.orthographicSize + (scroll* scrollSpeed), 1, 10);
            //Debug.Log(scroll);
            //transform.localEulerAngles = new Vector3(scroll*10, transform.localEulerAngles.y, transform.localEulerAngles.z);
            mainCamera.orthographicSize = scroll;
        }

        //if (Input.GetKeyDown(KeyCode.Backspace)) doMovement = !doMovement; //for disabling/enabling movement
        
        if (!doMovement) return;

        if (Input.GetKey("w")/* || Input.mousePosition.y >= Screen.height - panBorderThickness*/)
        {
            transform.Translate(cameraRotation * Vector3.forward * panSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey("s")/* || Input.mousePosition.y <= panBorderThickness */)
        {
            transform.Translate(cameraRotation * Vector3.back * panSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey("d")/* || Input.mousePosition.x >= Screen.width - panBorderThickness */)
        {
            transform.Translate(cameraRotation * Vector3.right * panSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey("a")/* || Input.mousePosition.x <= panBorderThickness */)
        {
            transform.Translate(cameraRotation * Vector3.left * panSpeed * Time.deltaTime, Space.World);
        }

        //TEMPORARY
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (TurnHandler.Instance.currentState == TurnHandlerStates.PLAYERSELECT)
            {
                NodeManager.Instance.Deselect();
            }
        }
        if (Input.GetKeyUp(KeyCode.B)) UIHelper.Instance.DebugDraw();
    }
}