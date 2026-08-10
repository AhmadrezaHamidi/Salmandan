import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";
import type { PagedResult, PageQuery } from "@/types/common";
import type { OrderStatus } from "@/types/enums";

export interface OrderListItem {
  id: number;
  shiftId: number;
  seniorId: number;
  agencyId: number;
  amount: number;
  status: OrderStatus;
  createdAt: string;
}

export interface OrderResult extends OrderListItem {
  platformFee: number;
  agencyShare: number;
  isInvoiceIssued: boolean;
  invoiceNumber?: string;
  escrowedAt?: string;
  releasedAt?: string;
  refundedAt?: string;
}

export interface CreateOrderResponse {
  orderId: number;
  amount: number;
  paymentUrl: string;
  authority: string;
}

export interface OrderFilter extends PageQuery {
  seniorId?: number;
  agencyId?: number;
  status?: OrderStatus;
}

const keys = {
  list: (f: OrderFilter) => ["orders", f] as const,
  detail: (id: number) => ["orders", id] as const,
};

export function useOrders(filter: OrderFilter) {
  return useQuery({
    queryKey: keys.list(filter),
    queryFn: async () => (await apiClient.get<PagedResult<OrderListItem>>("/Orders", { params: filter })).data,
    enabled: !!(filter.seniorId || filter.agencyId),
  });
}

export function useOrder(id: number) {
  return useQuery({
    queryKey: keys.detail(id),
    queryFn: async () => (await apiClient.get<OrderResult>(`/Orders/${id}`)).data,
    enabled: !!id,
  });
}

export function useCreateOrder() {
  return useMutation({
    mutationFn: async (shiftId: number) => (await apiClient.post<CreateOrderResponse>("/Orders", { shiftId })).data,
  });
}

function useOrderAction<TInput = void>(id: number, run: (input: TInput) => Promise<unknown>) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: run,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.detail(id) });
      qc.invalidateQueries({ queryKey: ["orders"] });
    },
  });
}

export const useReleaseOrder = (id: number) => useOrderAction(id, () => apiClient.put(`/Orders/${id}/Release`));
export const useRefundOrder = (id: number) =>
  useOrderAction<{ reason: string }>(id, (v) => apiClient.put(`/Orders/${id}/Refund`, v));
export const useIssueInvoice = (id: number) => useOrderAction(id, () => apiClient.post(`/Orders/${id}/Invoice`));
