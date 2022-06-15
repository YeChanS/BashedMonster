using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageControl : MonoBehaviour
{
    [SerializeField] GameObject[] _preCreatures;
    [SerializeField] Transform[] _spawnPos;
    [SerializeField] GameObject _nextStageDoorCollider;
    [SerializeField] GameObject _doorEff;
    int _preCreatureCount;
    int _maxCreatureCount;
    int _currCreatureCount;
    
    void Start()
    {
        _nextStageDoorCollider.GetComponent<CapsuleCollider2D>().enabled = false;
        _preCreatureCount = _preCreatures.Length;
        _maxCreatureCount = transform.Find("CreatureSpawnPos").childCount;
        _currCreatureCount = _maxCreatureCount;
        InitCreatures();
    }

    void Update()
    {
        if(_currCreatureCount <= 0)
        {
            _nextStageDoorCollider.GetComponent<CapsuleCollider2D>().enabled = true;
            _doorEff.SetActive(true);
        }
    }

    void InitCreatures()
    {
        for (int i = 0; i < _maxCreatureCount; i++)
        {
            int rd = Random.Range(0, _preCreatureCount);
            GameObject go = Instantiate(_preCreatures[rd], _spawnPos[i].transform.position, Quaternion.identity, _spawnPos[i]);
        }
    }

    public void DeadCreature()
    {
        _currCreatureCount--;
    }
}
