using System;

namespace WpfApp1.Events
{
    public static class TireBrandDeletedEvent
    {
        public static event EventHandler TireBrandDeleted;

        public static void OnTireBrandDeleted()
        {
            TireBrandDeleted?.Invoke(null, EventArgs.Empty);
        }
    }
}