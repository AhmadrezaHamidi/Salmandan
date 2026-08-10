import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";
import type { PagedResult, PageQuery } from "@/types/common";
import type { ObservationScale, MobilityLevel, IncidentType } from "@/types/enums";

export interface ShiftReportListItem {
  id: number;
  shiftId: number;
  seniorId: number;
  caregiverId: number;
  submittedAt: string;
  isApprovedByFamily: boolean;
  riskScore: number;
}

export interface MedicationIntake {
  medicationId: number;
  name: string;
  wasTaken: boolean;
  reason?: string;
}

export interface ShiftReportResult extends ShiftReportListItem {
  mood: ObservationScale;
  appetite: ObservationScale;
  sleepQuality: ObservationScale;
  mobility: MobilityLevel;
  painLevel: number;
  bloodPressureSystolic?: number;
  bloodPressureDiastolic?: number;
  pulseRate?: number;
  temperature?: number;
  bloodSugar?: number;
  oxygenSaturation?: number;
  hasIncident: boolean;
  incidentKind?: IncidentType;
  incidentDescription?: string;
  notes?: string;
  medicationIntakes: MedicationIntake[];
}

export interface HealthTimelinePoint {
  date: string;
  riskScore: number;
  mood: ObservationScale;
}

export interface HealthRiskResult {
  currentRiskScore: number;
  trend: "Improving" | "Stable" | "Worsening";
}

const keys = {
  bySenior: (seniorId: number, q: PageQuery) => ["reports", "senior", seniorId, q] as const,
  detail: (id: number) => ["reports", id] as const,
  timeline: (seniorId: number) => ["reports", "senior", seniorId, "timeline"] as const,
  risk: (seniorId: number) => ["reports", "senior", seniorId, "risk"] as const,
};

export function useReportsBySenior(seniorId: number, q: PageQuery) {
  return useQuery({
    queryKey: keys.bySenior(seniorId, q),
    queryFn: async () =>
      (await apiClient.get<PagedResult<ShiftReportListItem>>(`/ShiftReports/Senior/${seniorId}`, { params: q })).data,
    enabled: !!seniorId,
  });
}

export function useShiftReport(id: number) {
  return useQuery({
    queryKey: keys.detail(id),
    queryFn: async () => (await apiClient.get<ShiftReportResult>(`/ShiftReports/${id}`)).data,
    enabled: !!id,
  });
}

export function useHealthRisk(seniorId: number) {
  return useQuery({
    queryKey: keys.risk(seniorId),
    queryFn: async () => (await apiClient.get<HealthRiskResult>(`/ShiftReports/Senior/${seniorId}/HealthRisk`)).data,
    enabled: !!seniorId,
  });
}

export function useApproveReport(id: number, seniorId: number) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: () => apiClient.put(`/ShiftReports/${id}/Approve`),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.detail(id) });
      qc.invalidateQueries({ queryKey: keys.bySenior(seniorId, {}) });
    },
  });
}
