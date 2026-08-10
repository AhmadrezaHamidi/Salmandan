import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Button, Card, CardTitle } from "@/components/ui";
import { Input, Select } from "@/components/ui/Input";
import { ErrorState } from "@/components/ui/Spinner";
import { useRequestShift } from "@/features/shifts/api";
import { useSenior } from "@/features/seniors/api";
import { apiClient } from "@/lib/apiClient";
import { extractErrorMessage } from "@/lib/apiClient";
import { careServiceTypeLabels, shiftKindLabels } from "@/types/enums";

const schema = z.object({
  agencyId: z.coerce.number().min(1, "آژانس را انتخاب کنید"),
  serviceType: z.enum(["Companionship", "PersonalCare", "ClinicalNursing", "Physiotherapy"]),
  shiftKind: z.enum(["Hourly", "Daily", "Night", "FullDay", "LiveIn"]),
  date: z.string().min(1, "تاریخ الزامی است"),
  startTime: z.string().min(1, "ساعت شروع الزامی است"),
  endTime: z.string().min(1, "ساعت پایان الزامی است"),
  familyNote: z.string().optional(),
});
type FormValues = z.infer<typeof schema>;

interface AgencyListItem { id: number; name: string; city: string }

export function ShiftRequestPage() {
  const [params] = useSearchParams();
  const seniorId = Number(params.get("seniorId"));
  const navigate = useNavigate();
  const { data: senior } = useSenior(seniorId);
  const mutation = useRequestShift();
  const { register, handleSubmit, watch, formState } = useForm<FormValues>({ resolver: zodResolver(schema) });
  const serviceType = watch("serviceType");

  const { data: agencies } = useQuery({
    queryKey: ["agencies", "available", senior?.city, serviceType],
    queryFn: async () =>
      (await apiClient.get<AgencyListItem[]>("/Agencies/Available", { params: { city: senior?.city, serviceType } })).data,
    enabled: !!senior?.city && !!serviceType,
  });

  async function onSubmit(values: FormValues) {
    const id = await mutation.mutateAsync({ ...values, seniorId });
    navigate(`/shifts/${id}`);
  }

  return (
    <div className="mx-auto max-w-lg">
      <Card>
        <CardTitle>درخواست شیفت {senior ? `برای ${senior.fullName}` : ""}</CardTitle>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Select
            label="نوع خدمت"
            options={Object.entries(careServiceTypeLabels).map(([value, label]) => ({ value, label }))}
            {...register("serviceType")}
          />
          <Select
            label="آژانس"
            placeholder={agencies ? "انتخاب آژانس" : "ابتدا نوع خدمت را انتخاب کنید"}
            options={(agencies ?? []).map((a) => ({ value: String(a.id), label: a.name }))}
            {...register("agencyId")}
          />
          <Select
            label="نوع شیفت"
            options={Object.entries(shiftKindLabels).map(([value, label]) => ({ value, label }))}
            {...register("shiftKind")}
          />
          <Input label="تاریخ" type="date" {...register("date")} error={formState.errors.date?.message} />
          <div className="grid grid-cols-2 gap-3">
            <Input label="ساعت شروع" type="time" {...register("startTime")} error={formState.errors.startTime?.message} />
            <Input label="ساعت پایان" type="time" {...register("endTime")} error={formState.errors.endTime?.message} />
          </div>
          <Input label="یادداشت (اختیاری)" {...register("familyNote")} />
          {mutation.isError && <ErrorState message={extractErrorMessage(mutation.error)} />}
          <Button type="submit" className="w-full" loading={mutation.isPending}>
            ثبت درخواست
          </Button>
        </form>
      </Card>
    </div>
  );
}
