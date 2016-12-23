using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ServiceGrid : MonoBehaviour
{
    public ServiceToggle serviceTooglePrefab = null;

    public List<ServiceToggle> serviceToggleList = new List<ServiceToggle>();

    public IEnumerable<string> DisabledServiceNames
    {
        get
        {
            return serviceToggleList.Where((x) => !x.IsActive).Select((y) => y.service.ServiceName);
        }
    }

    public void CreateServiceToggle(ModSelectorService modSelectorService, ModSelectorService.Service service)
    {
        ServiceToggle modToggle = Instantiate<ServiceToggle>(serviceTooglePrefab);
        modToggle.modSelectorService = modSelectorService;
        modToggle.service = service;

        modToggle.gameObject.SetActive(true);
        modToggle.transform.SetParent(transform, false);

        serviceToggleList.Add(modToggle);
    }
}
