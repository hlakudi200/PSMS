import { useLocalSearchParams } from "expo-router";
import { AnnouncementDetailScreen } from "../../../src/screens/parent/AnnouncementDetailScreen";

export default function AnnouncementDetailRoute() {
  const { id } = useLocalSearchParams<{ id: string }>();
  return <AnnouncementDetailScreen announcementId={id} />;
}
