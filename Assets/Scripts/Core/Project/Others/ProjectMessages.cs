using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    public readonly struct ItemEquipChangedMessage : IMessageObject
    {
        public string EquippedInstanceId { get; }
        public string UnequippedInstanceId { get; }

        public ItemEquipChangedMessage(string equippedInstanceId, string unequippedInstanceId)
        {
            EquippedInstanceId = equippedInstanceId;
            UnequippedInstanceId = unequippedInstanceId;
        }
    }

    public readonly struct ItemSoldMessage : IMessageObject
    {
        public string InstanceId { get; }
        public int Value { get; }

        public ItemSoldMessage(string instanceId, int value)
        {
            InstanceId = instanceId;
            Value = value;
        }
    }
}
