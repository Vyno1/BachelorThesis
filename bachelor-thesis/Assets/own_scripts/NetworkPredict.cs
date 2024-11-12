using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Barracuda;
using UnityEngine;

public class NetworkPredict : MonoBehaviour
{
    public NNModel modelAsset;
    private readonly string[] _actions = {"Wave_right", "Wave_left", "Throw_right", "Throw_left", "Point_right", "Point_left"};
    private Model _runTimeModel;
    private IWorker _worker; 

    // Start is called before the first frame update
    void Start()
    {
        _runTimeModel = ModelLoader.Load(modelAsset);
    }


    public void Predict(Tensor input)
    {
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, _runTimeModel);
        _worker.Execute(input);
        var output = _worker.PeekOutput();
        var outputArr = output.ToReadOnlyArray();
        var maxIndex = outputArr.ToList().IndexOf(outputArr.Max());
        print($"Predicted action: {_actions[maxIndex]}");
        output.Dispose();
        _worker.Dispose();
    }
}