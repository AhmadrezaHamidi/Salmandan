import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Badge } from "@/components/ui";
import { Select } from "@/components/ui/Input";
import { DataTable } from "@/components/ui/Table";
import { useReportsBySenior } from "@/features/reports/api";
import { useMySeniors } from "@/features/seniors/api";
import { extractErrorMessage } from "@/lib/apiClient";

export function ReportsListPage() {
  const { data: seniors } = useMySeniors();
  const [seniorId, setSeniorId] = useState<number>();
  const [page, setPage] = useState(1);
  const { data, isLoading, error } = useReportsBySenior(seniorId ?? 0, { page, pageSize: 10 });

  return (
    <div>
      <h1 className="mb-5 text-xl font-bold text-gray-900">گزارش‌های مراقبت</h1>
      <div className="mb-4">
        <Select
          placeholder="انتخاب سالمند"
          options={(seniors ?? []).map((s) => ({ value: String(s.id), label: s.fullName }))}
          value={seniorId ?? ""}
          onChange={(e) => { setSeniorId(e.target.value ? Number(e.target.value) : undefined); setPage(1); }}
        />
      </div>
      {seniorId && (
        <DataTableWrapper seniorId={seniorId} page={page} setPage={setPage} data={data} isLoading={isLoading} error={error} />
      )}
    </div>
  );
}

function DataTableWrapper({
  seniorId, setPage, data, isLoading, error,
}: { seniorId: number; page: number; setPage: (p: number) => void; data: ReturnType<typeof useReportsBySenior>["data"]; isLoading: boolean; error: unknown }) {
  const navigate = useNavigate();
  return (
    <DataTable
      isLoading={isLoading}
      error={error ? extractErrorMessage(error) : undefined}
      data={data}
      onPageChange={setPage}
      onRowClick={(row) => navigate(`/reports/${row.id}?seniorId=${seniorId}`)}
      columns={[
        { header: "تاریخ ثبت", cell: (r) => new Date(r.submittedAt).toLocaleDateString("fa-IR") },
        { header: "امتیاز ریسک", cell: (r) => <Badge color={r.riskScore >= 50 ? "red" : r.riskScore >= 25 ? "amber" : "green"}>{r.riskScore}</Badge> },
        { header: "تایید خانواده", cell: (r) => (r.isApprovedByFamily ? <Badge color="green">تاییدشده</Badge> : <Badge>در انتظار</Badge>) },
      ]}
    />
  );
}
