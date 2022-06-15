using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    [SerializeField] Transform _target;
    //GameObject _target;
    float _speed = 3;

    public Vector2 _center;
    public Vector2 _size;
    float _h;
    float _w;
    static MiniMapCamera _uniqueInstance;
    public static MiniMapCamera _instance
    {
        get { return _uniqueInstance; }
    }
    void Awake()
    {
        _uniqueInstance = this;
    }
    void Start()
    {
        //_target = GameObject.FindGameObjectWithTag("Player");
        _h = GetComponent<Camera>().orthographicSize;
        //_h = Camera.main.orthographicSize;
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
                Change(135f, 45f, -95f, 15f);
                break;
            case 1:
                Change(130f, 45f, 1.5f, 15f);
                break;
            case 2:
                Change(93f, 45f, 79f, 15f);
                break;
            case 3:
                Change(159f, 45f, 169f, 15f);
                break;
            case 4:
                Change(109f, 45f, 267f, 15f);
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
        //transform.position = new Vector3(_target.position.x, _target.position.y, _target.position.z);
        transform.position = Vector3.Lerp(transform.position, _target.position, Time.deltaTime);
        transform.position = new Vector3(transform.position.x, transform.position.y, -10f);

        float lx = _size.x * 0.5f - _w;
        float clampX = Mathf.Clamp(transform.position.x, -lx + _center.x, lx + _center.x);
        float ly = _size.y * 0.5f - _h;
        float clampY = Mathf.Clamp(transform.position.y, -ly + _center.y, ly + _center.y);

        transform.position = new Vector3(clampX, clampY, -10f);
    }
}
