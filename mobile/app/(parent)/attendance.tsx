import { AttendanceProvider } from "../../src/providers/attendance";
import { ChildAttendanceScreen } from "../../src/screens/parent/ChildAttendanceScreen";

export default function AttendanceRoute() {
  return (
    <AttendanceProvider>
      <ChildAttendanceScreen />
    </AttendanceProvider>
  );
}
