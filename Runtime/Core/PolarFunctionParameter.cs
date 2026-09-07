using UnityEngine;

[System.Serializable]
public class PolarFunctionParameter
{
    [SerializeField]
    private string name;

    [SerializeField]
    private float value;

    public string Name => name;

    public float Value
    {
        get => value;
        set => this.value = value;
    }
}
