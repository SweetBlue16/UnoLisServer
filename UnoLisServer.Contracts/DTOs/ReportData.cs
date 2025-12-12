namespace UnoLisServer.Contracts.DTOs
{
    /// <summary>
    /// Represents the data associated with a user report, including information about the reporter, the reported user,
    /// and the report description.
    /// </summary>
    public class ReportData
    {
        public string ReporterNickname { get; set; }
        public string ReportedNickname { get; set; }
        public string Description { get; set; }
    }
}
