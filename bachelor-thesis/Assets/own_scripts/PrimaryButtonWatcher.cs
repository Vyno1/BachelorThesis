using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

[System.Serializable]
public class PrimaryButtonEvent : UnityEvent<bool, List<InputDevice>>
{
}

public class PrimaryButtonWatcher : MonoBehaviour
{
    public PrimaryButtonEvent primaryButtonPress;

    private bool lastButtonState = false;
    private List<InputDevice> handAndHeadsetDevices;

    private void Awake()
    {
        if (primaryButtonPress == null)
        {
            primaryButtonPress = new PrimaryButtonEvent();
        }

        handAndHeadsetDevices = new List<InputDevice>();
    }

    void OnEnable()
    {
        List<InputDevice> allDevices = new List<InputDevice>();
        InputDevices.GetDevices(allDevices);
        foreach (InputDevice device in allDevices)
            InputDevices_deviceConnected(device);

        InputDevices.deviceConnected += InputDevices_deviceConnected;
        InputDevices.deviceDisconnected += InputDevices_deviceDisconnected;
    }

    private void OnDisable()
    {
        InputDevices.deviceConnected -= InputDevices_deviceConnected;
        InputDevices.deviceDisconnected -= InputDevices_deviceDisconnected;
        handAndHeadsetDevices.Clear();
    }

    private void InputDevices_deviceConnected(InputDevice device)
    {
        bool discardedValue;
        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out discardedValue))
        {
            handAndHeadsetDevices.Add(device); // Add any devices that have a primary button.
        }
        else
        {
            List<InputFeatureUsage> featureUsages = new List<InputFeatureUsage>();
            device.TryGetFeatureUsages(featureUsages);
            foreach (var inputFeatureUsage in featureUsages)
            {
                if (inputFeatureUsage.name == "CenterEyePosition")
                {
                    handAndHeadsetDevices.Add(device);
                }
            }
        }
    }

    private void InputDevices_deviceDisconnected(InputDevice device)
    {
        if (handAndHeadsetDevices.Contains(device))
            handAndHeadsetDevices.Remove(device);
    }

    private List<InputDevice> sortDevices(List<InputDevice> unsorted)
    {
        var sortedList = new List<InputDevice>();
        var headIndex = 0;
        var leftIndex = 0;
        var rightIndex = 0;
        for (var i = 0; i < unsorted.Count; i++)
        {
            var characteristicString = unsorted[i].characteristics.ToString();
            if (characteristicString.Contains("Head"))
            {
                headIndex = i;
            }
            else if (characteristicString.Contains("Left"))
            {
                leftIndex = i;
            }
            else if (characteristicString.Contains("Right"))
            {
                rightIndex = i;
            }
        }
        sortedList.Add(unsorted[headIndex]);
        sortedList.Add(unsorted[leftIndex]);
        sortedList.Add(unsorted[rightIndex]);
        
        return sortedList;
    }

    void Update()
    {
        var tempState = false;
        var buttonDownList = new List<InputDevice>();
        foreach (var device in handAndHeadsetDevices)
        {
            bool primaryButtonState = false;
            tempState = device.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonState) // did get a value
                        && primaryButtonState // the value we got
                        || tempState; // cumulative result from other controllers
            if (primaryButtonState)
            {
                buttonDownList.Add(device);
            }
        }

        if (tempState == lastButtonState) return; // Button state changed since last frame
        sortDevices(handAndHeadsetDevices);
        primaryButtonPress.Invoke(tempState, handAndHeadsetDevices);
        lastButtonState = tempState;
    }
}