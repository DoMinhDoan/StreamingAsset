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


public class GameManager : MonoBehaviour
{

    [Header("Arena Objects")]
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
        StartCoroutine("RemoveLoadingScreen");
    }

    IEnumerator RemoveLoadingScreen()
    {
        yield return new WaitForSecondsRealtime(1);
        loadingScreen.SetActive(false);
        timer = 0.0f;
        Time.timeScale = 1.0f;
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
        SceneManager.LoadScene("MainScene"); 
        Time.timeScale = 1.0f;
        Rigidbody playerRB = playerTank.GetComponent<Rigidbody>();
        playerRB.isKinematic = true;
        playerRB.isKinematic = false;
    }


    public void StartNextLevel()
    {
        // Method to be completed via tutorial
    }

    public void ApplySkin(int indexOfSkin)
    {
        // Method to be completed via tutorial 
    }

}
