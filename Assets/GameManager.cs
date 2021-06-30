using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static Dictionary<bool, GameObject> PlayerN;

    public GameObject PlayerPrefab;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 pos = new Vector3(Random.Range(-5f, 5f), Random.Range(1f, 5f));
        GameObject entry = PhotonNetwork.Instantiate(PlayerPrefab.name, pos, Quaternion.identity);

        if (PlayerN == null) PlayerN = new Dictionary<bool, GameObject>();
        PlayerN.Add(true, entry);
        Debug.Log(PlayerN.Count);
        foreach (KeyValuePair<bool, GameObject> keyValue in PlayerN)
        {
            // keyValue.Value представляет класс Person
            Debug.Log(keyValue.Key + " - " + keyValue.Value.name);
        }

        foreach (bool c in PlayerN.Keys)
        {
            Debug.Log(c);
        }

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
