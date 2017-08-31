﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SaveController : MonoBehaviour {

    public static SaveController saveController;
    public Boolean load = false;
    TextMesh aiScore;
    TextMesh playerScore;

    private void Awake()
    {
        if (saveController == null)
        {
            DontDestroyOnLoad(gameObject);
            saveController = this;
        }
        else if (saveController != this) {
            Destroy(gameObject);
        }
        
    }

    public void Save () {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/saveinfo.dat", FileMode.OpenOrCreate);

        SaveData data = new SaveData();
        data.playerScoreInt = Ball.playerScoreLoad;
        data.aiScoreInt = Ball.aiScoreLoad;

        bf.Serialize(file, data);
        file.Close();
	}
	

	public void Load () {

        if (File.Exists(Application.persistentDataPath + "/saveinfo.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/saveinfo.dat", FileMode.Open);
            SaveData data = (SaveData)bf.Deserialize(file);
            file.Close();
            Ball.playerScoreLoad = data.playerScoreInt;
            Ball.aiScoreLoad = data.aiScoreInt;
            Ball.load = true;

        }
    }
}

[Serializable]
class SaveData {
public int playerScoreInt;
public int aiScoreInt;
}