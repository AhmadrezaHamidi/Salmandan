import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Badge } from "@/components/ui";
import { Select } from "@/components/ui/Input";
import { DataTable } from "@/components/ui/Table";
import { useAgencies, type AgencyFilter } from "@/features/agencies/api";
import { agencyStatusLabels } from "@/types/enums";
import { extractErrorMessage } from "@/lib/apiClient";

export function AgenciesListPage() {
  const [filter, setFilter] = useState<AgencyFilter>({ page: 1, pageSize: 10 });
  const { data, isLoading, error } = useAgencies(filter);
  const navigate = useNavigate();

  return (
    <div>
      <h1 className="mb-5 text-xl font-bold text-gray-900">آژانس‌ها</h1>
      <div className="mb-4">
        <Select
          placeholder="همه وضعیت‌ها"
          options={Object.entries(agencyStatusLabels).map(([value, label]) => ({ value, label }))}
          value={filter.status ?? ""}
          onChange={(e) => setFilter((f) => ({ ...f, page: 1, status: (e.target.value || undefined) as never }))}
        />
      </div>
      <DataTable
        isLoading={isLoading}
        error={error ? extractErrorMessage(error) : undefined}
        data={data}
        onPageChange={(page) => setFilter((f) => ({ ...f, page }))}
        onRowClick={(row) => navigate(`/agencies/${row.id}`)}
        columns={[
          { header: "نام", cell: (r) => r.name },
          { header: "شهر", cell: (r) => r.city },
          { header: "وضعیت", cell: (r) => <Badge>{agencyStatusLabels[r.status]}</Badge> },
          { header: "امتیاز", cell: (r) => r.ratingAvg.toFixed(1) },
          { header: "شیفت‌های تکمیل‌شده", cell: (r) => r.completedShifts },
        ]}
      />
    </div>
  );
}
