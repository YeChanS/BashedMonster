using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField]Transform _target;
    float _speed = 3;

    public Vector2 _center;
    public Vector2 _size;
    float _h;
    float _w;
    static CameraControl _uniqueInstance;
    public static CameraControl _instance
    {
        get { return _uniqueInstance; }
    }
    void Awake()
    {
        _uniqueInstance = this;
    }
    void Start()
    {
        //_target = Resources.Load<Transform>("Player");
        _h = Camera.main.orthographicSize;
        _w = _h * Screen.width / Screen.height;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_center, _size);
    }

    public void ChangeSize(int MapNum)
    {
        switch (MapNum)
        {
            case 0:
                Change(93f, 20f, -95f, 5f);
                break;
            case 1:
                Change(88f, 25f, 1.5f, 7.5f);
                break;
            case 2:
                Change(51f, 25f, 79f, 7.5f);
                break;
            case 3:
                Change(117f, 25f, 169f, 7.5f);
                break;
            case 4:
                Change(67f, 30f, 267f, 10f);
                break;
        }
    }

    void Change(float sizeX, float sizeY, float centerX, float centerY)
    {
        _size.x = sizeX;
        _size.y = sizeY;
        _center.x = centerX;
        _center.y = centerY;
    }


    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, _target.position, Time.deltaTime);
        transform.position = new Vector3(transform.position.x, transform.position.y, -10f);

        float lx = _size.x * 0.5f - _w;
        float clampX = Mathf.Clamp(transform.position.x, -lx + _center.x, lx + _center.x);
        float ly = _size.y * 0.5f - _h;
        float clampY = Mathf.Clamp(transform.position.y, -ly + _center.y, ly + _center.y);

        transform.position = new Vector3(clampX, clampY, -10f);
    }
}
