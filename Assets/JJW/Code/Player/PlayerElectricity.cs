using System;
using UnityEngine;

public class PlayerElectricity : MonoBehaviour,IElectricity
{
   private float maxElectricity = 1000;
   private float minElectricity = 0;
   public float CurrentElectricity { get => electricity; }
   public float electricity { get; private  set; }

   public void SetElectricity(float value)
   {
      electricity += value;
   }
}
