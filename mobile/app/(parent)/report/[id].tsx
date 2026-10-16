import { useLocalSearchParams } from "expo-router";
import { ReportCardDetailScreen } from "../../../src/screens/parent/ReportCardDetailScreen";

export default function ReportCardDetailRoute() {
  const { id } = useLocalSearchParams<{ id: string }>();
  return <ReportCardDetailScreen reportId={id} />;
}
