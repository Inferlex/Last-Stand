using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class ProgressionSystem
{
    public static void SaveProgression(MainMenu menu)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/progression.omg";
        FileStream stream = new FileStream(path, FileMode.Create);

        GameData data = new GameData(menu.missionUnlocked);
        data.highScores = menu.highScores;
        data.soundVolume = menu.GetSoundVolume();
        data.musicVolume = menu.GetMusicVolume();

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static void SaveProgression(GameData gameData)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/progression.omg";
        FileStream stream = new FileStream(path, FileMode.Create);   

        formatter.Serialize(stream, gameData);
        stream.Close();
    }

    public static GameData LoadProgression()
    {
        string path = Application.persistentDataPath + "/progression.omg";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            GameData data = formatter.Deserialize(stream) as GameData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.Log("Save file not found in " + path);
            return null;
        }
    }
}
