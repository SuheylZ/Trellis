namespace Trellis.Communications
{
    /// <summary>
    /// Checks the status of communication
    /// </summary>
    public interface ICommunicationStatus
    {
        bool CommunicationIsWorking();
    }
}