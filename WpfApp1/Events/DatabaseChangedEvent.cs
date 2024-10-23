using System;

namespace WpfApp1.Events
{
    public static class DatabaseChangedEvent
    {
        public static event EventHandler VehicleBrandDeleted;

        public static void OnVehicleBrandDeleted()
        {
            VehicleBrandDeleted?.Invoke(null, EventArgs.Empty);
        }
    }
}