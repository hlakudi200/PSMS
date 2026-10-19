import { NotificationSettingsProvider } from "../../src/providers/notification-settings";
import { NotificationSettingsScreen } from "../../src/screens/parent/NotificationSettingsScreen";

export default function NotificationSettingsRoute() {
  return (
    <NotificationSettingsProvider>
      <NotificationSettingsScreen />
    </NotificationSettingsProvider>
  );
}
