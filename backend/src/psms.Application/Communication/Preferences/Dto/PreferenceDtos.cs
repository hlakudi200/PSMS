using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;

namespace psms.Communication.Preferences.Dto;

/// <summary>COMM-05: the current user's notification consents + category preferences.</summary>
public class MyPreferencesDto
{
    public List<ChannelConsentDto> Consents { get; set; } = new List<ChannelConsentDto>();
    public List<CategoryPreferenceDto> Preferences { get; set; } = new List<CategoryPreferenceDto>();
}

public class ChannelConsentDto
{
    public NotificationChannel Channel { get; set; }
    public bool IsGranted { get; set; }
    public DateTime? GrantedDate { get; set; }
    public DateTime? RevokedDate { get; set; }
}

public class CategoryPreferenceDto
{
    public NotificationChannel Channel { get; set; }
    public NotificationType Category { get; set; }
    public bool IsEnabled { get; set; }
}

public class SetConsentDto
{
    public NotificationChannel Channel { get; set; }
    public bool IsGranted { get; set; }
}

public class SetPreferenceDto
{
    public NotificationChannel Channel { get; set; }
    public NotificationType Category { get; set; }
    public bool IsEnabled { get; set; }
}
