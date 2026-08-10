import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";
import type { PagedResult, PageQuery } from "@/types/common";
import {
  careServiceTypeOrdinal, shiftKindOrdinal, cancelActorOrdinal,
  type CareServiceType, type ShiftKind, type ShiftStatus, type CancelActor,
} from "@/types/enums";

export interface ShiftListItem {
  id: number;
  seniorId: number;
  agencyId: number;
  caregiverId?: number;
  serviceType: CareServiceType;
  shiftKind: ShiftKind;
  date: string;
  startTime: string;
  endTime: string;
  status: ShiftStatus;
  price: number;
}

export interface ShiftResult extends ShiftListItem {
  requestedByUserId: number;
  caregiverPayRate: number;
  checkInAt?: string;
  checkOutAt?: string;
  completedAt?: string;
  cancelReason?: string;
  familyNote?: string;
}

export interface RequestShiftInput {
  seniorId: number;
  agencyId: number;
  serviceType: CareServiceType;
  shiftKind: ShiftKind;
  date: string;
  startTime: string;
  endTime: string;
  familyNote?: string;
}

export interface ShiftFilter extends PageQuery {
  seniorId?: number;
  agencyId?: number;
  status?: ShiftStatus;
  from?: string;
  to?: string;
}

const keys = {
  list: (f: ShiftFilter) => ["shifts", f] as const,
  detail: (id: number) => ["shifts", id] as const,
};

export function useShifts(filter: ShiftFilter) {
  return useQuery({
    queryKey: keys.list(filter),
    queryFn: async () => (await apiClient.get<PagedResult<ShiftListItem>>("/Shifts", { params: filter })).data,
    enabled: !!(filter.seniorId || filter.agencyId),
  });
}

export function useShift(id: number) {
  return useQuery({
    queryKey: keys.detail(id),
    queryFn: async () => (await apiClient.get<ShiftResult>(`/Shifts/${id}`)).data,
    enabled: !!id,
  });
}

export function useRequestShift() {
  return useMutation({
    mutationFn: async (input: RequestShiftInput) =>
      (
        await apiClient.post<number>("/Shifts", {
          ...input,
          serviceType: careServiceTypeOrdinal[input.serviceType],
          shiftKind: shiftKindOrdinal[input.shiftKind],
        })
      ).data,
  });
}

function useShiftAction<TInput = void>(id: number, run: (input: TInput) => Promise<unknown>) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: run,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.detail(id) });
      qc.invalidateQueries({ queryKey: ["shifts"] });
    },
  });
}

export const useAssignCaregiver = (id: number) =>
  useShiftAction<{ caregiverId: number }>(id, (v) => apiClient.put(`/Shifts/${id}/Assign`, v));

export const useCheckInShift = (id: number) =>
  useShiftAction<{ latitude: number; longitude: number }>(id, (v) => apiClient.put(`/Shifts/${id}/CheckIn`, v));

export const useCheckOutShift = (id: number) =>
  useShiftAction(id, () => apiClient.put(`/Shifts/${id}/CheckOut`));

export const useCompleteShift = (id: number) =>
  useShiftAction(id, () => apiClient.put(`/Shifts/${id}/Complete`));

export const useCancelShift = (id: number) =>
  useShiftAction<{ cancelledBy: CancelActor; reason?: string }>(id, (v) =>
    apiClient.put(`/Shifts/${id}/Cancel`, { ...v, cancelledBy: cancelActorOrdinal[v.cancelledBy] })
  );

export const useMarkNoShow = (id: number) =>
  useShiftAction(id, () => apiClient.put(`/Shifts/${id}/NoShow`));
