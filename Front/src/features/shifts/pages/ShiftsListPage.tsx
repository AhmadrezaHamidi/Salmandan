import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Button, Badge } from "@/components/ui";
import { Select } from "@/components/ui/Input";
import { DataTable } from "@/components/ui/Table";
import { useShifts, type ShiftFilter } from "@/features/shifts/api";
import { useMySeniors } from "@/features/seniors/api";
import { shiftStatusLabels, shiftStatusColor, careServiceTypeLabels, type ShiftStatus } from "@/types/enums";
import { extractErrorMessage } from "@/lib/apiClient";

export function ShiftsListPage() {
  const { data: seniors } = useMySeniors();
  const [filter, setFilter] = useState<ShiftFilter>({ page: 1, pageSize: 10 });
  const { data, isLoading, error } = useShifts(filter);
  const navigate = useNavigate();

  return (
    <div>
      <div className="mb-5 flex items-center justify-between">
        <h1 className="text-xl font-bold text-gray-900">شیفت‌ها</h1>
        {filter.seniorId && <Button onClick={() => navigate(`/shifts/new?seniorId=${filter.seniorId}`)}>درخواست شیفت</Button>}
      </div>

      <div className="mb-4 flex flex-wrap gap-3">
        <Select
          placeholder="انتخاب سالمند"
          options={(seniors ?? []).map((s) => ({ value: String(s.id), label: s.fullName }))}
          value={filter.seniorId ?? ""}
          onChange={(e) => setFilter((f) => ({ ...f, page: 1, seniorId: e.target.value ? Number(e.target.value) : undefined }))}
        />
        <Select
          placeholder="همه وضعیت‌ها"
          options={Object.entries(shiftStatusLabels).map(([value, label]) => ({ value, label }))}
          value={filter.status ?? ""}
          onChange={(e) => setFilter((f) => ({ ...f, page: 1, status: (e.target.value || undefined) as ShiftStatus }))}
        />
      </div>

      <DataTable
        isLoading={isLoading}
        error={error ? extractErrorMessage(error) : undefined}
        data={data}
        onPageChange={(page) => setFilter((f) => ({ ...f, page }))}
        onRowClick={(row) => navigate(`/shifts/${row.id}`)}
        columns={[
          { header: "تاریخ", cell: (r) => r.date },
          { header: "ساعت", cell: (r) => `${r.startTime} - ${r.endTime}` },
          { header: "نوع خدمت", cell: (r) => careServiceTypeLabels[r.serviceType] },
          { header: "وضعیت", cell: (r) => <Badge color={shiftStatusColor[r.status]}>{shiftStatusLabels[r.status]}</Badge> },
          { header: "مبلغ", cell: (r) => r.price.toLocaleString("fa-IR") },
        ]}
      />

      {!filter.seniorId && !isLoading && (
        <p className="mt-4 text-sm text-gray-400">برای مشاهده شیفت‌ها، ابتدا یک سالمند انتخاب کنید</p>
      )}
    </div>
  );
}
