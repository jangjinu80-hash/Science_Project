using System;
using UnityEngine;

namespace JJW.Code.Player
{
    public sealed class CapacitorInventory : MonoBehaviour
    {
        public static readonly string[] Names = { "기본형", "급속형", "유지형" };
        public static readonly float[] Capacities = { 40, 80, 30 };
        public static readonly float[] Outputs = { 20, 60, 8 };
        public static readonly float[] LossRates = { 0.06f, 0.35f, 0.015f };
        private readonly float[] energy = new float[3];
        public int Selected { get; private set; }
        public int Unlocked { get; private set; }
        public float Supply { get; private set; }
        public float Current => energy[Selected];
        public event Action Changed;

        public void Initialize(StageDefinition stage)
        {
            Array.Clear(energy, 0, energy.Length);
            Selected = 0;
            Unlocked = stage.capacitorCount;
            Supply = stage.supply;
            Changed?.Invoke();
        }

        private void Update()
        {
            if (Time.deltaTime <= 0) return;
            for (int i = 0; i < Unlocked; i++)
                energy[i] = Mathf.Max(0, energy[i] - LossRates[i] * Time.deltaTime);
        }

        public float GetEnergy(int index) => energy[index];
        public float ChargeLimit => Mathf.Max(0, Mathf.Min(Supply, Capacities[Selected] - Current));

        public void Select(int index)
        {
            if (index < 0 || index >= Unlocked) return;
            Selected = index;
            Changed?.Invoke();
        }

        public float Charge(float amount)
        {
            float transferred = Mathf.Clamp(amount, 0, ChargeLimit);
            energy[Selected] += transferred;
            Supply = Mathf.Max(0, Supply - transferred);
            Changed?.Invoke();
            return transferred;
        }

        public string Check(float cost, float output)
        {
            if (Outputs[Selected] < output) return "출력 부족 · 급속형으로 교체하세요";
            if (Current + 0.001f < cost) return $"에너지 부족 · {cost:0} 필요 / 현재 {Current:0.0}";
            return "";
        }

        public bool Spend(float cost, float output)
        {
            if (Check(cost, output).Length != 0) return false;
            energy[Selected] = Mathf.Max(0, Current - cost);
            Changed?.Invoke();
            return true;
        }
    }
}
