using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerController : BaseController
{
    public ContainerInfoSO feet;
    public Color color;
    public string TemplateID;

    public void SetInfo(string templateID)
    {
        this.TemplateID = templateID;
    }

    
}
