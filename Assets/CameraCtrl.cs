using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    public static CameraCtrl Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }
    private Vector3 OriginPos = new Vector3(0, 12, -20);
    public Vector3 ViewPos = new Vector3(0, 12, 0);
    private Quaternion OriginRotate = Quaternion.Euler(0, 0, 0);
    public Quaternion ViewRotate = Quaternion.Euler(90, 0, 0);
    private float OriginFOV = 60f;

    private float _pinchStartDistance;
    private Vector2 _touchStartPos;
    private Vector3 _touchStartPos_3;
    public float dragSpeed = 10f;

    private bool move = false;
    private float _processAnimate = -1f;
    public float SpeedAnimate = 2f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 touch0, touch1;
        Vector3 touch0_3;
        if (Input.touchCount >= 2)
        {
            float distance;
            touch0 = Input.GetTouch(0).position;
            touch1 = Input.GetTouch(1).position;
            distance = Vector2.Distance(touch0, touch1);
            if (_pinchStartDistance <= 0)
            {
                _pinchStartDistance = distance;
            }
            else
            {
                float change = distance - _pinchStartDistance;
                Camera.main.fieldOfView += Camera.main.fieldOfView * change / _pinchStartDistance;
                _pinchStartDistance = distance;
            }
        }
        else
        {
            _pinchStartDistance = 0;
            /*touch move camera

            if (Input.touchCount == 1)
            {
                touch0 = Input.GetTouch(0).position;
                if (!move)
                {
                    _touchStartPos = touch0;
                    move = true;
                }
                else
                {
                    Vector3 pos = Camera.main.ScreenToViewportPoint(touch0 - _touchStartPos);
                    Vector3 move = new Vector3(-pos.x * dragSpeed, 0, -pos.y * dragSpeed);

                    transform.Translate(move, Space.World);
                    _touchStartPos = touch0;
                }
            }
            if (move && Input.touchCount == 0)
            {
                move = false;
            }
            */

            // touch0_3 = Input.mousePosition;
            // if (Input.GetMouseButtonDown(0))
            // {
            //     Debug.Log("Touch 2000");
            //     if (!move)
            //     {
            //         Debug.Log("Touch 2");
            //         _touchStartPos_3 = touch0_3;
            //         move = true;
            //     }
            // }
            // if (move && !Input.GetMouseButton(0))
            // {
            //     Debug.Log("Touch 21");
            //     move = false;
            // }
            // if (move)
            // {
            //     Vector3 pos = Camera.main.ScreenToViewportPoint(touch0_3 - _touchStartPos_3);
            //     Vector3 move = new Vector3(pos.x * 1f, 0, pos.y * 1f);

            //     transform.Translate(move, Space.World);
            // }
        }

        //rotate
        if (_processAnimate > -1)
        {
            _processAnimate += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(OriginRotate, ViewRotate, _processAnimate / SpeedAnimate);
            transform.position = Vector3.Lerp(OriginPos, ViewPos, _processAnimate / SpeedAnimate);
            if (_processAnimate >= SpeedAnimate)
            {
                _processAnimate = -1;
            }
        }
    }

    public void AnimateStartGame()
    {
        _processAnimate = -1;
        OriginPos = gameObject.transform.position;
        OriginRotate = gameObject.transform.rotation;
    }
}
