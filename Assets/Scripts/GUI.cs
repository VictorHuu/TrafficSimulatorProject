using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
///<summary>
///
///</summary>
namespace TrafficBase
{
    public class GUI : MonoBehaviour
    {
        private GUIStyle buttonStyle;
        private GUIStyle labelStyle;
        bool IsPause = false;
        private void OnGUI()
        {
            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle();
                buttonStyle.fontSize = 24;
                buttonStyle.normal.textColor = Color.white;
                buttonStyle.normal.background = MakeTex(2, 2, new Color(0.2f, 0.3f, 0.4f, 1f));
                buttonStyle.hover.background = MakeTex(2, 2, new Color(0.1f, 0.2f, 0.2f, 1f));
            }
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle();
                labelStyle.normal.textColor = Color.black; // Set the desired color for the label text
            }
            
            
            if (SceneManager.GetActiveScene().name == "Begin")
            {
                if (GUILayout.Button("退出程序", buttonStyle))
                {
                    Application.Quit();
                }
                GUILayout.Label("十字路口最大通过量: " + Crossing.Capacity.ToString("F2"),labelStyle);
                Crossing.Capacity = (int)GUILayout.HorizontalSlider(Crossing.Capacity, 0f, 5f);
                GUIStyle buttonStyle2=new GUIStyle();
                buttonStyle2.fontSize = 48;
                GUILayout.BeginArea(new Rect((Screen.width - 240f) / 2f, (Screen.height - 60f) / 2f, 240f, 60f));
                GUILayout.ExpandWidth(true);
                GUILayout.ExpandHeight(true);
                if (GUILayout.Button("开始演示", buttonStyle2))
                    SceneManager.LoadScene("SampleScene");
                GUILayout.EndArea();
            }
            if (SceneManager.GetActiveScene().name == "SampleScene")
            {
                if (GUILayout.Button("返回开始界面", buttonStyle))
                    SceneManager.LoadScene("Begin");
                GUILayout.BeginArea(new Rect(Screen.width-120f, 0f, 120f, 60f));
                if (!IsPause)
                    if (GUILayout.Button("暂停演示", buttonStyle)) {
                        Time.timeScale = 0f;
                        IsPause= true;
                    }
                if (IsPause)
                    if (GUILayout.Button("恢复演示", buttonStyle))
                    {
                        Time.timeScale = 1f;
                        IsPause = false;
                    }
                GUILayout.EndArea();
                GUILayout.BeginArea(new Rect(0f, Screen.height-60f, 120f, 60f));
                GUILayout.Label("演示倍速: " + Time.timeScale.ToString("F2"), labelStyle) ;
                Time.timeScale = GUILayout.HorizontalSlider(Time.timeScale, 0.5f, 3f);
                GUILayout.EndArea();
            }
        }

        private Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }



    }
}