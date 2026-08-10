import { useState } from "react";
import { Badge } from "@/components/ui";
import { Select } from "@/components/ui/Input";
import { DataTable } from "@/components/ui/Table";
import { useOrders, type OrderFilter } from "@/features/orders/api";
import { useMySeniors } from "@/features/seniors/api";
import { orderStatusLabels } from "@/types/enums";
import { extractErrorMessage } from "@/lib/apiClient";

export function OrdersListPage() {
  const { data: seniors } = useMySeniors();
  const [filter, setFilter] = useState<OrderFilter>({ page: 1, pageSize: 10 });
  const { data, isLoading, error } = useOrders(filter);

  return (
    <div>
      <h1 className="mb-5 text-xl font-bold text-gray-900">پرداخت‌ها</h1>
      <div className="mb-4">
        <Select
          placeholder="انتخاب سالمند"
          options={(seniors ?? []).map((s) => ({ value: String(s.id), label: s.fullName }))}
          value={filter.seniorId ?? ""}
          onChange={(e) => setFilter((f) => ({ ...f, page: 1, seniorId: e.target.value ? Number(e.target.value) : undefined }))}
        />
      </div>
      <DataTable
        isLoading={isLoading}
        error={error ? extractErrorMessage(error) : undefined}
        data={data}
        onPageChange={(page) => setFilter((f) => ({ ...f, page }))}
        columns={[
          { header: "شیفت", cell: (r) => `#${r.shiftId}` },
          { header: "مبلغ", cell: (r) => r.amount.toLocaleString("fa-IR") },
          { header: "وضعیت", cell: (r) => <Badge>{orderStatusLabels[r.status]}</Badge> },
          { header: "تاریخ", cell: (r) => new Date(r.createdAt).toLocaleDateString("fa-IR") },
        ]}
      />
      {!filter.seniorId && !isLoading && <p className="mt-4 text-sm text-gray-400">ابتدا یک سالمند انتخاب کنید</p>}
    </div>
  );
}
