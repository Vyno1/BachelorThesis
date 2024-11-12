using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;


public class RecordActions : MonoBehaviour
{
    public enum UseOfScript
    {
        RecordToFile,
        SendToNN,
        TestFromFile
    };

    public bool enablePositionPrint;

    public UseOfScript scriptUse;

    // Variables used for specifying recording path
    public string currentPerson;
    public string currentAction;

    // Path to the folder in which the recordings are supposed to be saved
    public string saveRecordPath;

    // Path from which the CSV is supposed to be read
    public string readRecordPath;
    public NetworkPredict networkPredict;
    public PrimaryButtonWatcher primaryButtonWatcher;

    #region Private Variables

    private bool _wasPressed = false;

    // Lists for saving all the recorded Vector3
    private List<Vector3> _positions1 = new List<Vector3>();
    private List<Vector3> _velocities1 = new List<Vector3>();
    private List<Vector3> _rotations1 = new List<Vector3>();
    private List<Vector3> _positions2 = new List<Vector3>();
    private List<Vector3> _velocities2 = new List<Vector3>();
    private List<Vector3> _rotations2 = new List<Vector3>();
    private List<Vector3> _positions3 = new List<Vector3>();
    private List<Vector3> _velocities3 = new List<Vector3>();
    private List<Vector3> _rotations3 = new List<Vector3>();

    // Path for saving the CSV
    private const string TempPath = @"LOCATION\PERSON\ACTION\TIME.csv";

    #endregion


    private void Start()
    {
        if (scriptUse == UseOfScript.TestFromFile)
        {
            primaryButtonWatcher.primaryButtonPress.AddListener(ReplayFromFile);
        }
        else
        {
            primaryButtonWatcher.primaryButtonPress.AddListener(RecActions);
        }
    }


    // Region with general methods

    #region General Functionality

    private string ConstructFilePath(string time)
    {
        var sb = new StringBuilder(TempPath);
        sb.Replace("LOCATION", saveRecordPath);
        sb.Replace("PERSON", currentPerson);
        sb.Replace("ACTION", currentAction);
        sb.Replace("TIME", time);
        return sb.ToString();
    }

    private void ClearLists()
    {
        _positions1.Clear();
        _positions2.Clear();
        _positions3.Clear();
        _velocities1.Clear();
        _velocities2.Clear();
        _velocities3.Clear();
        _rotations1.Clear();
        _rotations2.Clear();
        _rotations3.Clear();
    }

    #endregion

    // Region that contains all of the recording methods

    #region Record Actions

    private void RecActions(bool buttonPressed, List<InputDevice> devicePressed)
    {
        if (!buttonPressed) return;
        var currentTime = DateTime.Now.TimeOfDay.ToString(@"hh\:mm\:ss").Replace(":", "-");
        if (!_wasPressed)
        {
            _wasPressed = true;
            Debug.Log("Started Tracking");

            for (var i = 0; i < devicePressed.Count; i++)
            {
                StartCoroutine(RecFunc(devicePressed[i], i));
            }
        }
        else
        {
            _wasPressed = false;
            Debug.Log("Turned off tracking");
            StopAllCoroutines();
            switch (scriptUse)
            {
                case UseOfScript.RecordToFile:
                    SaveAsCSV(_positions1, _velocities1, _rotations1, _positions2, _velocities2, _rotations2,
                        _positions3,
                        _velocities3, _rotations3, currentTime);
                    break;
                case UseOfScript.SendToNN:

                    SaveAsCSV(_positions1, _velocities1, _rotations1, _positions2, _velocities2, _rotations2,
                        _positions3,
                        _velocities3, _rotations3, currentTime);

                    ClearLists();
                    print("================================================================================");

                    ReplayFromFile(buttonPressed, devicePressed);

                    break;
                case UseOfScript.TestFromFile:
                    print("Something went wrong, the enum is set to TestFromFile, but you still landed in record.");
                    break;
                default:
                    print("Something went wrong, the enum has taken on a completely wrong value.");
                    break;
            }

            ClearLists();
        }
    }


