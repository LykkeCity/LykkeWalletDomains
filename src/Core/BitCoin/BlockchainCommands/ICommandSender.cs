using System.Threading.Tasks;

namespace Core.BitCoin.BlockchainCommands
{
    public interface ICommandSender
    {
        Task SendCommand(string command);
    }
}
