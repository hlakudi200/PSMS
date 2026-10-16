using AutoMapper;
using psms.Communication.Announcements.Dto;
using psms.Communication.AnnouncementReads.Dto;
using psms.Communication.Documents.Dto;
using psms.Communication.Messages.Dto;
using psms.Communication.Notifications.Dto;
using psms.Domain.Communication.Entities;

namespace psms.Communication.Shared;

public class CommunicationMapper : Profile
{
    public CommunicationMapper()
    {
        CreateAnnouncementMappings();
        CreateAnnouncementReadMappings();
        CreateDocumentMappings();
        CreateMessageMappings();
        CreateNotificationMappings();
    }

    private void CreateAnnouncementMappings()
    {
        // IsReadByMe depends on who is asking, so the service sets it.
        CreateMap<Announcement, AnnouncementDto>()
            .ForMember(dest => dest.ReadCount,
                opt => opt.MapFrom(src => src.Reads != null ? src.Reads.Count : 0))
            .ForMember(dest => dest.IsReadByMe, opt => opt.Ignore());

        CreateMap<Announcement, AnnouncementListDto>()
            .ForMember(dest => dest.ReadCount,
                opt => opt.MapFrom(src => src.Reads != null ? src.Reads.Count : 0))
            .ForMember(dest => dest.IsReadByMe, opt => opt.Ignore());
    }

    private void CreateAnnouncementReadMappings()
    {
        CreateMap<AnnouncementRead, AnnouncementReadDto>();
    }

    private void CreateDocumentMappings()
    {
        CreateMap<Document, DocumentDto>();
        CreateMap<Document, DocumentListDto>();
    }

    private void CreateMessageMappings()
    {
        // The names are users, not message fields: the service fills them in.
        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.SenderName, opt => opt.Ignore())
            .ForMember(dest => dest.RecipientName, opt => opt.Ignore());
        CreateMap<Message, MessageListDto>()
            .ForMember(dest => dest.SenderName, opt => opt.Ignore())
            .ForMember(dest => dest.RecipientName, opt => opt.Ignore());
    }

    private void CreateNotificationMappings()
    {
        CreateMap<Notification, NotificationDto>();
        CreateMap<Notification, NotificationListDto>();
    }
}
