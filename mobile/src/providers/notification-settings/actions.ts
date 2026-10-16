import { createAction } from "redux-actions";
import type {
  ICategoryPreference, IChannelConsent, IMyContact, INotificationSettingsStateContext,
} from "./context";

type SettingsPatch = Partial<INotificationSettingsStateContext>;

export enum NotificationSettingsActionEnums {
  loadPending = "NOTIFICATION_SETTINGS_LOAD_PENDING",
  loadSuccess = "NOTIFICATION_SETTINGS_LOAD_SUCCESS",
  loadError = "NOTIFICATION_SETTINGS_LOAD_ERROR",
  savePending = "NOTIFICATION_SETTINGS_SAVE_PENDING",
  saveSuccess = "NOTIFICATION_SETTINGS_SAVE_SUCCESS",
  saveError = "NOTIFICATION_SETTINGS_SAVE_ERROR",
  contactPending = "NOTIFICATION_SETTINGS_CONTACT_PENDING",
  contactSuccess = "NOTIFICATION_SETTINGS_CONTACT_SUCCESS",
  contactError = "NOTIFICATION_SETTINGS_CONTACT_ERROR",
}

export const loadPending = createAction<SettingsPatch>(NotificationSettingsActionEnums.loadPending, () => ({
  isPending: true, isSuccess: false, isError: false,
}));

export const loadSuccess = createAction<
  SettingsPatch,
  { consents: IChannelConsent[]; preferences: ICategoryPreference[]; contact: IMyContact }
>(NotificationSettingsActionEnums.loadSuccess, ({ consents, preferences, contact }) => ({
  isPending: false, isSuccess: true, isError: false, consents, preferences, contact,
}));

export const loadError = createAction<SettingsPatch>(NotificationSettingsActionEnums.loadError, () => ({
  isPending: false, isSuccess: false, isError: true,
}));

/** The toggle moves at once; saveError puts it back if the server says no. */
export const savePending = createAction<
  SettingsPatch,
  { savingKey: string; consents?: IChannelConsent[]; preferences?: ICategoryPreference[] }
>(NotificationSettingsActionEnums.savePending, ({ savingKey, consents, preferences }) => ({
  savingKey,
  saveErrorMessage: undefined,
  ...(consents && { consents }),
  ...(preferences && { preferences }),
}));

export const saveSuccess = createAction<SettingsPatch>(NotificationSettingsActionEnums.saveSuccess, () => ({
  savingKey: undefined,
}));

export const saveError = createAction<
  SettingsPatch,
  { message: string; consents?: IChannelConsent[]; preferences?: ICategoryPreference[] }
>(NotificationSettingsActionEnums.saveError, ({ message, consents, preferences }) => ({
  savingKey: undefined,
  saveErrorMessage: message,
  ...(consents && { consents }),
  ...(preferences && { preferences }),
}));

export const contactPending = createAction<SettingsPatch>(NotificationSettingsActionEnums.contactPending, () => ({
  isContactSaving: true, contactErrorMessage: undefined,
}));

export const contactSuccess = createAction<SettingsPatch, IMyContact>(
  NotificationSettingsActionEnums.contactSuccess,
  (contact) => ({ isContactSaving: false, contact })
);

export const contactError = createAction<SettingsPatch, string>(
  NotificationSettingsActionEnums.contactError,
  (message) => ({ isContactSaving: false, contactErrorMessage: message })
);
