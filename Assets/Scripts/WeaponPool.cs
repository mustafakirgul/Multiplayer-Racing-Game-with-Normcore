using Normal.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPool : MonoBehaviour
{
    #region Singleton Logic
    public static WeaponPool instance = null;
    private void SingletonCheck()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    [SerializeField]
    public Pool[] pools;
    private GameObject _buffer;
    Realtime _realtime;
    bool isPopulated;
    [Serializable]
    public class Pool
    {
        public string name;
        public int id;
        public int startCount;
        public int topLimit = -1;
        public GameObject origin;
        public List<PoolMember> passiveCopies;
        public List<PoolMember> activeCopies;
    }
    private void Awake()
    {
        SingletonCheck();
        _realtime = GameObject.FindObjectOfType<Realtime>();
    }

    private void Update()
    {
        if (!isPopulated)
        {
            if (_realtime.connected)
            {
                isPopulated = true;
                PopulatePools();
            }
        }
    }

    private void PopulatePools()
    {
        for (int i = 0; i < pools.Length; i++)
        {
            if (pools[i].startCount > 0)
            {
                for (int x = 0; x < pools[i].startCount; x++)
                {
                    _buffer = Realtime.Instantiate(pools[i].name,
                    position: Vector3.zero,
                    rotation: Quaternion.identity,
               ownedByClient: true,
    preventOwnershipTakeover: false,
                 useInstance: _realtime);
                    _buffer.transform.parent = transform;
                    _buffer.GetComponent<PoolMember>().Freeze();
                }
            }
        }
    }

    public GameObject Pull(int _id)
    {
        if (pools[_id].passiveCopies.Count > 1)//Leave 1 behind as a backup
        {
            _buffer = pools[_id].passiveCopies[0].Thaw();
        }
        else if (pools[_id].topLimit > 0 && pools[_id].activeCopies.Count < pools[_id].topLimit)
        {
            _buffer = Realtime.Instantiate(pools[_id].name,
            position: Vector3.zero,          
            rotation: Quaternion.identity, 
       ownedByClient: true,                
         useInstance: _realtime);
            _buffer.transform.parent = transform;
            if (!pools[_id].passiveCopies.Contains(_buffer.GetComponent<PoolMember>()))
            {
                pools[_id].passiveCopies.Add(_buffer.GetComponent<PoolMember>());
            }
        }
        return _buffer;
    }
}
