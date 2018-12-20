/*
 * Copyright (c) 2017 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * Notwithstanding the foregoing, you may not use, copy, modify, merge, publish, 
 * distribute, sublicense, create a derivative work, and/or sell copies of the 
 * Software in any work that is designed, intended, or marketed for pedagogical or 
 * instructional purposes related to programming, coding, application development, 
 * or information technology.  Permission for such use, copying, modification,
 * merger, publication, distribution, sublicensing, creation of derivative works, 
 * or sale is expressly withheld.
 *    
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Networking;
using System.Collections.Generic;

[System.Serializable]
public class Arena
{
    public string arenaFilePath;
    public int levelNumber;
    public string levelName;
    public float startX;
    public float startZ;
    public float targetX;
    public float targetZ;
}

public class GameManager : MonoBehaviour
{
    [Header("Arena")]
    public List<Arena> arenaList = new List<Arena>();
    public Texture2D arenaTexture;

    [Header("Arena Prefab")]
    public GameObject floorPrefab;
    public GameObject weakFloorPrefab;
    public GameObject wallPrefab;
    public GameObject weakWallPrefab;
    public GameObject mineTilePrefab;

    [Header("Arena Objects")]
    public GameObject defaultArena;
    public GameObject arenaTiles;
    public GameObject target;

    private GameObject playerTank;

    [Header("Game UI")]
    public GameObject loadingScreen;
    public GameObject pauseMenuCamera;
    public GameObject pauseScreen;
    public GameObject gameOverScreen;
    public GameObject winScreen;
    public string arenaName = "Starter Level";

    public GameObject PlayerUI;

    [Space]
    public int currentLevel = 1;
    private bool isPaused = false;

    public Text timerText;
    private float timer;
    public string formattedTime;

    public Image playerAvatar;
    public Text playerName;

    public AudioSource musicPlayer;

    [Header("Tank Customisation")]
    public Texture2D tankTexture;
    public Texture2D tankTreads;
    public Renderer tankRenderer;
    private Texture2D newTankTexture;
    private Vector3 defaultTankPrimary = new Vector3(580, 722, 467);
    private Vector3 defaultTankSecondary = new Vector3(718, 149, 0);

    [Header("Skin")]
    public List<Texture2D> tankSkins;
    public List<string> tankSkinNames;
    public GameObject skinContainer;
    public GameObject skinObject;

    private bool skinMenuAssembled = false;


    public void UpdateTimerUI()
    {
        timer += Time.deltaTime;
        int minutes = Mathf.FloorToInt(timer / 60F);
        int seconds = Mathf.FloorToInt(timer - minutes * 60);
        formattedTime = string.Format("{0:0}:{1:00}", minutes, seconds);
        timerText.text = arenaName + " " + formattedTime;
    }

    void Start()
    {
        loadingScreen.SetActive(true);
        Time.timeScale = 0.0f;
        playerTank = GameObject.FindGameObjectWithTag("Player");

        DirectoryInfo directoryInfo = new DirectoryInfo(Application.streamingAssetsPath);
        print("Streaming Assets Path : " + Application.streamingAssetsPath);
        FileInfo[] allFiles = directoryInfo.GetFiles("*.*");
        foreach (var file in allFiles)
        {
            if (file.Name.Contains("player1"))
            {
                StartCoroutine("LoadPlayerUI", file);
            }
            else if (file.Name.Contains("playercolor"))
            {
                StartCoroutine("LoadPlayerColor", file);
            }
            else if (file.Name.Contains("skin"))
            {
                StartCoroutine("LoadSkin", file);
            }
            else if (file.Name.Contains("Arena"))
            {
                StartCoroutine("LoadArena", file);
            }
        }

        if (arenaList.Count != 0)
        {
            Destroy(defaultArena);
            StartCoroutine("LoadLevel", arenaList[0]);
        }

        StartCoroutine("RemoveLoadingScreen");
    }

    IEnumerator LoadPlayerUI(FileInfo file)
    {
        if (file.Name.Contains("meta"))
        {
            yield break;
        }
        else
        {
            string playerFileWithoutExtension = Path.GetFileNameWithoutExtension(file.ToString());
            string[] playerNameData = playerFileWithoutExtension.Split(" "[0]);

            string strPlayerName = "";

            for (int i = 1; i < playerNameData.Length; i++)
            {
                strPlayerName = strPlayerName + playerNameData[i] + " ";
            }

            string wwwPlayerFilePath = "file://" + file.FullName.ToString();
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(wwwPlayerFilePath);
            yield return www.SendWebRequest();

            Texture2D avatarTex = ((DownloadHandlerTexture)www.downloadHandler).texture;
            playerAvatar.sprite = Sprite.Create(avatarTex, new Rect(0, 0, avatarTex.width, avatarTex.height), new Vector2(0.5f, 0.5f));
            playerName.text = strPlayerName;
        }
    }

    IEnumerator LoadBackgroundMusic(FileInfo musicFile)
    {
        if (musicFile.Name.Contains("meta"))
        {
            yield break;
        }
        else
        {
            string musicFilePath = musicFile.FullName.ToString();
            string url = string.Format("file://{0}", musicFilePath);

            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS);
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                musicPlayer.clip = DownloadHandlerAudioClip.GetContent(www);
                musicPlayer.Play();
            }

        }
    }

    IEnumerator LoadPlayerColor(FileInfo colorFile)
    {
        if (colorFile.Name.Contains("meta"))
        {
            yield break;
        }
        else
        {
            string wwwPlayerColorPath = "file://" + colorFile.FullName.ToString();
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(wwwPlayerColorPath);
            yield return www.SendWebRequest();

            Texture2D playerColorTex = ((DownloadHandlerTexture)www.downloadHandler).texture;

            Color primaryColor = playerColorTex.GetPixel(5, 5);
            Color secondaryColor = playerColorTex.GetPixel(15, 5);

            Color[] currentPixelColors = tankTexture.GetPixels();
            Color[] newPixelColors = new Color[currentPixelColors.Length];

            float percentageDifferenceAllowed = 0.05f;

            for (int i = 0; i < currentPixelColors.Length; i++)
            {

                // If the color matches the defaultTankPrimary you hard coded, the new primaryColor value is saved in its place to the newPixelColor array. 
                // If the color matches the defaultTankSecondary, save the new secondaryColor;
                // if the color matches neither, simply save the same color back.
                Vector3 colorToTest = new Vector3(Mathf.RoundToInt(currentPixelColors[i].r * 1000), Mathf.RoundToInt(currentPixelColors[i].g * 1000), Mathf.RoundToInt(currentPixelColors[i].b * 1000));
                if ((colorToTest - defaultTankPrimary).sqrMagnitude <= (colorToTest * percentageDifferenceAllowed).sqrMagnitude)
                {
                    newPixelColors[i] = primaryColor;
                }
                else if ((colorToTest - defaultTankSecondary).sqrMagnitude <= (colorToTest * percentageDifferenceAllowed).sqrMagnitude)
                {
                    newPixelColors[i] = secondaryColor;
                }
                else
                {
                    newPixelColors[i] = currentPixelColors[i];
                }
            }

            newTankTexture = new Texture2D(tankTexture.width, tankTexture.height);
            newTankTexture.SetPixels(newPixelColors);
            newTankTexture.Apply();

            ApplyTextureToTank(tankRenderer, newTankTexture);
        }
    }

    IEnumerator LoadSkin(FileInfo file)
    {
        if (file.Name.Contains("meta"))
        {
            yield break;
        }
        else
        {
            string skinFileWithoutExtension = Path.GetFileNameWithoutExtension(file.ToString());
            string[] tankSkinData = skinFileWithoutExtension.Split(" "[0]);

            string skinName = tankSkinData[0];

            string wwwTankSkinFilePath = "file://" + file.FullName.ToString();
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(wwwTankSkinFilePath);
            yield return www.SendWebRequest();

            Texture2D skinTex = ((DownloadHandlerTexture)www.downloadHandler).texture;

            tankSkins.Add(skinTex);
            tankSkinNames.Add(skinName);
        }
    }

    IEnumerator LoadArena(FileInfo file)
    {
        if (file.Name.Contains("meta"))
        {
            yield break;
        }
        else
        {
            Arena arena = new Arena();
            // arena <x coordinate of player start> <y coordinate of player start> <x coordinate of target> <y coordinate of target> <Name of your level>.png
            string arenaFileWithoutExtension = Path.GetFileNameWithoutExtension(file.ToString());
            string[] arenaData = arenaFileWithoutExtension.Split(" "[0]);

            string levelName = "";
            if (arenaData.Length < 5)
            {
                Debug.LogError("Wrong in file name. It should be in formatn : arena <x coordinate of player start> <y coordinate of player start> <x coordinate of target> <y coordinate of target> <Name of your level>.png");
                yield break;
            }
            else if (arenaData.Length == 5)   // Not contain name in asset
            {
                levelName = "" + (arenaList.Count + 1);
            }
            else
            {
                for (int i = 5; i < arenaData.Length; i++)
                {
                    levelName = levelName + arenaData[i] + " ";
                }
            }

            arena.startX = int.Parse(arenaData[1]);
            arena.startZ = int.Parse(arenaData[2]);
            arena.targetX = int.Parse(arenaData[3]);
            arena.targetZ = int.Parse(arenaData[4]);
            arena.levelName = levelName;
            arena.arenaFilePath = "file://" + file.FullName.ToString();

            arenaList.Add(arena);
        }
    }

    IEnumerator LoadLevel(Arena arenaToLoad)
    {
        arenaName = arenaToLoad.levelName;

        loadingScreen.SetActive(true);
        gameOverScreen.SetActive(false);
        winScreen.SetActive(false);

        foreach (Transform child in arenaTiles.transform)
        {
            Destroy(child.gameObject);
        }

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(arenaToLoad.arenaFilePath);
        yield return www.SendWebRequest();

        Texture2D arenaTex = ((DownloadHandlerTexture)www.downloadHandler).texture;

        // yellow = 255 255 0 = No tiles
        // red = 255 0 0 = Floor tile with mine
        // blue = 0 0 255 = Weak floor
        // green = 0 255 0 = Weak wall
        // black = 0 0 0 = Nornal wall
        // alpha = 0 = Normal floor
        Color[] arenaColor = arenaTex.GetPixels();
        for(int i = 0; i< arenaColor.Length; i++)
        {
            int xPosition = ((i + 1) % 100);
            if (xPosition == 0)
            {
                xPosition = 100;
            }
            int zPosition = (i / 100) + 1;

            if (arenaColor[i].a < 0.1f) // alpha = 0 = Normal floor
            {
                GameObject.Instantiate(floorPrefab, new Vector3(xPosition / 1.0f, 0.0f, zPosition / 1.0f), Quaternion.Euler(90, 0, 0), arenaTiles.transform);
            }
            else
            {
                if (arenaColor[i].r > 0.9f && arenaColor[i].g > 0.9f && arenaColor[i].b < 0.1f) // yellow = 255 255 0 = No tiles
                {

                }
                else if (arenaColor[i].r > 0.9f && arenaColor[i].g < 0.1f && arenaColor[i].b < 0.1f)    // red = 255 0 0 = Floor tile with mine
                {
                    GameObject.Instantiate(mineTilePrefab, new Vector3(xPosition / 1.0f, 0.0f, zPosition / 1.0f), Quaternion.identity, arenaTiles.transform);
                }
                else if (arenaColor[i].r < 0.1f && arenaColor[i].g > 0.9f && arenaColor[i].b < 0.1f)    // green = 0 255 0 = Weak wall
                {
                    GameObject.Instantiate(weakWallPrefab, new Vector3(xPosition / 1.0f, 0.0f, zPosition / 1.0f), Quaternion.identity, arenaTiles.transform);
                }
                else if (arenaColor[i].r < 0.1f && arenaColor[i].g < 0.1f && arenaColor[i].b > 0.9f)    // blue = 0 0 255 = Weak floor
                {
                    GameObject.Instantiate(weakFloorPrefab, new Vector3(xPosition / 1.0f, 0.0f, zPosition / 1.0f), Quaternion.identity, arenaTiles.transform);
                }
                else    // black = 0 0 0 = Nornal wall
                {
                    GameObject.Instantiate(wallPrefab, new Vector3(xPosition / 1.0f, 0.0f, zPosition / 1.0f), Quaternion.identity, arenaTiles.transform);
                }
            }
        }

        StartCoroutine("RemoveLoadingScreen");
        Time.timeScale = 1.0f;

        playerTank.transform.position = new Vector3(arenaToLoad.startX / 1.0f, 1.0f, (100 - arenaToLoad.startZ) / 1.0f);
        playerTank.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

        target.transform.position = new Vector3(arenaToLoad.targetX / 1.0f, 0.6f, (100 - arenaToLoad.targetZ) / 1.0f);
    }

    void ApplyTextureToTank(Renderer tankRenderer, Texture2D texture)
    {
        Renderer[] renders = tankRenderer.GetComponentsInChildren<Renderer>();
        foreach (var render in renders)
        {
            render.material.mainTexture = texture;
        }

        tankRenderer.materials[1].mainTexture = texture;
        tankRenderer.materials[0].mainTexture = tankTreads;
    }

    IEnumerator RemoveLoadingScreen()
    {
        yield return new WaitForSecondsRealtime(1);
        loadingScreen.SetActive(false);
        timer = 0.0f;
        Time.timeScale = 1.0f;

        if (!skinMenuAssembled)
        {
            StartCoroutine("AssembleSkinMenu");
        }
    }

    IEnumerator AssembleSkinMenu()
    {
        skinMenuAssembled = true;

        for (int i = 0; i < tankSkins.Count; i++)
        {
            GameObject skinObj = Instantiate(skinObject, new Vector3(0, 0, 0), Quaternion.identity, skinContainer.transform);

            skinObj.transform.localPosition = new Vector3(100 + (200 * i), -80, 0);

            SkinManager skinManager = skinObj.GetComponent<SkinManager>();
            skinManager.ConfigureSkin(tankSkinNames[i], i);
            ApplyTextureToTank(skinManager.tankRenderer, tankSkins[i]);
        }

        yield return null;
    }

    void Update()
    {
        UpdateTimerUI();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                PlayerUI.SetActive(true);
                pauseScreen.SetActive(false);
                pauseMenuCamera.SetActive(false);
                isPaused = false;
                Time.timeScale = 1.0f;
            }
            else
            {
                PlayerUI.SetActive(false);
                pauseScreen.SetActive(true);
                pauseMenuCamera.SetActive(true);
                isPaused = true;
                Time.timeScale = 0.0f;
            }
        }
    }

    public void RestartLevel()
    {
        if(arenaList.Count > 0)
        {
            StartCoroutine("LoadLevel", arenaList[currentLevel - 1]);
        }
        else
        {
            SceneManager.LoadScene("MainScene");
        }
        Time.timeScale = 1.0f;
        Rigidbody playerRB = playerTank.GetComponent<Rigidbody>();
        playerRB.isKinematic = true;
        playerRB.isKinematic = false;
    }


    public void StartNextLevel()
    {
        if(currentLevel < arenaList.Count)
        {
            currentLevel++;
            StartCoroutine("LoadLevel", arenaList[currentLevel - 1]);
        }
        else
        {
            SceneManager.LoadScene("MainScene");
        }

        // There may be residual input applied to the tank when restarting or transitioning between levels.
        // Toggle the player's Rigidbody from kinematic to non-kinematic to zero this out.
        playerTank.GetComponent<Rigidbody>().isKinematic = true;
        playerTank.GetComponent<Rigidbody>().isKinematic = false;
    }

    public void ApplySkin(int indexOfSkin)
    {        
        ApplyTextureToTank(tankRenderer, tankSkins[indexOfSkin]);
        PlayerUI.SetActive(true);
        pauseMenuCamera.SetActive(false);
        isPaused = false;
        Time.timeScale = 1.0f;
    }

}
