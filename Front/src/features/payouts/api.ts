import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";
import type { PagedResult, PageQuery } from "@/types/common";
import type { PayoutStatus } from "@/types/enums";

export interface PayoutListItem {
  id: number;
  agencyId: number;
  caregiverId: number;
  periodFrom: string;
  periodTo: string;
  shiftCount: number;
  totalHours: number;
  totalAmount: number;
  status: PayoutStatus;
}

export interface PayoutResult extends PayoutListItem {
  paidAt?: string;
  paymentReference?: string;
}

export interface GeneratePayrollResponse {
  payoutCount: number;
  shiftCount: number;
  totalAmount: number;
  payoutIds: number[];
}

export interface PayoutFilter extends PageQuery {
  agencyId?: number;
  caregiverId?: number;
  status?: PayoutStatus;
}

const keys = {
  list: (f: PayoutFilter) => ["payouts", f] as const,
  detail: (id: number) => ["payouts", id] as const,
};

export function usePayouts(filter: PayoutFilter) {
  return useQuery({
    queryKey: keys.list(filter),
    queryFn: async () => (await apiClient.get<PagedResult<PayoutListItem>>("/Payouts", { params: filter })).data,
    enabled: !!(filter.agencyId || filter.caregiverId),
  });
}

export function useGeneratePayroll() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (input: { agencyId: number; from: string; to: string }) =>
      (await apiClient.post<GeneratePayrollResponse>("/Payouts/Generate", input)).data,
    onSuccess: () => qc.invalidateQueries({ queryKey: ["payouts"] }),
  });
}

export function useMarkPayoutPaid(id: number) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (paymentReference: string) => apiClient.put(`/Payouts/${id}/Pay`, { paymentReference }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["payouts"] }),
  });
}
