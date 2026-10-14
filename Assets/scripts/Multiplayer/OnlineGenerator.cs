using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Src.Units
{
    public class OnlineGenerator : OnlineBuilding
    {
        public int income = 10;
        public bool resourceType;
        public GameObject generatorArea;
        private int degradation = 0;

        public new void Start()
        {
            GameEvents.current.OnOnlineBlueprintGeneratorStart += OnBlueprintGeneratorStart;
            base.Start();
            //StartCoroutine(Generate());
        }

        public IEnumerator Generate()
        {
            while (income > 0) //as the income of the generator would degrade, it would stop generating
            {
                yield return new WaitForSeconds(4f);
                if (resourceType)
                {
                    GameEvents.current.ResourceEarn(new int[2] {GetId(), income });
                }
                else
                {
                    GameEvents.current.SpResourceEarn(new int[2] { GetId(), income });
                }
                degradation++;
                if (degradation >= 35)
                { //if degradation exceeds, then the generator income is decreased
                    income--;
                    degradation = 0;
                }
            }
        }

        public void OnBlueprintGeneratorStart(int[] vals)
        {    
            if (vals[1] != GetId()) return;
            if (getRole() == Role.Generator && generatorArea)
                if (vals[0] == 1)
                {
                    generatorArea.GetComponent<SpriteRenderer>().color = new Color32(255, 0, 0, 255);
                }
                else
                {
                    generatorArea.GetComponent<SpriteRenderer>().color = new Color32(255, 0, 0, 0);
                }
        }

        public override void Activate(bool val)
        {
            base.Activate(val);
            if (IsActivated())
            {
                StartCoroutine(Generate());
            }          
        }
    }
}
