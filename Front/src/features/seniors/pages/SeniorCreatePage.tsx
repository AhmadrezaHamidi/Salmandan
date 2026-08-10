import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useNavigate } from "react-router-dom";
import { Button, Card, CardTitle } from "@/components/ui";
import { Input, Select } from "@/components/ui/Input";
import { ErrorState } from "@/components/ui/Spinner";
import { useCreateSenior } from "@/features/seniors/api";
import { dependencyLevelLabels } from "@/types/enums";
import { extractErrorMessage } from "@/lib/apiClient";

const schema = z.object({
  fullName: z.string().min(2, "نام الزامی است"),
  nationalId: z.string().regex(/^\d{10}$/).optional().or(z.literal("")),
  birthDate: z.string().min(1, "تاریخ تولد الزامی است"),
  gender: z.enum(["Male", "Female"]),
  dependencyLevel: z.enum(["Independent", "PartiallyDependent", "Dependent", "FullyDependent"]),
  city: z.string().min(1, "شهر الزامی است"),
  district: z.string().optional(),
  address: z.string().min(1, "آدرس الزامی است"),
  notes: z.string().optional(),
});
type FormValues = z.infer<typeof schema>;

export function SeniorCreatePage() {
  const navigate = useNavigate();
  const mutation = useCreateSenior();
  const { register, handleSubmit, formState } = useForm<FormValues>({ resolver: zodResolver(schema) });

  async function onSubmit(values: FormValues) {
    const id = await mutation.mutateAsync(values);
    navigate(`/seniors/${id}`);
  }

  return (
    <div className="mx-auto max-w-lg">
      <Card>
        <CardTitle>افزودن سالمند</CardTitle>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Input label="نام کامل" {...register("fullName")} error={formState.errors.fullName?.message} />
          <Input label="کد ملی (اختیاری)" dir="ltr" {...register("nationalId")} error={formState.errors.nationalId?.message} />
          <Input label="تاریخ تولد" type="date" {...register("birthDate")} error={formState.errors.birthDate?.message} />
          <Select
            label="جنسیت"
            options={[{ value: "Male", label: "مرد" }, { value: "Female", label: "زن" }]}
            {...register("gender")}
          />
          <Select
            label="سطح وابستگی"
            options={Object.entries(dependencyLevelLabels).map(([value, label]) => ({ value, label }))}
            {...register("dependencyLevel")}
          />
          <Input label="شهر" {...register("city")} error={formState.errors.city?.message} />
          <Input label="منطقه (اختیاری)" {...register("district")} />
          <Input label="آدرس" {...register("address")} error={formState.errors.address?.message} />
          <Input label="یادداشت (اختیاری)" {...register("notes")} />
          {mutation.isError && <ErrorState message={extractErrorMessage(mutation.error)} />}
          <Button type="submit" className="w-full" loading={mutation.isPending}>
            ثبت
          </Button>
        </form>
      </Card>
    </div>
  );
}
