using Content.Server.EUI;
using Content.Shared.Eui;
using Content.Shared._Gabystation.ServerCurrency.UI;

namespace Content.Server._Gabystation.ServerCurrency.UI
{
    public sealed class CurrencyEui : BaseEui
    {

        public CurrencyEui()
        {
            IoCManager.InjectDependencies(this);
        }

        public override void Opened()
        {
            StateDirty();
        }

        public override EuiStateBase GetNewState()
        {
            return new CurrencyEuiState();
        }


        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);
        }
    }
}
