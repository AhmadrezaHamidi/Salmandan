import { useState } from "react";
import { useParams } from "react-router-dom";
import { Button, Card, CardTitle, Badge } from "@/components/ui";
import { Input } from "@/components/ui/Input";
import { Spinner, ErrorState } from "@/components/ui/Spinner";
import { useCaregiver, useSubmitDocuments, useVerifyCaregiver, useRejectCaregiver } from "@/features/caregivers/api";
import { caregiverStatusLabels } from "@/types/enums";
import { extractErrorMessage } from "@/lib/apiClient";

export function CaregiverDetailPage() {
  const { id } = useParams();
  const caregiverId = Number(id);
  const { data: c, isLoading, error } = useCaregiver(caregiverId);
  const submitDocs = useSubmitDocuments(caregiverId);
  const verify = useVerifyCaregiver(caregiverId);
  const reject = useRejectCaregiver(caregiverId);
  const [reason, setReason] = useState("");

  if (isLoading) return <Spinner />;
  if (error) return <ErrorState message={extractErrorMessage(error)} />;
  if (!c) return null;

  return (
    <div className="mx-auto max-w-xl">
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-xl font-bold text-gray-900">{c.fullName}</h1>
        <Badge>{caregiverStatusLabels[c.status]}</Badge>
      </div>

      <Card className="mb-4 space-y-1 text-sm">
        <div>سطح: {c.level}</div>
        <div>تعداد عدم‌حضور: {c.noShowCount}</div>
        <div className="flex flex-wrap gap-2 pt-1">
          {c.skills.map((s) => <Badge key={s.skillType} color="blue">{s.skillType} ({s.yearsOfExperience} سال)</Badge>)}
        </div>
      </Card>

      {c.status === "Pending" && (
        <Card className="mb-4 space-y-3">
          <CardTitle>مدارک احراز صلاحیت</CardTitle>
          <div className="flex flex-wrap gap-3 text-sm">
            <label className="flex items-center gap-1">
              <input type="checkbox" defaultChecked={c.hasCriminalRecordCheck} id="crc" /> گواهی عدم سوءپیشینه
            </label>
          </div>
          <Button
            loading={submitDocs.isPending}
            onClick={() => {
              const crc = (document.getElementById("crc") as HTMLInputElement).checked;
              submitDocs.mutate({ hasCriminalRecordCheck: crc, hasCertificate: c.hasCertificate, hasLiabilityInsurance: c.hasLiabilityInsurance });
            }}
          >
            به‌روزرسانی مدارک
          </Button>
          <div className="flex flex-wrap gap-2 pt-2">
            <Button loading={verify.isPending} onClick={() => verify.mutate()}>تایید پرستار</Button>
            <Input placeholder="دلیل رد" value={reason} onChange={(e) => setReason(e.target.value)} />
            <Button variant="danger" loading={reject.isPending} onClick={() => reject.mutate({ reason })}>رد</Button>
          </div>
        </Card>
      )}
    </div>
  );
}
