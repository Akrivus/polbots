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
        var texture = Resources.Load<Texture>($"Flags/{country.ISO3166}");
        flag.material.mainTexture = texture;
    }
}
