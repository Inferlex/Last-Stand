using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Sprite[] sprites;
    public Transform pos;

    private void Start()
    {
        transform.SetParent(null);
    }

    private void Update()
    {
        if (pos)
            transform.position = pos.transform.position;
        else
            Destroy(gameObject);
    }

    public void ChangeSprites(int maxHp, int currentHp)
    {
        float hpIndex = (float) currentHp / (float) maxHp;
        if (hpIndex > 0 && hpIndex < 0.10)
            GetComponent<SpriteRenderer>().sprite = sprites[0];
        else if (hpIndex > 0.10 && hpIndex < 0.25)
            GetComponent<SpriteRenderer>().sprite = sprites[1];
        else if (hpIndex > 0.25 && hpIndex < 0.40)
            GetComponent<SpriteRenderer>().sprite = sprites[2];
        else if (hpIndex > 0.40 && hpIndex < 0.55)
            GetComponent<SpriteRenderer>().sprite = sprites[3];
        else if (hpIndex > 0.55 && hpIndex < 0.70)
            GetComponent<SpriteRenderer>().sprite = sprites[4];
        else if (hpIndex > 0.70 && hpIndex < 0.85)
            GetComponent<SpriteRenderer>().sprite = sprites[5];
        else if (hpIndex > 0.85 && hpIndex <= 1)
            GetComponent<SpriteRenderer>().sprite = sprites[6];
    }

    public void ChangeTransparency(bool value)
    {
        if (value)        
            GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
        else
            GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 0);

    }
}
