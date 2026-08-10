import { useParams, useSearchParams } from "react-router-dom";
import { Button, Card, CardTitle, Badge } from "@/components/ui";
import { Spinner, ErrorState } from "@/components/ui/Spinner";
import { useShiftReport, useApproveReport } from "@/features/reports/api";
import { useAuthStore } from "@/store/authStore";
import { extractErrorMessage } from "@/lib/apiClient";

const scaleColor = { Poor: "red", Fair: "amber", Good: "green" } as const;

export function ReportDetailPage() {
  const { id } = useParams();
  const [params] = useSearchParams();
  const seniorId = Number(params.get("seniorId"));
  const reportId = Number(id);
  const role = useAuthStore((s) => s.session?.role);
  const { data: r, isLoading, error } = useShiftReport(reportId);
  const approve = useApproveReport(reportId, seniorId);

  if (isLoading) return <Spinner />;
  if (error) return <ErrorState message={extractErrorMessage(error)} />;
  if (!r) return null;

  return (
    <div className="mx-auto max-w-xl">
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-xl font-bold text-gray-900">گزارش شیفت #{r.shiftId}</h1>
        <Badge color={r.riskScore >= 50 ? "red" : r.riskScore >= 25 ? "amber" : "green"}>ریسک: {r.riskScore}</Badge>
      </div>

      <Card className="mb-4 grid grid-cols-2 gap-3 text-sm">
        <div>خلق‌وخو: <Badge color={scaleColor[r.mood]}>{r.mood}</Badge></div>
        <div>اشتها: <Badge color={scaleColor[r.appetite]}>{r.appetite}</Badge></div>
        <div>کیفیت خواب: <Badge color={scaleColor[r.sleepQuality]}>{r.sleepQuality}</Badge></div>
        <div>تحرک: {r.mobility}</div>
        <div>سطح درد: {r.painLevel}/10</div>
        {r.pulseRate && <div>ضربان قلب: {r.pulseRate}</div>}
        {r.oxygenSaturation && <div>اکسیژن خون: {r.oxygenSaturation}%</div>}
        {r.temperature && <div>دما: {r.temperature}°C</div>}
      </Card>

      {r.hasIncident && (
        <Card className="mb-4 border-red-200 bg-red-50">
          <CardTitle className="text-red-700">حادثه ثبت‌شده: {r.incidentKind}</CardTitle>
          <p className="text-sm text-red-700">{r.incidentDescription}</p>
        </Card>
      )}

      <Card className="mb-4">
        <CardTitle>مصرف داروها</CardTitle>
        <ul className="space-y-1 text-sm">
          {r.medicationIntakes.map((m) => (
            <li key={m.medicationId}>
              {m.name} — {m.wasTaken ? <Badge color="green">مصرف‌شده</Badge> : <Badge color="red">مصرف‌نشده</Badge>}
              {m.reason ? ` (${m.reason})` : ""}
            </li>
          ))}
        </ul>
      </Card>

      {role === "Family" && !r.isApprovedByFamily && (
        <Button loading={approve.isPending} onClick={() => approve.mutate()}>تایید گزارش</Button>
      )}
      {r.isApprovedByFamily && <Badge color="green">این گزارش توسط خانواده تایید شده است</Badge>}
    </div>
  );
}
