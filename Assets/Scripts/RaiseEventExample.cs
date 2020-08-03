using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class RaiseEventExample : MonoBehaviourPun
{
    [SerializeField]
    private GameObject colorChangeCube;

    private const byte COLOR_CHANGE_EVENT = 0;

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    private void NetworkingClient_EventReceived(EventData obj)
    {
        if (obj.Code == COLOR_CHANGE_EVENT)
        {
            object[] datas = (object[]) obj.CustomData;
            float r = (float) datas[0];
            float g = (float) datas[1];
            float b = (float) datas[2];
            setCubeColor(r, g, b);
        }
    }

    private void changeColor()
    {
        float r = Random.Range(0f,1f);
        float g = Random.Range(0f,1f);
        float b = Random.Range(0f,1f);

        // colorChangeCube.color = new Color(r, g, b, 1f);
        setCubeColor(r, g, b);

        // cant specify incoming objects
        object[] datas = new object[] { r, g, b }; // base.photonView.ViewID,
        PhotonNetwork.RaiseEvent(COLOR_CHANGE_EVENT, datas, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        // GO a, b Send RPC on A over network, other clients also get RPC on A
        // send event over network on A, and Object B subscribes to events, both receive the event
        // create RPC behavior unreliably, send photon view id base.photonView.ViewID,
    }

    private void setCubeColor(float r, float g, float b)
    {
        //Get the Renderer component from the new cube
        var cubeRenderer = colorChangeCube.GetComponent<Renderer>();

        //Call SetColor using the shader property name "_Color" and setting the color to red
        cubeRenderer.material.SetColor("_Color", new Color(r, g, b, 1f));
    }

    void Start()
    {
        // Debug.Log("room name: " + Photon.Realtime.Room.name);
        // Debug.Log("room name: " + Photon.Realtime.Room);
    }

    void Update()
    {
        if (base.photonView.IsMine && Input.GetKey(KeyCode.Space)) // or GetKeyDown
        {
            changeColor();
        }
        if (!base.photonView.IsMine && Input.GetKey(KeyCode.Space)) // or GetKeyDown
        {
            Debug.Log("not mine");
        }
        
    }
}
