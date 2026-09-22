import { useLocalSearchParams } from "expo-router";
import { AssessmentDetailScreen } from "../../../src/screens/student/AssessmentDetailScreen";

export default function AssessmentDetailRoute() {
  const { id } = useLocalSearchParams<{ id: string }>();
  return <AssessmentDetailScreen assessmentId={id} />;
}
