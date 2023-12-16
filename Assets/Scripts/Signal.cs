using System.Collections;
using UnityEngine;
///<summary>
///
///</summary>
namespace TrafficBase
{
    public class Signal : MonoBehaviour
    {
        [HideInInspector]
        public Color color;
        public SteeringDirection hinder;
        private void Start()
        {
            int seed = System.DateTime.Now.Millisecond;
            Random.InitState(seed);
            float num = Random.Range(0f, 1f);
            if (num < 0.5f)
                color = Color.red;
            else
                color = Color.green;
            GetComponent<MeshRenderer>().material.color = color;
            InvokeRepeating("AlterColor", 8f, 8f);
        } 
        private void AlterColor()
        {
            if (color == Color.green)
            {
                StartCoroutine(Aux1(Color.green,Color.red));
            }
            else
            {
                StartCoroutine(Aux1(Color.red,Color.green));
            }
        }
        private IEnumerator Aux1(Color src,Color dst)
        {
            for(int i=0;i<3;i++)
            {
                color = src;
                // Enable emission
                GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
                // Set the emission intensity
                float emissionIntensity = 0.2f; // Adjust the intensity as needed
                GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", src * emissionIntensity);
                GetComponent<MeshRenderer>().material.color = color;
                yield return new WaitForSeconds(0.25f);
                if (i == 2) break;
                emissionIntensity = 0.7f; // Adjust the intensity as needed
                GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", src * emissionIntensity);
                GetComponent<MeshRenderer>().material.color = color;
                yield return new WaitForSeconds(0.25f);
            }
            color = dst;
            GetComponent<MeshRenderer>().material.color = color;
        }
    }

}