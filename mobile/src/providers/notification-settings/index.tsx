import { useCallback, useContext, useEffect, useMemo, useReducer, useRef } from "react";
import { getApiErrorMessage, getAxiosInstance } from "../../utils/axios-instance";
import type { ContactForm, ExternalChannel, NotificationCategory } from "../../utils/notification-labels";
import { NotificationSettingsReducer } from "./reducer";
import {
  INITIAL_STATE, NotificationSettingsActionContext, NotificationSettingsStateContext,
  type ICategoryPreference, type IChannelConsent, type IMyContact, type INotificationSettingsActionContext,
} from "./context";
import {
  contactError, contactPending, contactSuccess, loadError, loadPending, loadSuccess,
  saveError, savePending, saveSuccess,
} from "./actions";

const PREFERENCES = "/api/services/app/UserNotificationPreference";
const CONTACT = "/api/services/app/UserContact";
const IN_APP = 1;

const mapContact = (dto: any): IMyContact => ({
  email: dto.email || undefined,
  mobileNumber: dto.mobileNumber || undefined,
  whatsAppNumber: dto.whatsAppNumber || undefined,
  deviceCount: (dto.devices as any[] | undefined)?.length ?? 0,
});

export const NotificationSettingsProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(NotificationSettingsReducer, INITIAL_STATE);
  const instance = useRef(getAxiosInstance()).current;
  // The toggles update before the server answers, and put back what was there on a refusal.
  const stateRef = useRef(state);
  useEffect(() => { stateRef.current = state; }, [state]);

  const loadAsync = useCallback(async () => {
    dispatch(loadPending());
    try {
      // Sequential: the shared database pool is small.
      const { data: preferencesData } = await instance.get(`${PREFERENCES}/GetMyPreferences`);
      const { data: contactData } = await instance.get(`${CONTACT}/GetMyContact`);
      const result = preferencesData.result;
      dispatch(loadSuccess({
        consents: (result.consents as any[])
          .filter((c) => c.channel !== IN_APP)
          .map((c): IChannelConsent => ({
            channel: c.channel,
            isGranted: c.isGranted,
            grantedDate: c.grantedDate ?? undefined,
            revokedDate: c.revokedDate ?? undefined,
          })),
        preferences: (result.preferences as any[])
          .filter((p) => p.channel !== IN_APP)
          .map((p): ICategoryPreference => ({ channel: p.channel, category: p.category, isEnabled: p.isEnabled })),
        contact: mapContact(contactData.result),
      }));
    } catch {
      dispatch(loadError());
    }
  }, [instance]);

  const setConsentAsync = useCallback(async (channel: ExternalChannel, isGranted: boolean) => {
    const previous = stateRef.current.consents ?? [];
    const now = new Date().toISOString();
    const existing = previous.find((c) => c.channel === channel);
    const updated: IChannelConsent = {
      channel,
      isGranted,
      grantedDate: isGranted ? now : existing?.grantedDate,
      revokedDate: isGranted ? undefined : now,
    };
    dispatch(savePending({
      savingKey: `consent:${channel}`,
      consents: [...previous.filter((c) => c.channel !== channel), updated],
    }));
    try {
      await instance.post(`${PREFERENCES}/SetConsent`, { channel, isGranted });
      dispatch(saveSuccess());
    } catch (error) {
      dispatch(saveError({ message: getApiErrorMessage(error, "Couldn't save that change."), consents: previous }));
    }
  }, [instance]);

  const setPreferenceAsync = useCallback(async (
    channel: ExternalChannel, category: NotificationCategory, isEnabled: boolean
  ) => {
    const previous = stateRef.current.preferences ?? [];
    dispatch(savePending({
      savingKey: `preference:${channel}:${category}`,
      preferences: [
        ...previous.filter((p) => !(p.channel === channel && p.category === category)),
        { channel, category, isEnabled },
      ],
    }));
    try {
      await instance.post(`${PREFERENCES}/SetPreference`, { channel, category, isEnabled });
      dispatch(saveSuccess());
    } catch (error) {
      dispatch(saveError({ message: getApiErrorMessage(error, "Couldn't save that change."), preferences: previous }));
    }
  }, [instance]);

  const updateContactAsync = useCallback(async (contact: ContactForm) => {
    dispatch(contactPending());
    try {
      // ABP maps an Update* method to PUT.
      const { data } = await instance.put(`${CONTACT}/UpdateMyContact`, {
        mobileNumber: contact.mobileNumber || null,
        whatsAppNumber: contact.whatsAppNumber || null,
      });
      dispatch(contactSuccess(mapContact(data.result)));
      return true;
    } catch (error) {
      dispatch(contactError(getApiErrorMessage(error, "Couldn't save your contact details.")));
      return false;
    }
  }, [instance]);

  const actions = useMemo<INotificationSettingsActionContext>(
    () => ({ loadAsync, setConsentAsync, setPreferenceAsync, updateContactAsync }),
    [loadAsync, setConsentAsync, setPreferenceAsync, updateContactAsync]
  );

  return (
    <NotificationSettingsStateContext.Provider value={state}>
      <NotificationSettingsActionContext.Provider value={actions}>{children}</NotificationSettingsActionContext.Provider>
    </NotificationSettingsStateContext.Provider>
  );
};

export const useNotificationSettingsState = () => useContext(NotificationSettingsStateContext);
export const useNotificationSettingsActions = () => {
  const context = useContext(NotificationSettingsActionContext);
  if (!context) throw new Error("useNotificationSettingsActions must be used within a NotificationSettingsProvider");
  return context;
};
