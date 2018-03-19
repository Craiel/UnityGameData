namespace Craiel.UnityGameData.Contracts
{
    public delegate bool GameDataValidationFixDelegate(object owner, object entry);
    
    public interface IGameDataRuntimeValidationContext
    {
        void Warning(object owner, object source, GameDataValidationFixDelegate fixDelegate, string message);

        void WarningFormat(object owner, object source, GameDataValidationFixDelegate fixDelegate,
            string message, params object[] formatArguments);

        void Error(object owner, object source, GameDataValidationFixDelegate fixDelegate, string message);

        void ErrorFormat(object owner, object source, GameDataValidationFixDelegate fixDelegate, string message,
            params object[] formatArguments);
    }
}