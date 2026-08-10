import { useState } from "react";
import { Badge, Button, Card, CardTitle } from "@/components/ui";
import { Input } from "@/components/ui/Input";
import { DataTable } from "@/components/ui/Table";
import { usePayouts, useGeneratePayroll, type PayoutFilter } from "@/features/payouts/api";
import { payoutStatusLabels } from "@/types/enums";
import { extractErrorMessage } from "@/lib/apiClient";

export function PayoutsListPage() {
  const [filter, setFilter] = useState<PayoutFilter>({ page: 1, pageSize: 10 });
  const [agencyIdInput, setAgencyIdInput] = useState("");
  const [range, setRange] = useState({ from: "", to: "" });
  const { data, isLoading, error } = usePayouts(filter);
  const generate = useGeneratePayroll();

  return (
    <div>
      <h1 className="mb-5 text-xl font-bold text-gray-900">تسویه حقوق پرستاران</h1>

      <Card className="mb-4">
        <CardTitle>تولید تسویه حقوق دوره‌ای</CardTitle>
        <div className="flex flex-wrap items-end gap-2">
          <Input label="شناسه آژانس" value={agencyIdInput} onChange={(e) => setAgencyIdInput(e.target.value)} />
          <Input label="از تاریخ" type="date" value={range.from} onChange={(e) => setRange((r) => ({ ...r, from: e.target.value }))} />
          <Input label="تا تاریخ" type="date" value={range.to} onChange={(e) => setRange((r) => ({ ...r, to: e.target.value }))} />
          <Button
            loading={generate.isPending}
            onClick={() => {
              const agencyId = Number(agencyIdInput);
              generate.mutate({ agencyId, ...range });
              setFilter((f) => ({ ...f, agencyId }));
            }}
          >
            تولید
          </Button>
        </div>
        {generate.isSuccess && (
          <p className="mt-2 text-sm text-green-600">
            {generate.data.payoutCount} فیش حقوقی برای {generate.data.shiftCount} شیفت به مبلغ{" "}
            {generate.data.totalAmount.toLocaleString("fa-IR")} تومان تولید شد
          </p>
        )}
        {generate.isError && <p className="mt-2 text-sm text-red-600">{extractErrorMessage(generate.error)}</p>}
      </Card>

      <DataTable
        isLoading={isLoading}
        error={error ? extractErrorMessage(error) : undefined}
        data={data}
        onPageChange={(page) => setFilter((f) => ({ ...f, page }))}
        columns={[
          { header: "پرستار", cell: (r) => `#${r.caregiverId}` },
          { header: "دوره", cell: (r) => `${r.periodFrom} تا ${r.periodTo}` },
          { header: "تعداد شیفت", cell: (r) => r.shiftCount },
          { header: "مبلغ کل", cell: (r) => r.totalAmount.toLocaleString("fa-IR") },
          { header: "وضعیت", cell: (r) => <Badge>{payoutStatusLabels[r.status]}</Badge> },
        ]}
      />
      {!filter.agencyId && !isLoading && <p className="mt-4 text-sm text-gray-400">ابتدا تسویه‌ای تولید یا شناسه آژانس را وارد کنید</p>}
    </div>
  );
}
