import { createContext } from "react";
import type { ContactForm, ExternalChannel, NotificationCategory } from "../../utils/notification-labels";

export interface IChannelConsent {
  channel: ExternalChannel;
  isGranted: boolean;
  grantedDate?: string;
  revokedDate?: string;
}

/** Only opt-outs are interesting: a category with no row is sent. */
export interface ICategoryPreference {
  channel: ExternalChannel;
  category: NotificationCategory;
  isEnabled: boolean;
}

export interface IMyContact {
  email?: string;
  mobileNumber?: string;
  whatsAppNumber?: string;
  /** Phones registered for push. None means a push has nowhere to go. */
  deviceCount: number;
}

export interface INotificationSettingsStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  consents?: IChannelConsent[];
  preferences?: ICategoryPreference[];
  contact?: IMyContact;
  /** Which toggle is saving, e.g. "consent:3" or "preference:3:4". */
  savingKey?: string;
  saveErrorMessage?: string;
  isContactSaving: boolean;
  contactErrorMessage?: string;
}

export interface INotificationSettingsActionContext {
  /** Loads the consents, the category opt-outs and the contact details. */
  loadAsync: () => Promise<void>;
  /** POPIA consent for one channel: granted, or withdrawn. */
  setConsentAsync: (channel: ExternalChannel, isGranted: boolean) => Promise<void>;
  setPreferenceAsync: (channel: ExternalChannel, category: NotificationCategory, isEnabled: boolean) => Promise<void>;
  /** Resolves true once saved, so the form knows it can show the saved state. */
  updateContactAsync: (contact: ContactForm) => Promise<boolean>;
}

export const INITIAL_STATE: INotificationSettingsStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
  isContactSaving: false,
};

export const NotificationSettingsStateContext = createContext<INotificationSettingsStateContext>(INITIAL_STATE);
export const NotificationSettingsActionContext = createContext<INotificationSettingsActionContext | undefined>(undefined);
