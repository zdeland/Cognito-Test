using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amazon;
using Amazon.Runtime;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using Amazon.CognitoSync;
using Amazon.CognitoSync.SyncManager;
using Facebook.Unity;
using System;

public class SyncClient : MonoBehaviour {
    public GameObject LoggedInCanvas;
    string name;
    int score;
    Dataset playerInfo;
    public CognitoSyncManager syncManager;
    public CognitoAWSCredentials credentials;


	// Use this for initialization
	void Start () {
        UnityInitializer.AttachToGameObject(this.gameObject);
        //remove this line if building on IOS device
        AWSConfigs.LoggingConfig.LogTo = LoggingOptions.UnityLogger;

        credentials = new CognitoAWSCredentials("us-east-1:b849fb4e-9c2b-4e3a-89a2-22f2372138dd", RegionEndpoint.USEast1);
        if (FB.IsLoggedIn)
        {
            credentials.AddLogin("graph.facebook.com", AccessToken.CurrentAccessToken.TokenString);
        }

        syncManager = new CognitoSyncManager(credentials,RegionEndpoint.USEast1);
        // Open our datasets
        playerInfo = syncManager.OpenOrCreateDataset("playerInfo");
       
        // Fetch locally stored data from a previous run
        name = string.IsNullOrEmpty(playerInfo.Get("name")) ? "Enter your name" : playerInfo.Get("name");
        //score = string.IsNullOrEmpty(playerInfo.Get("score")) ? "Enter your score" : Int32.Parse(playerInfo.Get("score").ToString());

        // Define Synchronize callbacks
        // when ds.SynchronizeAsync() is called the localDataset is merged with the remoteDataset 
        // OnDatasetDeleted, OnDatasetMerged, OnDatasetSuccess,  the corresponding callback is fired.
        // The developer has the freedom of handling these events needed for the Dataset
        playerInfo.OnSyncSuccess += this.HandleSyncSuccess; // OnSyncSucess uses events/delegates pattern
        playerInfo.OnSyncFailure += this.HandleSyncFailure; // OnSyncFailure uses events/delegates pattern
        playerInfo.OnSyncConflict = this.HandleSyncConflict;
        playerInfo.OnDatasetMerged = this.HandleDatasetMerged;
        playerInfo.OnDatasetDeleted = this.HandleDatasetDeleted;
    }

    private bool HandleDatasetDeleted(Dataset dataset)
    {
        Debug.Log("Cognito sync has deleted the dataset.");
        return false;
    }

    private bool HandleDatasetMerged(Dataset dataset, List<string> datasetNames)
    {
        Debug.Log("Cognito sync has merged data.");
        return false;
    }

    private bool HandleSyncConflict(Dataset dataset, List<SyncConflict> conflicts)
    {
        Debug.Log("Cognito sync has a conflict.");
        return false;
    }

    private void HandleSyncFailure(object sender, SyncFailureEventArgs e)
    {
        Debug.Log("Cognito sync has failed: " + e.ToString());
    }

    private void HandleSyncSuccess(object sender, SyncSuccessEventArgs e)
    {
        Debug.Log("Cognito sync was successful: "+e.ToString());
    }


    public void ChangeName(string newName)
    {
        name = newName;
        Debug.Log("Name updated...");
        playerInfo.Put("name", newName);
        playerInfo.SynchronizeAsync();
    }

    public void UpdateScore(string newScore)
    {
        playerInfo.Put("score", newScore);
        try
        {
            score = int.Parse(newScore);
            //playerInfo.Put("score",newScore);
        }
        catch
        {
            Debug.Log("Unable to parse score value from string "+newScore+" to int");
        }
    }

    public void Synchronize()
    {
        playerInfo.SynchronizeOnConnectivity(); //syncs data when it connects to a network
    }
}
