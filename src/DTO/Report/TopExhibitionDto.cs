namespace artgallery_server.DTO.Report
{
    public class TopExhibitionDto
    {
        public required string ExhibitionName { get; set; }
        public int TicketCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
