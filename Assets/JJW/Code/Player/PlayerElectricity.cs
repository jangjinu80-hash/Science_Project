using System;
using UnityEngine;

public class PlayerElectricity : MonoBehaviour
{
   public float electricity = 0;
   public static PlayerElectricity Instance;

   private void Awake()
   {
      if (Instance == null)
         Instance = this;
   }
   
   
}
