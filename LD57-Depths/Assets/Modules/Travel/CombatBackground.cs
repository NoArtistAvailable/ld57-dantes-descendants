using elZach.Common;
using LD57;
using UnityEngine;

public class CombatBackground : MonoBehaviour
{
    public Texture2D[] backgrounds;
    public Color[] colors;

    public int test = 0;
    public Button<CombatBackground> testButton = new Button<CombatBackground>(x =>
    {
        PlayerManager.instance.circleOfHell = x.test;
        x.Start();
    });

    void Start()
    {
        var mat = GetComponent<Renderer>().material;
        mat.SetTexture("_MainTex",backgrounds[PlayerManager.instance.circleOfHell]);
        mat.SetColor("_Color",colors[PlayerManager.instance.circleOfHell]);
    }
}
