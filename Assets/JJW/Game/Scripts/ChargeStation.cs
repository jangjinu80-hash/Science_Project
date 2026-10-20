namespace JJW.Code.Player
{
    public sealed class ChargeStation : LabDevice
    {
        public override string Describe(CapacitorInventory inventory)
        {
            return $"충전기  ·  공급 잔량 {inventory.Supply:0.0}\n[E] 충전량 배분  ·  화면을 여는 동안 시간 정지";
        }

        public override void Interact(StageController stage) => stage.OpenCharger();
    }
}
