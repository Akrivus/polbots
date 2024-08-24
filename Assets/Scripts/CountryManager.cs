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

    private Vector3 cameraTarget;
    private Vector3 cameraCenter;
    private bool isCentered;

    private void OnEnable()
    {
        names = countries.Select(c => c.Name).ToArray();
        controllers = new CountryController[0];
    }

    private void Update()
    {
        if (isCentered)
            return;
        var target = Vector3.Lerp(Camera.main.transform.position, cameraTarget, Time.deltaTime);
        var center = cameraCenter - cameraTarget;
        if (center.magnitude > 0.1f)
            SetCamera(target, Quaternion.Slerp(Camera.main.transform.rotation, Quaternion.LookRotation(center), Time.deltaTime));
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

    public void CenterCamera(bool set = false)
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
        cameraCenter = (first + last) / 2f;
        cameraTarget = new Vector3(cameraCenter.x, 0, distance);

        if (set)
            SetCamera(cameraTarget, cameraCenter);
        isCentered = set;
    }

    private void SetCamera(Vector3 target, Vector3 center)
    {
        SetCamera(target, Quaternion.LookRotation(center - target));
    }

    private void SetCamera(Vector3 target, Quaternion rotation)
    {
        Camera.main.transform.position = target;
        Camera.main.transform.rotation = rotation;
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
