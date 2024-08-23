using System.Linq;
using UnityEngine;

public class CountryManager : MonoBehaviour
{
    public Country this[string name] => countries.FirstOrDefault(c => c.Name == name);

    public string[] names { get; private set; }

    [SerializeField]
    public Country[] countries;

    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private Transform group;

    [SerializeField]
    public CountryController[] controllers;

    private void OnEnable()
    {
        names = countries.Select(c => c.Name).ToArray();
        controllers = new CountryController[0];
    }

    public void SpawnCountries(Country[] countries)
    {
        controllers = new CountryController[countries.Length];
        for (var i = 0; i < countries.Length; ++i)
            SpawnCountry(countries[i], i);
    }

    public void DespawnCountries()
    {
        foreach (var controller in controllers)
            Destroy(controller.gameObject);
        controllers = new CountryController[0];
    }

    private void SpawnCountry(Country country, int index)
    {
        var fab = Instantiate(prefab, group)
            .GetComponent<CountryController>();
        fab.SetCountry(country);
        fab.position = new Vector3(
            index * country.Size * 1.2f, 0, 0);
        fab.manager = this;
        controllers[index] = fab;
    }

    public void CenterCamera()
    {
        if (controllers.Length == 0)
            return;

        var length = controllers.Length;
        int i;
        for (i = 0; i < length; ++i)
            if (!controllers[i].IsActive)
                break;
        i = i - 1 < 0 ? 0 : i - 1;

        var first = controllers[0].position;
        var last = controllers[i].position;

        var distance = Vector3.Distance(first, last) + length / (i + 1f);
        var center = (first + last) / 2f;
        var position = new Vector3(center.x, 0, distance);

        Debug.DrawRay(center, Vector3.up, Color.red);

        Camera.main.transform.position = position;
        Camera.main.transform.LookAt(center);
    }

    public CountryController Get(string name)
    {
        return controllers.FirstOrDefault(c => c.Name == name);
    }

    public bool Has(string name)
    {
        return controllers.Any(c => c.Name == name);
    }

    public bool Knows(string name)
    {
        return countries.Any(c => c.Name == name);
    }
}
