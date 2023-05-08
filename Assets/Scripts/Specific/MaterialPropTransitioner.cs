using System.Collections.Generic;
using System.Collections;
using NaughtyAttributes;
using DG.Tweening;
using UnityEngine;
using System;

[Serializable]
public class MaterialProperty
{
    [SerializeField] public string name;
    [SerializeField, Dropdown("GetTypeValues")] public int type;

    static private Type[] types = new Type[] { typeof(float), typeof(Vector3) };
    private DropdownList<int> GetTypeValues() {
        return new DropdownList<int>()
        {
            { "Float", 0 },
            { "Vector3", 1 }
        };
    }

    static public Type GetType(int type) => types[type];
}

public delegate void UpdateMaterial(Material newMaterial);

public class MaterialPropTransitioner: MonoBehaviour
{
    [SerializeField, BoxGroup("Materials")] public Material initialMaterial;
    [SerializeField, BoxGroup("Materials")] public Material mainMaterial;
    [SerializeField, BoxGroup("Materials")] public Material finalMaterial;
    [SerializeField, BoxGroup("Materials")] public bool forceInitOnStart = true;
    [SerializeField, BoxGroup("Materials")] public bool autoResetOnStop = true;
    [SerializeField] public float animationDuration = 6f;
    [SerializeField] public Ease materialColorAnimationEase = Ease.Linear;
    [SerializeField] public List<MaterialProperty> propertiesToTransition;

    [Button]
    public void ResetMaterial() => this.mainMaterial.CopyPropertiesFromMaterial(this.initialMaterial);
    [Button]
    public void TestFinalMaterial() => this.mainMaterial.CopyPropertiesFromMaterial(this.finalMaterial);

    void Start()
    {
        if(this.forceInitOnStart) {
            this.ResetMaterial();
        }
    }

    void OnApplicationQuit()
    {
        if(this.autoResetOnStop) {
            this.ResetMaterial();
        }
    }

    public void Transition() {
        if(this.mainMaterial == null || this.initialMaterial == null || this.finalMaterial == null) {
            return;
        }

        foreach (var property in this.propertiesToTransition) {
            if(!this.mainMaterial.HasProperty(property.name)) {
                print("MainMaterial.Shader não possui a propriedade " + property.name + ", skipping!");
                continue;
            }
            if(!this.initialMaterial.HasProperty(property.name)) {
                print("InitialMaterial.Shader não possui a propriedade " + property.name + ", skipping!");
                continue;
            }
            if(!this.finalMaterial.HasProperty(property.name)) {
                print("FinalMaterial.Shader não possui a propriedade " + property.name + ", skipping!");
                continue;
            }

            if(MaterialProperty.GetType(property.type) == typeof(float)) {
                this.SetMainMaterialValue(property.name, this.initialMaterial.GetFloat(property.name), this.finalMaterial.GetFloat(property.name));
            } else if(MaterialProperty.GetType(property.type) == typeof(Vector3)) {
                this.SetMainMaterialValue(property.name, this.initialMaterial.GetVector(property.name), this.finalMaterial.GetVector(property.name));
            } else {
                print("Tipo da propriedade " + property.name + " não é tratada, skipping!");
                //continue;
            }           
        }
    }

    private void SetMainMaterialValue(string propertyName, float currentValue, float finalValue) {
        //print("Float - Transitioning from " + currentValue + " to " + finalMaterial);
        DOTween
            .To(() => currentValue, x => currentValue = x, finalValue, this.animationDuration)
            .OnUpdate(() => {
                this.mainMaterial.SetFloat(propertyName, currentValue);
            });
    }

    private void SetMainMaterialValue(string propertyName, Vector3 currentValue, Vector3 finalValue) {
        //print("Vector3 - Transitioning from " + currentValue + " to " + finalMaterial);
        DOTween
            .To(() => currentValue, x => currentValue = x, finalValue, this.animationDuration)
            .OnUpdate(() => {
                this.mainMaterial.SetVector(propertyName, currentValue);
            });
    }
}
