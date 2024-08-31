using UnityEngine;

public class FlagController : MonoBehaviour
{
    public Country Country
    {
        get => country;
        set
        {
            country = value;
            SetFlag(country);
        }
    }

    [SerializeField]
    private Country country;

    [SerializeField]
    private MeshRenderer flag;

    void Start()
    {
        SetFlag(Country);
    }

    private void SetFlag(Country country)
    {
        flag.material.mainTexture = country.Texture;
    }
}
