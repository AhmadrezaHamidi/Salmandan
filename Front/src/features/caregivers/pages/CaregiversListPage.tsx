import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Badge } from "@/components/ui";
import { Select } from "@/components/ui/Input";
import { DataTable } from "@/components/ui/Table";
import { useCaregivers, type CaregiverFilter } from "@/features/caregivers/api";
import { caregiverStatusLabels } from "@/types/enums";
import { extractErrorMessage } from "@/lib/apiClient";

export function CaregiversListPage() {
  const [filter, setFilter] = useState<CaregiverFilter>({ page: 1, pageSize: 10 });
  const { data, isLoading, error } = useCaregivers(filter);
  const navigate = useNavigate();

  return (
    <div>
      <h1 className="mb-5 text-xl font-bold text-gray-900">پرستاران</h1>
      <div className="mb-4">
        <Select
          placeholder="همه وضعیت‌ها"
          options={Object.entries(caregiverStatusLabels).map(([value, label]) => ({ value, label }))}
          value={filter.status ?? ""}
          onChange={(e) => setFilter((f) => ({ ...f, page: 1, status: (e.target.value || undefined) as never }))}
        />
      </div>
      <DataTable
        isLoading={isLoading}
        error={error ? extractErrorMessage(error) : undefined}
        data={data}
        onPageChange={(page) => setFilter((f) => ({ ...f, page }))}
        onRowClick={(row) => navigate(`/caregivers/${row.id}`)}
        columns={[
          { header: "نام", cell: (r) => r.fullName },
          { header: "سطح", cell: (r) => r.level },
          { header: "وضعیت", cell: (r) => <Badge>{caregiverStatusLabels[r.status]}</Badge> },
          { header: "امتیاز", cell: (r) => r.ratingAvg.toFixed(1) },
        ]}
      />
    </div>
  );
}
