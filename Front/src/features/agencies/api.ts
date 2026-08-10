import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";
import type { PagedResult, PageQuery } from "@/types/common";
import {
  careServiceTypeOrdinal, shiftKindOrdinal,
  type AgencyStatus, type CareServiceType, type ShiftKind,
} from "@/types/enums";

export interface AgencyListItem {
  id: number;
  name: string;
  city: string;
  status: AgencyStatus;
  ratingAvg: number;
  completedShifts: number;
}

export interface AgencyPriceItem {
  serviceType: CareServiceType;
  shiftKind: ShiftKind;
  price: number;
  caregiverPayRate: number;
}

export interface AgencyResult extends AgencyListItem {
  licenseNumber: string;
  licenseExpiryDate: string;
  district?: string;
  address?: string;
  phoneNumber: string;
  commissionPercent: number;
  prices: AgencyPriceItem[];
}

export interface AgencyBalanceResult {
  totalRevenue: number;
  totalCommission: number;
  totalPayouts: number;
  netBalance: number;
}

export interface AgencyFilter extends PageQuery {
  city?: string;
  status?: AgencyStatus;
}

const keys = {
  list: (f: AgencyFilter) => ["agencies", f] as const,
  detail: (id: number) => ["agencies", id] as const,
  balance: (id: number) => ["agencies", id, "balance"] as const,
};

export function useAgencies(filter: AgencyFilter) {
  return useQuery({
    queryKey: keys.list(filter),
    queryFn: async () => (await apiClient.get<PagedResult<AgencyListItem>>("/Agencies", { params: filter })).data,
  });
}

export function useAgency(id: number) {
  return useQuery({
    queryKey: keys.detail(id),
    queryFn: async () => (await apiClient.get<AgencyResult>(`/Agencies/${id}`)).data,
    enabled: !!id,
  });
}

export function useAgencyBalance(id: number) {
  return useQuery({
    queryKey: keys.balance(id),
    queryFn: async () => (await apiClient.get<AgencyBalanceResult>(`/Agencies/${id}/Balance`)).data,
    enabled: !!id,
  });
}

export function useRegisterAgency() {
  return useMutation({
    mutationFn: async (input: { name: string; licenseNumber: string; licenseExpiryDate: string; city: string; district?: string; address?: string; phoneNumber: string }) =>
      (await apiClient.post<number>("/Agencies", input)).data,
  });
}

function useAgencyAction<TInput = void>(id: number, run: (input: TInput) => Promise<unknown>) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: run,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.detail(id) });
      qc.invalidateQueries({ queryKey: ["agencies"] });
    },
  });
}

export const useApproveAgency = (id: number) => useAgencyAction(id, () => apiClient.put(`/Agencies/${id}/Approve`));
export const useRejectAgency = (id: number) =>
  useAgencyAction<{ reason: string }>(id, (v) => apiClient.put(`/Agencies/${id}/Reject`, v));
export const useSuspendAgency = (id: number) =>
  useAgencyAction<{ reason: string }>(id, (v) => apiClient.put(`/Agencies/${id}/Suspend`, v));
export const useSetAgencyPrice = (id: number) =>
  useAgencyAction<AgencyPriceItem>(id, (v) =>
    apiClient.post(`/Agencies/${id}/Prices`, {
      ...v,
      serviceType: careServiceTypeOrdinal[v.serviceType],
      shiftKind: shiftKindOrdinal[v.shiftKind],
    })
  );
