using BaseArchitecture.Core;

namespace SpaceInvaders.Project
{
    /// <summary>Shared by every mode: the managers behind it are scoped to the running one, so the
    /// same screen shows that mode's own gear.</summary>
    public class InventoryScreen : Screen<InventoryModel, InventoryView, InventoryController>
    {
    }
}
