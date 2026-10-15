import { useLocalSearchParams } from "expo-router";
import { SubjectPerformanceScreen } from "../../src/screens/parent/SubjectPerformanceScreen";

export default function SubjectPerformanceRoute() {
  const { subject, termId } = useLocalSearchParams<{ subject: string; termId: string }>();
  return <SubjectPerformanceScreen subjectName={subject} termId={termId} />;
}
