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
        CenterCamera();
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
        fab.transform.localPosition = new Vector3(
            index * country.Size * 1.2f, 0, 0);
        controllers[index] = fab;
    }

    private void CenterCamera()
    {
        if (controllers.Length == 0)
            return;

        var first = controllers[0].transform.position;
        var last = controllers[controllers.Length - 1].transform.position;

        var distance = Vector3.Distance(first, last) + 1f;
        var center = (first + last) / 2;
        var position = new Vector3(center.x, 0, distance);

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
