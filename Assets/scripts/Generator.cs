using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Src.Units
{
    public class Generator : Building
    {
        public int income = 10;
        public bool resourceType;
        public GameObject generatorArea;
        private int degradation = 0;

        public new void Start()
        {
            GameEvents.current.OnBlueprintGeneratorStart += OnBlueprintGeneratorStart;
            base.Start();
            StartCoroutine(Generate());
        }

        public IEnumerator Generate()
        {
            while (income > 0) //as the income of the generator would degrade, it would stop generating
            {
                yield return new WaitForSeconds(4f);
                if (resourceType)
                {
                    GameEvents.current.ResourceEarn(income);
                }
                else
                {
                    GameEvents.current.SpResourceEarn(income);
                }
                degradation++;
                if (degradation >= 35)
                { //if degradation exceeds, then the generator income is decreased
                    income--;
                    degradation = 0;
                }
            }
        }

        public void OnBlueprintGeneratorStart(bool value)
        {
            if (getRole() == Role.Generator && generatorArea)
                if (value)
                {
                    generatorArea.GetComponent<SpriteRenderer>().color = new Color32(255, 0, 0, 255);
                }
                else
                {
                    generatorArea.GetComponent<SpriteRenderer>().color = new Color32(255, 0, 0, 0);
                }
        }
    }
}
