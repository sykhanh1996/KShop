using KShop.BackendServer.Data.Enums;

namespace KShop.BackendServer.Data.Interfaces
{
    public interface ISwitchable
    {
        Status Status { set; get; }
    }
}