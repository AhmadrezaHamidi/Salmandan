import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";
import type { PagedResult, PageQuery } from "@/types/common";
import {
  genderOrdinal, caregiverLevelOrdinal, caregiverSkillTypeOrdinal,
  type CaregiverLevel, type CaregiverStatus, type CaregiverSkillType, type Gender,
} from "@/types/enums";

export interface CaregiverListItem {
  id: number;
  fullName: string;
  agencyId: number;
  level: CaregiverLevel;
  status: CaregiverStatus;
  ratingAvg: number;
  completedShifts: number;
}

export interface CaregiverSkill {
  skillType: CaregiverSkillType;
  yearsOfExperience: number;
}

export interface CaregiverResult extends CaregiverListItem {
  nationalId: string;
  gender: Gender;
  birthYear: number;
  hasCriminalRecordCheck: boolean;
  hasCertificate: boolean;
  hasLiabilityInsurance: boolean;
  noShowCount: number;
  skills: CaregiverSkill[];
}

export interface CaregiverFilter extends PageQuery {
  agencyId?: number;
  status?: CaregiverStatus;
}

const keys = {
  list: (f: CaregiverFilter) => ["caregivers", f] as const,
  detail: (id: number) => ["caregivers", id] as const,
};

export function useCaregivers(filter: CaregiverFilter) {
  return useQuery({
    queryKey: keys.list(filter),
    queryFn: async () => (await apiClient.get<PagedResult<CaregiverListItem>>("/Caregivers", { params: filter })).data,
  });
}

export function useCaregiver(id: number) {
  return useQuery({
    queryKey: keys.detail(id),
    queryFn: async () => (await apiClient.get<CaregiverResult>(`/Caregivers/${id}`)).data,
    enabled: !!id,
  });
}

export function useRegisterCaregiver(agencyId: number) {
  return useMutation({
    mutationFn: async (input: { fullName: string; nationalId: string; gender: Gender; birthYear: number; level: CaregiverLevel }) =>
      (
        await apiClient.post<number>(`/Caregivers/Agency/${agencyId}`, {
          ...input,
          gender: genderOrdinal[input.gender],
          level: caregiverLevelOrdinal[input.level],
        })
      ).data,
  });
}

function useCaregiverAction<TInput = void>(id: number, run: (input: TInput) => Promise<unknown>) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: run,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.detail(id) });
      qc.invalidateQueries({ queryKey: ["caregivers"] });
    },
  });
}

export const useSubmitDocuments = (id: number) =>
  useCaregiverAction<{ hasCriminalRecordCheck: boolean; hasCertificate: boolean; hasLiabilityInsurance: boolean }>(
    id, (v) => apiClient.put(`/Caregivers/${id}/Documents`, v)
  );
export const useVerifyCaregiver = (id: number) => useCaregiverAction(id, () => apiClient.put(`/Caregivers/${id}/Verify`));
export const useRejectCaregiver = (id: number) =>
  useCaregiverAction<{ reason: string }>(id, (v) => apiClient.put(`/Caregivers/${id}/Reject`, v));
export const useSuspendCaregiver = (id: number) =>
  useCaregiverAction<{ reason: string }>(id, (v) => apiClient.put(`/Caregivers/${id}/Suspend`, v));
export const useAddSkill = (id: number) =>
  useCaregiverAction<CaregiverSkill>(id, (v) =>
    apiClient.post(`/Caregivers/${id}/Skills`, { ...v, skillType: caregiverSkillTypeOrdinal[v.skillType] })
  );
