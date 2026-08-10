import { useState } from "react";
import { useParams } from "react-router-dom";
import { Button, Card, CardTitle, Badge } from "@/components/ui";
import { Input } from "@/components/ui/Input";
import { Spinner, ErrorState } from "@/components/ui/Spinner";
import { useAgency, useAgencyBalance, useApproveAgency, useRejectAgency, useSuspendAgency } from "@/features/agencies/api";
import { useAuthStore } from "@/store/authStore";
import { agencyStatusLabels, careServiceTypeLabels, shiftKindLabels } from "@/types/enums";
import { extractErrorMessage } from "@/lib/apiClient";

export function AgencyDetailPage() {
  const { id } = useParams();
  const agencyId = Number(id);
  const role = useAuthStore((s) => s.session?.role);
  const { data: agency, isLoading, error } = useAgency(agencyId);
  const { data: balance } = useAgencyBalance(agencyId);
  const approve = useApproveAgency(agencyId);
  const reject = useRejectAgency(agencyId);
  const suspend = useSuspendAgency(agencyId);
  const [reason, setReason] = useState("");

  if (isLoading) return <Spinner />;
  if (error) return <ErrorState message={extractErrorMessage(error)} />;
  if (!agency) return null;

  return (
    <div className="mx-auto max-w-2xl">
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-xl font-bold text-gray-900">{agency.name}</h1>
        <Badge>{agencyStatusLabels[agency.status]}</Badge>
      </div>

      <Card className="mb-4 space-y-1 text-sm">
        <div>شهر: {agency.city} {agency.district ? `— ${agency.district}` : ""}</div>
        <div>شماره مجوز: {agency.licenseNumber}</div>
        <div>انقضای مجوز: {agency.licenseExpiryDate}</div>
        <div>درصد کمیسیون: {agency.commissionPercent}%</div>
        {balance && <div>موجودی خالص: {balance.netBalance.toLocaleString("fa-IR")} تومان</div>}
      </Card>

      <Card className="mb-4">
        <CardTitle>تعرفه‌ها</CardTitle>
        <ul className="space-y-1 text-sm">
          {agency.prices.map((p, i) => (
            <li key={i}>
              {careServiceTypeLabels[p.serviceType]} — {shiftKindLabels[p.shiftKind]}: {p.price.toLocaleString("fa-IR")} تومان
            </li>
          ))}
        </ul>
      </Card>

      {role === "Admin" && agency.status === "Pending" && (
        <Card className="space-y-3">
          <CardTitle>بررسی درخواست آژانس</CardTitle>
          <Button loading={approve.isPending} onClick={() => approve.mutate()}>تایید آژانس</Button>
          <div className="flex gap-2">
            <Input placeholder="دلیل رد" value={reason} onChange={(e) => setReason(e.target.value)} />
            <Button variant="danger" loading={reject.isPending} onClick={() => reject.mutate({ reason })}>رد درخواست</Button>
          </div>
        </Card>
      )}

      {role === "Admin" && agency.status === "Active" && (
        <Card className="flex items-center gap-2">
          <Input placeholder="دلیل تعلیق" value={reason} onChange={(e) => setReason(e.target.value)} />
          <Button variant="danger" loading={suspend.isPending} onClick={() => suspend.mutate({ reason })}>تعلیق آژانس</Button>
        </Card>
      )}
    </div>
  );
}
