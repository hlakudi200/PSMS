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
        CreateMap<Announcement, AnnouncementDto>()
            .ForMember(dest => dest.ReadCount,
                opt => opt.MapFrom(src => src.Reads != null ? src.Reads.Count : 0));

        CreateMap<Announcement, AnnouncementListDto>()
            .ForMember(dest => dest.ReadCount,
                opt => opt.MapFrom(src => src.Reads != null ? src.Reads.Count : 0));
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
        CreateMap<Message, MessageDto>();
        CreateMap<Message, MessageListDto>();
    }

    private void CreateNotificationMappings()
    {
        CreateMap<Notification, NotificationDto>();
        CreateMap<Notification, NotificationListDto>();
    }
}
