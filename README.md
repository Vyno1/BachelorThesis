# Documentation for Unity and Python Notebook

This readme is detailing a couple of things that are important for the execution of the project

## Unity

The Unity installation is very straight forward. After pulling the repo make sure, that Barracuda is installed. If not go to https://docs.unity3d.com/Packages/com.unity.barracuda@3.0/manual/Installing.html for more information.
Once Barracuda is installed, you should see multiple folders in Assets, the most important ones are LSTM_Models, own_scripts, own_prefabs and own_scenes. own_scenes contains an already premade and setup scene that you can use to try out the script.  In the scene click on the recording cube and put in a valid path on your computer, where you want the CSV files to be saved. In the first dropdown you can choose the functionality between "RecordToFile", "SendToNN" and "TestFromFile".
### Record to file
Will record the performed actions to CSV files in the sepcified location. To run press the primary button (often B or X button on the handset controller) to start the recording and press it again to stop the recording.

### Test from file
Will read out the CSV file at the specified location, after pressing the primary button in VR. Prediction output of the Neural Network will be printed on the console. Make sure a Neural Network model from the LSTM_Models folder has been set in the inspector to enable predictions.

### Send to NN
Records the performed action in VR and immediately gives it to the Neural Network for prediction. Specify a path to a folder, where the actions are going to be saved. Press primary button once to start recording, press once more to stop the recording. As soon as the action has been performed and tracking has stopped, the motion will be saved as a CSV and the predicted output of the network will be displayed in the console window. Make sure a Neural Network model from the LSTM_Models folder has been set in the inspector to enable predictions.

## Selfsetup
If you want to setup your own scene, make sure that the VR prefab rig has been put into the scene. Also make sure to create an object, that you can attach the PrimaryButtonwatcher.cs, the RecordActions.cs and NetworkPredict.cs script to. Once that is done the rest follows the tutorial above.

## Python scripts

### HeadsetVelComp

A python script to compute the velocity of the headset based on the previous position and the current position. Replace the PATH TO CSV with a valid directory to execute.

### Python notebook

This is the python notebook that was used to train the Neural Network. If you want to use this notebook with your own dataset provide either the local path or a connect Google drive through the Google colab environment. Substitute the path_to_train and path_to_test with your corresponding paths and substitute the actions parameter with your recorded actions. Also provide a path for path_to_scores to save all the summarized scores. Running all cells before the cofusion matrices will run the network ten times and print out the avarage accuracy, as well as the accuracy of all the runs. They will be saved to your provided folder. You can also run the Confusion matrix section to create a confusion matrix from your best run. There is also a section which will export you chosen model to an ONNX model. The last section will run a "leave-one-subject-out" cross validation. Please make sure to provide a correct number of participants and name your folders correctly.
