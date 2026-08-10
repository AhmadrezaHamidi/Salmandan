import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";
import {
  genderOrdinal, dependencyLevelOrdinal, familyRelationOrdinal,
  familyAccessLevelOrdinal, medicationFrequencyOrdinal,
  type DependencyLevel, type Gender, type FamilyRelation, type FamilyAccessLevel, type MedicationFrequency,
} from "@/types/enums";

export interface SeniorListItem {
  id: number;
  fullName: string;
  city: string;
  district?: string;
  dependencyLevel: DependencyLevel;
  age: number;
}

export interface MedicalCondition {
  id: number;
  title: string;
  description?: string;
  diagnosedAt?: string;
  isChronic: boolean;
}

export interface Medication {
  id: number;
  name: string;
  dosage: string;
  frequency: MedicationFrequency;
  instruction?: string;
  startDate: string;
  endDate?: string;
  isActive: boolean;
}

export interface Allergy {
  id: number;
  title: string;
  reaction?: string;
}

export interface EmergencyContact {
  id: number;
  fullName: string;
  phoneNumber: string;
  relation: FamilyRelation;
  priority: number;
}

export interface FamilyMemberItem {
  userId: number;
  displayName: string;
  relation: FamilyRelation;
  accessLevel: FamilyAccessLevel;
  joinedAt: string;
}

export interface SeniorResult extends SeniorListItem {
  nationalId?: string;
  birthDate: string;
  gender: Gender;
  address: string;
  notes?: string;
  isActive: boolean;
  conditions: MedicalCondition[];
  medications: Medication[];
  allergies: Allergy[];
  emergencyContacts: EmergencyContact[];
  familyMembers: FamilyMemberItem[];
}

export interface CreateSeniorInput {
  fullName: string;
  nationalId?: string;
  birthDate: string;
  gender: Gender;
  dependencyLevel: DependencyLevel;
  city: string;
  district?: string;
  address: string;
  latitude?: number;
  longitude?: number;
  notes?: string;
}

const keys = {
  mine: ["seniors", "mine"] as const,
  detail: (id: number) => ["seniors", id] as const,
};

export function useMySeniors() {
  return useQuery({
    queryKey: keys.mine,
    queryFn: async () => (await apiClient.get<SeniorListItem[]>("/Seniors/Mine")).data,
  });
}

export function useSenior(id: number) {
  return useQuery({
    queryKey: keys.detail(id),
    queryFn: async () => (await apiClient.get<SeniorResult>(`/Seniors/${id}`)).data,
    enabled: !!id,
  });
}

export function useCreateSenior() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (input: CreateSeniorInput) =>
      (
        await apiClient.post<number>("/Seniors", {
          ...input,
          gender: genderOrdinal[input.gender],
          dependencyLevel: dependencyLevelOrdinal[input.dependencyLevel],
        })
      ).data,
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.mine }),
  });
}

function useSeniorMutation<TInput>(seniorId: number, run: (input: TInput) => Promise<unknown>) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: run,
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.detail(seniorId) }),
  });
}

export function useAddCondition(seniorId: number) {
  return useSeniorMutation(seniorId, (input: Omit<MedicalCondition, "id">) =>
    apiClient.post(`/Seniors/${seniorId}/Conditions`, input)
  );
}

export function useAddMedication(seniorId: number) {
  return useSeniorMutation(seniorId, (input: Omit<Medication, "id" | "isActive">) =>
    apiClient.post(`/Seniors/${seniorId}/Medications`, {
      ...input,
      frequency: medicationFrequencyOrdinal[input.frequency],
    })
  );
}

export function useAddAllergy(seniorId: number) {
  return useSeniorMutation(seniorId, (input: Omit<Allergy, "id">) =>
    apiClient.post(`/Seniors/${seniorId}/Allergies`, input)
  );
}

export function useAddEmergencyContact(seniorId: number) {
  return useSeniorMutation(seniorId, (input: Omit<EmergencyContact, "id">) =>
    apiClient.post(`/Seniors/${seniorId}/EmergencyContacts`, {
      ...input,
      relation: familyRelationOrdinal[input.relation],
    })
  );
}

export function useAddFamilyMember(seniorId: number) {
  return useSeniorMutation(
    seniorId,
    (input: { phoneNumber: string; relation: FamilyRelation; accessLevel: FamilyAccessLevel }) =>
      apiClient.post(`/Seniors/${seniorId}/Family`, {
        ...input,
        relation: familyRelationOrdinal[input.relation],
        accessLevel: familyAccessLevelOrdinal[input.accessLevel],
      })
  );
}

export function useSetDependencyLevel(seniorId: number) {
  return useSeniorMutation(seniorId, (dependencyLevel: DependencyLevel) =>
    apiClient.put(`/Seniors/${seniorId}/DependencyLevel`, { dependencyLevel: dependencyLevelOrdinal[dependencyLevel] })
  );
}
