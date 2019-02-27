using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ViveInput : MonoBehaviour
{
    [SerializeField]
    public SteamVR_Action_Single squeezeAction;
    public SteamVR_Action_Vector2 touchPadAction;
    private InteractionManager interactionManager = null;

    void Update()
    {
       

        if (SteamVR_Actions.default_Teleport.GetStateDown(SteamVR_Input_Sources.Any)){
            print("Teleport down");
        }

        if (SteamVR_Actions.default_GrabPinch.GetStateUp(SteamVR_Input_Sources.Any)){
            print("grab pinch up");
        }

        float triggerValue = squeezeAction.GetAxis(SteamVR_Input_Sources.Any);

        if (triggerValue > 0.0f) {
            print(triggerValue);
        }

        Vector2 touchPadValue = touchPadAction.GetAxis(SteamVR_Input_Sources.Any);

        if (touchPadValue != Vector2.zero) {
            print(touchPadValue);
        }

    }
}