    private IEnumerator RecFunc(InputDevice inputDevice, int deviceNum)
    {
        while (_wasPressed)
        {
            inputDevice.TryGetFeatureValue(CommonUsages.devicePosition, out var pos);
            inputDevice.TryGetFeatureValue(CommonUsages.deviceVelocity, out var vel);
            inputDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out var rot);
            if (enablePositionPrint)
            {
                print($"pos{deviceNum}: {pos}");
                print($"velocity{deviceNum}: {vel}");
                print($"rotation{deviceNum}: {rot.eulerAngles}");
                print("================================================================================");
            }

            var posList = new List<Vector3>();
            var velList = new List<Vector3>();
            var rotList = new List<Vector3>();
            switch (deviceNum)
            {
                case 0:
                    posList = _positions1;
                    velList = _velocities1;
                    rotList = _rotations1;
                    break;
                case 1:
                    posList = _positions2;
                    velList = _velocities2;
                    rotList = _rotations2;
                    break;
                case 2:
                    posList = _positions3;
                    velList = _velocities3;
                    rotList = _rotations3;
                    break;
                default:
                    Debug.Log("Weird new device reported a button press");
                    break;
            }

            posList.Add(pos);
            velList.Add(vel);
            rotList.Add(rot.eulerAngles);
            yield return new WaitForSeconds(0.1f);
        }
    }

    #endregion

    // Region containing all methods that are used to save to a file

    #region Save To File

    private void SaveAsCSV(IReadOnlyList<Vector3> pos1, IReadOnlyList<Vector3> vel1, IReadOnlyList<Vector3> rot1,
        IReadOnlyList<Vector3> pos2, IReadOnlyList<Vector3> vel2, IReadOnlyList<Vector3> rot2,
        IReadOnlyList<Vector3> pos3, IReadOnlyList<Vector3> vel3, IReadOnlyList<Vector3> rot3, string time)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(
            "HeadPos.x, HeadPos.y, HeadPos.z, HeadVel.x, HeadVel.y, HeadVel.z, HeadRot.x, HeadRot.y, HeadRot.z," +
            "LeftPos.x, LeftPos.y, LeftPos.z, LeftVel.x, LeftVel.y, LeftVel.z, LeftRot.x, LeftRot.y, LeftRot.z," +
            "RightPos.x, RightPos.y, RightPos.z, RightVel.x, RightVel.y, RightVel.z, RightRot.x, RightRot.y, RightRot.z");
        var filepath = ConstructFilePath(time);
        readRecordPath = filepath;
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        for (var i = 0; i < pos1.Count; i++)
        {
            sb.AppendLine(
                $"{pos1[i].x} ,{pos1[i].y}, {pos1[i].z}, {vel1[i].x}, {vel1[i].y}, {vel1[i].z}, {rot1[i].x}, {rot1[i].x}, {rot1[i].x}," +
                $"{pos2[i].x} ,{pos2[i].y}, {pos2[i].z}, {vel2[i].x}, {vel2[i].y}, {vel2[i].z}, {rot2[i].x}, {rot2[i].x}, {rot2[i].x}," +
                $"{pos3[i].x} ,{pos3[i].y}, {pos3[i].z}, {vel3[i].x}, {vel3[i].y}, {vel3[i].z}, {rot3[i].x}, {rot3[i].x}, {rot3[i].x}");
        }

        File.Create(filepath).Dispose();
        File.WriteAllText(filepath, sb.ToString());
    }

    #endregion

    // Region for all methods used for sending movements to the Neural Network

    #region Communicating with NN

    private void SendToNN(List<Vector3> pos1, List<Vector3> vel1, List<Vector3> rot1,
        List<Vector3> pos2, List<Vector3> vel2, List<Vector3> rot2,
        List<Vector3> pos3, List<Vector3> vel3, List<Vector3> rot3)
    {
        Tensor input = new Tensor(1, 1, 27, 41);
        List<List<Vector3>> allLists = new List<List<Vector3>>
        {
            pos1,
            vel1,
            rot1,
            pos2,
            vel2,
            rot2,
            pos3,
            vel3,
            rot3
        };
        allLists = PadTo41(allLists);
        int currentListIndex = 0;
        foreach (var list in allLists)
        {
            for (var j = 0; j < list.Count; j++)
            {
                input[0, 0, currentListIndex, j] = list[j].x;
                input[0, 0, currentListIndex + 1, j] = list[j].y;
                input[0, 0, currentListIndex + 2, j] = list[j].z;
            }

            currentListIndex += 3;
        }

        networkPredict.Predict(input);
        input.Dispose();
    }


    // Pads the Lists to length 41 with forward padding, since the LSTM Layer expects length 41
    private List<List<Vector3>> PadTo41(List<List<Vector3>> allLists)
    {
        List<List<Vector3>> returnList = new List<List<Vector3>>();
        foreach (var list in allLists)
        {
            switch (list.Count)
            {
                case < 41:
                    List<Vector3> zeroList = new List<Vector3>();
                    for (int i = 0; i < 41 - list.Count; i++)
                    {
                        zeroList.Add(new Vector3(0, 0, 0));
                    }

                    zeroList.AddRange(list);
                    returnList.Add(zeroList);
                    break;
                case > 41:
                    break;
                default:
                    returnList.Add(list);
                    break;
            }
        }

        return returnList;
    }

    #endregion

    // Region used for reading from CSV and feeding to Neural Network

    #region From File to NN

    private void ReplayFromFile(bool buttonPressed, List<InputDevice> devicePressed)
    {
        if (!buttonPressed) return;

        _wasPressed = true;
        CSVToLists();
        SendToNN(_positions1, _velocities1, _rotations1, _positions2, _velocities2, _rotations2, _positions3,
            _velocities3, _rotations3);
        ClearLists();
        _wasPressed = false;
    }

    private void CSVToLists()
    {
        var path = $@"{readRecordPath}";
        string content = File.ReadAllText(path);
        List<float[]> rows = ParseCSV(content);
        var currentLists = new List<List<Vector3>>
        {
            _positions1,
            _velocities1,
            _rotations1,
            _positions2,
            _velocities2,
            _rotations2,
            _positions3,
            _velocities3,
            _rotations3
        };
        foreach (var row in rows)
        {
            var currentListIndex = 0;
            for (var i = 0; i < row.Length; i += 3)
            {
                var list = currentLists[currentListIndex];
                list.Add(new Vector3(row[i], row[i + 1], row[i + 2]));
                currentListIndex += 1;
            }
        }
    }

    List<float[]> ParseCSV(string csvText)
    {
        List<float[]> rows = new List<float[]>();
        string[] lines = csvText.Split('\n').Skip(1).ToArray();

        foreach (string line in lines)
        {
            if (line == "")
            {
                continue;
            }

            string[] items = line.Split(',');
            var floatItems = Array.ConvertAll(items, float.Parse);
            rows.Add(floatItems);
        }

        return rows;
    }

    #endregion
}