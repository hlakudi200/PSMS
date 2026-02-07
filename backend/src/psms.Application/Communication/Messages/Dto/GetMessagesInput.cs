using Abp.Application.Services.Dto;

namespace psms.Communication.Messages.Dto;

public class GetMessagesInput : PagedAndSortedResultRequestDto
{
    public bool? IsRead { get; set; }
    public string Search { get; set; }
}
