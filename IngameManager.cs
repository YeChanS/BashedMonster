using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameManager : MonoBehaviour
{
    [SerializeField] Transform[] _playerSpawnPos;
    [SerializeField] GameObject _playerObject;
    [SerializeField] GameObject _preResultWindow;

    ResultWindow _resutWindow;
    GameObject _player;
    int _playerSpawnIdx;

    int _maxStageIdx;

    static IngameManager _uniqueInstance;
    public static IngameManager _instance
    {
        get { return _uniqueInstance; }
    }
    void Awake()
    {
        _uniqueInstance = this;
        _maxStageIdx = _playerSpawnPos.Length;
        _playerSpawnIdx = 0;
        _player = GameObject.FindGameObjectWithTag("Player");
        PlayerSpawn();
    }
    void Start()
    {
        CameraControl._instance.ChangeSize(_playerSpawnIdx);
        MiniMapCamera._instance.ChangeSize(_playerSpawnIdx);
        _playerSpawnIdx++;
    }

    void Update()
    {
        
    }


    void PlayerSpawn()
    {
        if (_player == null)
        {
            GameObject go = Instantiate(_playerObject, _playerSpawnPos[_playerSpawnIdx].position, Quaternion.identity);
            _player = GameObject.FindGameObjectWithTag("Player");
        }
        else
            _player.gameObject.SetActive(true);
    }

    public void NextStage()
    {
        if (_playerSpawnIdx == _maxStageIdx)
        {
            _playerSpawnIdx = 0;
            if (_resutWindow == null)
            {
                string text = "Clear";
                GameObject go = Instantiate(_preResultWindow, _preResultWindow.transform.position, Quaternion.identity);
                go.GetComponent<ResultWindow>().SetResultText(text, Color.red);
                _resutWindow = go.GetComponent<ResultWindow>();
            }
            else
                _resutWindow.gameObject.SetActive(true);

            return;
        }
        CameraControl._instance.ChangeSize(_playerSpawnIdx);
        MiniMapCamera._instance.ChangeSize(_playerSpawnIdx);
        _player.transform.position = _playerSpawnPos[_playerSpawnIdx].position;
        _playerSpawnIdx++;

    }
}
